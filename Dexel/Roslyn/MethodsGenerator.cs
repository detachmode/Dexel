using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;



namespace Roslyn
{
    public static class MethodsGenerator
    {

        public static SyntaxNode GenerateStaticMethod(SyntaxGenerator generator, string createName, SyntaxNode[] body = null)
        {
            return generator.MethodDeclaration(createName, null,
                null, null,
                Accessibility.Public,
                DeclarationModifiers.Static,
                body ?? new SyntaxNode[] {});
        }

        public static SyntaxNode GetReturnPart(SyntaxGenerator generator, SoftwareCell softwareCell)
        {
            var outputStream = softwareCell.OutputStreams.First();
            return DataStreamParser.GetOutputPart(outputStream.DataNames)
                .Select(nametype => DataTypeParser.ConvertToTypeExpression(generator, nametype))
                .First();
        }

        public static SyntaxNode GenerateStaticMethod(SyntaxGenerator generator,SoftwareCell softwareCell , SyntaxNode[] body = null)
        {
            var methodName = GetMethodName(softwareCell);
            var returntype = GetReturnPart(generator,softwareCell);
            var parameters = GetParameters(generator,softwareCell);

            return generator.MethodDeclaration(methodName, parameters,
                null, returntype,
                Accessibility.Public,
                DeclarationModifiers.Static,
                body ?? new SyntaxNode[] { });
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
                    var name = GenerateParameterName(nametype);
                    var typeExpression = DataTypeParser.ConvertToTypeExpression(generator, nametype);                
                    return generator.ParameterDeclaration(name, typeExpression);
                }).ToArray();
        }


        public static string GenerateParameterName(NameType nametype)
        {
            var lower = nametype.Type.ToLower();
            if (nametype.IsArray || nametype.IsList)
            {
                lower += "s";
            }
            return nametype.Name ?? lower;
        }


        public static string GetMethodName(SoftwareCell softwareCell)
        {
            if (string.IsNullOrEmpty(softwareCell.Name))
            {
                throw new Exception("SoftwareCell has no name");
            }
            return softwareCell.Name.Split(' ').Where(s => !string.IsNullOrEmpty(s)).Select(s => Helper.FirstCharToUpper(s)).Aggregate((s, s2) => s + s2);
        }


        public static SyntaxNode[] GetNotImplementatedException(SyntaxGenerator generator)
        {
            return new []
            {
                generator.ThrowStatement(generator.IdentifierName(" new NotImplementedException()"))
            };
        }
    }
}