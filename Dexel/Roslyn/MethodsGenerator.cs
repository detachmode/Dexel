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
        public static SyntaxNode GenerateStaticMethod(SyntaxGenerator generator, string createName,
            SyntaxNode[] body = null)
        {
            return generator.MethodDeclaration(createName, null,
                null, null,
                Accessibility.Public,
                DeclarationModifiers.Static,
                statements: body ?? new SyntaxNode[] {});
        }


        public static SyntaxNode GetReturnPart(SyntaxGenerator generator, SoftwareCell softwareCell)
        {
            var outputStream = softwareCell.OutputStreams.First();
            return DataStreamParser.GetOutputPart(outputStream.DataNames)
                .Select(nametype => DataTypeParser.ConvertToTypeExpression(generator, nametype))
                .First();
        }


        public static SyntaxNode GenerateStaticMethod(SyntaxGenerator generator, SoftwareCell softwareCell,
            SyntaxNode[] body = null)
        {
            var methodName = GetMethodName(softwareCell);
            var returntype = GetReturnPart(generator, softwareCell);
            var parameters = GetParameters(generator, softwareCell);

            return generator.MethodDeclaration(methodName, parameters,
                null, returntype,
                Accessibility.Public,
                DeclarationModifiers.Static,
                statements: body ?? new SyntaxNode[] {});
        }


        public static SyntaxNode[] GetParameters(SyntaxGenerator generator, SoftwareCell softwareCell)
        {
            var resultSyntaxNodes = new List<SyntaxNode>();
            DetermineParameterGenerator(softwareCell, resultSyntaxNodes,
                nametypes => StreamParameter(generator, resultSyntaxNodes, nametypes),
                nametypes => NonStreamParameter(generator, resultSyntaxNodes, nametypes));

            return resultSyntaxNodes.ToArray();
        }


        public static void DetermineParameterGenerator(SoftwareCell softwareCell, List<SyntaxNode> result,
            Action<IEnumerable<NameType>> isStream, Action<IEnumerable<NameType>> isNotStream)
        {
            if (!softwareCell.InputStreams.Any())
                return;

            var inputDataNames = softwareCell.InputStreams.First().DataNames;
            var nametypes = DataStreamParser.GetInputPart(inputDataNames).ToList();

            if (nametypes.Any(x => x.IsInsideStream))
                isStream(nametypes);
            else
                isNotStream(nametypes);
        }


        public static void StreamParameter(SyntaxGenerator generator,
            List<SyntaxNode> resultSyntaxNodes, IEnumerable<NameType> nametypes)
        {
            throw new NotImplementedException();
        }


        private static void NonStreamParameter(SyntaxGenerator generator, List<SyntaxNode> resultSyntaxNodes,
            IEnumerable<NameType> nametypes)
        {
            nametypes
                .Where(nameType => DataTypeParser.ConvertToTypeExpression(generator, nameType.Type) != null).ToList()
                .ForEach(nametype =>
                {
                    var name = GenerateParameterName(nametype);
                    var typeExpression = DataTypeParser.ConvertToTypeExpression(generator, nametype);
                    resultSyntaxNodes.Add(item: generator.ParameterDeclaration(name, typeExpression));
                });
        }


        public static string GenerateParameterName(NameType nametype)
        {
            var lower = nametype.Type.ToLower();
            if (nametype.IsArray || nametype.IsList)
                lower += "s";
            return nametype.Name ?? lower;
        }


        public static string GetMethodName(SoftwareCell softwareCell)
        {
            if (string.IsNullOrEmpty(softwareCell.Name))
                throw new Exception("SoftwareCell has no name");
            return
                softwareCell.Name.Split(' ')
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => Helper.FirstCharToUpper(s))
                    .Aggregate((s, s2) => s + s2);
        }


        public static SyntaxNode[] GetNotImplementatedException(SyntaxGenerator generator)
        {
            return new[]
            {
                generator.ThrowStatement(expression: generator.IdentifierName(" new NotImplementedException()"))
            };
        }
    }
}