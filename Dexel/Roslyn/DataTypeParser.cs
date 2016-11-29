using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn
{
    public static class DataTypeParser
    {


        public static SyntaxNode ConvertToTypeExpression(SyntaxGenerator generator, string type)
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
            var singletype = ConvertToTypeExpression(generator, nametype.Type);
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


        public static SyntaxNode ConvertToTypeExpression(SyntaxGenerator generator, IEnumerable<NameType> nametypes)
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
                        generator.GenericName("IEnumerable",
                            generator.GenericName("Tupel",
                                generator.IdentifierName(
                                    alltypes.Select(nt => ConvertNameTypeToTypeExpression(generator, nt).ToFullString())
                                        .Aggregate((f, s) => f + "," + s))));

                }
                return generator.GenericName("IEnumerable", ConvertNameTypeToTypeExpression(generator, alltypes.First()));

            }
            if (alltypes.Count > 1)
            {
                generator.GenericName("Tupel",
                    generator.IdentifierName(
                        alltypes.Select(nt => ConvertNameTypeToTypeExpression(generator, nt).ToFullString())
                            .Aggregate((f, s) => f + "," + s)));
            }
            else
            {
                return ConvertNameTypeToTypeExpression(generator, alltypes.First());
            }

            return null;
        }


        public static void OutputIsStream(SoftwareCell softwareCell, Action isStream = null, Action isNotStream = null)
        {
            DataStreamParser.IsStream(softwareCell.OutputStreams.First().DataNames, isStream, isNotStream);
        }
    }
}