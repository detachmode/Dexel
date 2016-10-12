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
    }
}