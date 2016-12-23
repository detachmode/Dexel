using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model.DataTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Roslyn.Parser;

namespace Roslyn
{
    public static class DataTypesGenerator
    {
        public static IEnumerable<SyntaxNode> GenerateFields(SyntaxGenerator generator, CustomDataType customDataType)
        {
            return customDataType.SubDataTypes.Select(fieldDt => Helper.TryCatch(() => 
                    FieldDeclaration(generator, fieldDt),
                    errormsg: $"Couldn't generate field of data type {customDataType.Name}"));
        }


        private static SyntaxNode FieldDeclaration(SyntaxGenerator generator, SubDataType dt)
        {
            return generator.FieldDeclaration(
                name: Helper.FirstCharToUpper(dt.Name),
                type: DataTypeParser.ConvertToType(generator, dt.Type),
                accessibility: Accessibility.Public);
        }
    }
}