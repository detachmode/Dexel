using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn.Parser
{
    public static class DataTypeParser
    {


        public static SyntaxNode ConvertToType(SyntaxGenerator generator, string type)
        {
            if (type.ToLower() == "datetime")   // bug in roslyn?        
                return generator.IdentifierName("DateTime");

            var res = generator.IdentifierName(type);
            
            Convert(type, specialType => res = generator.TypeExpression(specialType));
            return res;

        }

        private static void Convert(string type, Action<SpecialType> onConverted)
        {
            switch (type)
            {
                case "bool":
                    onConverted(SpecialType.System_Boolean);
                    return;
                case "int":
                    onConverted(SpecialType.System_Int32);
                    return;

            }

            SpecialType outType;
            var success = Enum.TryParse($"system_{type}", true, out outType);
            if (success)
                onConverted(outType);
        }

        public static bool IsSystemType(string type)
        {
            bool success = false;
            Convert(type, specialType => success = true);
            return success;
        }


        public static SyntaxNode ConvertNameTypeToTypeExpression(SyntaxGenerator generator, NameType nametype)
        {
            var singletype = ConvertToType(generator, nametype.Type);
            //if (nametype.IsInsideStream)
            //{
            //    if (nametype.IsList == false && nametype.IsArray == false)
            //        return generator.GenericName("IEnumerable", singletype);
            //    if (nametype.IsList == true && nametype.IsArray == false)
            //        return generator.GenericName("IEnumerable", generator.GenericName("List", singletype));
            //    if (nametype.IsList == false && nametype.IsArray == true)
            //        return generator.GenericName("IEnumerable", generator.ArrayTypeExpression(singletype));
            //}

            if (nametype.IsArray)
                return generator.ArrayTypeExpression(singletype);
            if (nametype.IsList && nametype.IsArray == false)
                return generator.GenericName("List", singletype);

            return singletype;

        }


        public static SyntaxNode ConvertToType(SyntaxGenerator generator, IEnumerable<NameType> nametypes)
        {
            var alltypes = nametypes.ToList();
            if (alltypes.Count == 0)
            {
                return null;
            }
            if (alltypes.Any(nt => nt.IsInsideStream))
            {
                if (alltypes.Count > 1)
                {
                    return
                            generator.GenericName("Tupel",
                                generator.IdentifierName(
                                    alltypes.Select(nt => ConvertNameTypeToTypeExpression(generator, nt).ToFullString())
                                        .Aggregate((f, s) => f + "," + s)));

                }
                return ConvertNameTypeToTypeExpression(generator, alltypes.First());

            }
            if (alltypes.Count > 1)
            {
                return generator.GenericName("Tupel",
                    generator.IdentifierName(
                        alltypes.Select(nt => ConvertNameTypeToTypeExpression(generator, nt).ToFullString())
                            .Aggregate((f, s) => f + "," + s)));
            }


            return ConvertNameTypeToTypeExpression(generator, alltypes.First());

        }


        public enum DataFlowImplementationStyle
        {
            AsAction,
            AsReturn
        }

        public class MethodSignaturePart
        {
            public DataFlowImplementationStyle ImplementWith;
            public string Datanames;
            public string ActionNames;
        }

        public static List<MethodSignaturePart> AnalyseOutputs(FunctionUnit functionUnit)
        {
            var result = new List<MethodSignaturePart>();

            var copyOfOutputs = functionUnit.OutputStreams.ToList();
            OutputByReturn(functionUnit, dsdByReturn =>
            {
                result.Add(new MethodSignaturePart
                {
                    Datanames = dsdByReturn.DataNames,
                    ImplementWith = DataFlowImplementationStyle.AsReturn
                });
                copyOfOutputs.Remove(dsdByReturn);
            });

            copyOfOutputs.ForEach(dsd =>
            {
                result.Add(new MethodSignaturePart
                {
                    Datanames = dsd.DataNames,
                    ActionNames =  dsd.ActionName,
                    ImplementWith = DataFlowImplementationStyle.AsAction
                });
            });

            return result;
        }


        private static void OutputByReturn(FunctionUnit functionUnit, Action<DataStreamDefinition> onFound)
        {
            var noActionsnames = functionUnit.OutputStreams.Where(dsd => string.IsNullOrWhiteSpace(dsd.ActionName)).ToList();
            if (noActionsnames.Count == 1)
            {
                DataStreamParser.CheckIsStream(noActionsnames.First().DataNames,
                    isNotStream: () =>
                    {
                        onFound(noActionsnames.First());
                    },
                    isStream: () =>
                    {
                        DataStreamParser.CheckIsStream(functionUnit.InputStreams.First().DataNames,
                            isStream: () => onFound(noActionsnames.First()));
                    });
            }
        }


        public static void OutputOrInputIsStream(FunctionUnit functionUnit, Action bothAreStreams = null, Action onInputIsStream = null, Action onOutputIsStream = null, Action noStream = null )
        {
            var outputIsStream = false;
            var inputIsStream = false;
            DataStreamParser.CheckIsStream(functionUnit.OutputStreams.First().DataNames, () => outputIsStream = true );
            DataStreamParser.CheckIsStream(functionUnit.InputStreams.First().DataNames, () => inputIsStream = true);

            if (outputIsStream && inputIsStream)
                bothAreStreams?.Invoke();
            else if (inputIsStream)
                onInputIsStream?.Invoke();
            else if (outputIsStream)
                onOutputIsStream?.Invoke();
            else
                noStream?.Invoke();
        }
    }
}