using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn.Parser
{
    public static class TypeConverter
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
            if (nametype.IsArray)
                return generator.ArrayTypeExpression(singletype);
            if (nametype.IsList && nametype.IsArray == false)
                return generator.GenericName("IEnumerable", singletype);

            return singletype;

        }




        public static SyntaxNode ConvertToType(SyntaxGenerator generator, IEnumerable<NameType> nametypes)
        {
            SyntaxNode @return = null;


            AnalyseTypes(nametypes,
                moreThanOne: types =>
                {
                    var convertedtypes = types.Select(nt => ConvertNameTypeToTypeExpression(generator, nt));
                    @return = GenerateTupel(generator, convertedtypes);
                },
                isSingleType: t =>
                {
                    @return = ConvertNameTypeToTypeExpression(generator, t);
                });

            return @return;
        }


        private static SyntaxNode GenerateTupel(SyntaxGenerator generator, IEnumerable<SyntaxNode> types)
        {
            return generator.GenericName("Tupel", types.ToArray());
        }

        private static void AnalyseTypes(IEnumerable<NameType> nametypes, 
            Action<NameType>  isSingleType, Action<IEnumerable<NameType>> moreThanOne)
        {
            var alltypes = nametypes.ToList();


            
            if (alltypes.Count == 0)
                return;


            if (alltypes.Count > 1)
                moreThanOne(alltypes);
            else
                isSingleType(alltypes.First());
            



        }



        public static void OutputOrInputIsStream(FunctionUnit functionUnit, Action bothAreStreams = null, Action onInputIsStream = null, Action onOutputIsStream = null, Action noStream = null)
        {
            var outputIsStream = false;
            var inputIsStream = false;
            DataStreamParser.CheckIsStream(functionUnit.OutputStreams.First().DataNames, () => outputIsStream = true);
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