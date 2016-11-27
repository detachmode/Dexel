using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model.DataTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn
{
    public static class DataTypesGenerator
    {
        public static IEnumerable<SyntaxNode> GenerateFields(SyntaxGenerator generator, DataType dataType)
        {
            return dataType.DataTypes.Select(fieldDt => Helper.TryCatch(() => 
                    FieldDeclaration(generator, fieldDt),
                    errormsg: $"Couldn't generate field of data type {dataType.Name}"));
        }


        private static SyntaxNode FieldDeclaration(SyntaxGenerator generator, DataType dt)
        {
            return generator.FieldDeclaration(name: Helper.FirstCharToUpper(dt.Name),
                type: DataTypeParser.ConvertToTypeExpression(generator, dt.Type),
                accessibility: Accessibility.Public);
        }
    }
}