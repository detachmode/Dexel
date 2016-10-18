using System.Collections.Generic;
using System.Linq;
using Dexel.Contracts.Model;
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

        public static SyntaxNode GetReturnPart(SyntaxGenerator generator, ISoftwareCell softwareCell)
        {
            var outputStream = softwareCell.OutputStreams.First();
            return DataStreamParser.GetOutputPart(outputStream.DataNames)
                .Select(nametype => DataTypeParser.ConvertToTypeExpression(generator, nametype))
                .First();
        }

        public static SyntaxNode GenerateMethod(SyntaxGenerator generator,ISoftwareCell softwareCell)
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

        public static IEnumerable<SyntaxNode> GetParameters(SyntaxGenerator generator, ISoftwareCell softwareCell)
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
            return nametype.Name ?? "param" + i;
        }


        public static string GetMethodName(ISoftwareCell softwareCell)
        {
            return softwareCell.Name.Replace(' ', '_');
        }
    }
}