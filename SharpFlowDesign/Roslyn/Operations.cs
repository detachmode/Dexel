using System.Collections.Generic;
using System.Linq;
using FlowDesignModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;



namespace Roslyn
{
    public static class Operations
    {



        public static SyntaxNode GenerateOperationMethod(SyntaxGenerator generator, string createName, SyntaxNode[] body = null)
        {
            return generator.MethodDeclaration(createName, null,
                null, null,
                Accessibility.Public,
                DeclarationModifiers.None,
                body ?? new SyntaxNode[] {});
        }

        public static SyntaxNode GetReturnPart(SyntaxGenerator generator, SoftwareCell softwareCell)
        {
            var outputDataNames = softwareCell.OutputStreams.First().DataNames;
            return DataStreamParser.ParseDataNames(outputDataNames, pipePart: 1)
                .Select(nameType => DataTypeParser.ConvertToTypeExpression(generator, nameType.Type))
                .First();
        }

        public static SyntaxNode GenerateOperationMethod(SyntaxGenerator generator,SoftwareCell softwareCell)
        {
            var methodName = GetMethodName(softwareCell);
            var returntype = GetReturnPart(generator,softwareCell);
            var parameters = GetParameters(generator,softwareCell);

            ////var constructorParameters = new SyntaxNode[] {
            //      _generator.ParameterDeclaration("LastName",
            //      _generator.TypeExpression(SpecialType.System_String)),


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
            return DataStreamParser.ParseDataNames(inputDataNames, pipePart: 2)
                .Where(nameType => DataTypeParser.ConvertToTypeExpression(generator,nameType.Type) != null)
                .Select(nametype =>
                {
                    ++i;
                    var name = nametype.Name ?? "param" + i;                    
                    return generator.ParameterDeclaration(name, DataTypeParser.ConvertToTypeExpression(generator,nametype.Type));
                }).ToArray();
        }

        public static string GetMethodName(SoftwareCell softwareCell)
        {
            return softwareCell.Name.Replace(' ', '_');
        }
    }
}