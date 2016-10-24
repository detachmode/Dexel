using Dexel.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn
{
    public static class DataTypeParser
    {


        public static SyntaxNode ConvertToTypeExpression(SyntaxGenerator generator,string type)
        {
            switch (type)
            {
                case "":
                    return null;
                case "string":
                    return generator.TypeExpression(SpecialType.System_String);
                case "int":
                    return generator.TypeExpression(SpecialType.System_Int32);
                case "double":
                    return generator.TypeExpression(SpecialType.System_Double);

                default:
                    return generator.IdentifierName(type);
            }
        }




        public static SyntaxNode ConvertToTypeExpression(SyntaxGenerator generator, NameType nametype)
        {
            var singletype = ConvertToTypeExpression(generator, nametype.Type);
            if (nametype.IsInsideStream)
            {
                if (nametype.IsList == false && nametype.IsArray == false)
                    return generator.GenericName("IEnumerable", singletype);
                if (nametype.IsList == true && nametype.IsArray == false)
                    return generator.GenericName("IEnumerable", generator.GenericName("List", singletype));
                if (nametype.IsList == false && nametype.IsArray == true)
                    return generator.GenericName("IEnumerable", generator.ArrayTypeExpression(singletype));
            }

            if (nametype.IsArray)
                return generator.ArrayTypeExpression(singletype);
            if (nametype.IsList && nametype.IsArray == false)
               return generator.GenericName("List", singletype);

            return singletype;

        }
    }
}