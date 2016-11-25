using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;



namespace Roslyn
{
    public static class MethodsGenerator
    {

        public static SyntaxNode GenerateMethod(SyntaxGenerator generator, string createName, SyntaxNode[] body = null)
        {
            return generator.MethodDeclaration(createName, null,
                null, null,
                Accessibility.Public,
                DeclarationModifiers.None,
                body ?? new SyntaxNode[] {});
        }

        public static SyntaxNode GetReturnPart(SyntaxGenerator generator, SoftwareCell softwareCell)
        {
            var outputStream = softwareCell.OutputStreams.First();
            return DataStreamParser.GetOutputPart(outputStream.DataNames)
                .Select(nametype => DataTypeParser.ConvertToTypeExpression(generator, nametype))
                .First();
        }

        public static SyntaxNode GenerateMethod(SyntaxGenerator generator,SoftwareCell softwareCell)
        {
            var methodName = GetMethodName(softwareCell);
            var returntype = GetReturnPart(generator,softwareCell);
            var parameters = GetParameters(generator,softwareCell);

            return generator.MethodDeclaration(methodName, parameters,
                null, returntype,
                Accessibility.Public,
                DeclarationModifiers.None,
                new SyntaxNode[] {});
        }

        public static IEnumerable<SyntaxNode> GetParameters(SyntaxGenerator generator, SoftwareCell softwareCell)
        {
            if (!softwareCell.InputStreams.Any())
                return null;

            var inputDataNames = softwareCell.InputStreams.First().DataNames;
            var i = 0;
            var nametypes = DataStreamParser.GetInputPart(inputDataNames);
            return nametypes
                .Where(nameType => DataTypeParser.ConvertToTypeExpression(generator,nameType.Type) != null)
                .Select(nametype =>
                {
                    ++i;
                    var name = GenerateParameterName(nametype, i);
                    var typeExpression = DataTypeParser.ConvertToTypeExpression(generator, nametype);                
                    return generator.ParameterDeclaration(name, typeExpression);
                }).ToArray();
        }


        private static string GenerateParameterName(NameType nametype, int i)
        {
            var lower = nametype.Type.ToLower();
            if (nametype.IsArray || nametype.IsList)
            {
                lower += "s";
            }
            return nametype.Name ?? lower;
        }

        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        public static string GetMethodName(SoftwareCell softwareCell)
        {
            return softwareCell.Name.Split(' ').Where(s => !string.IsNullOrEmpty(s)).Select(s => FirstCharToUpper(s)).Aggregate((s, s2) => s + s2);
        }
    }
}