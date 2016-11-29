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
                body ?? new SyntaxNode[] { });
        }


        public static SyntaxNode GetReturnPart(SyntaxGenerator generator, SoftwareCell softwareCell)
        {
            SyntaxNode result = null;

            DataTypeParser.OutputIsStream(softwareCell,
                isStream: () => { },
                isNotStream: () =>
                {
                    var outputStream = softwareCell.OutputStreams.First();
                    result = DataTypeParser.ConvertToTypeExpression(generator,
                        DataStreamParser.GetOutputPart(outputStream.DataNames));
                });

            return result;
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
                body ?? new SyntaxNode[] { });
        }


        public static IEnumerable<SyntaxNode> GetParameters(SyntaxGenerator generator, SoftwareCell softwareCell)
        {
            var result = new List<SyntaxNode>();
            DataTypeParser.OutputIsStream(softwareCell,
                isStream: () => { MethodParameterSignatureForStreamOutput(generator, softwareCell, result); },
                isNotStream: () => MethodParameterSignatureFromInputs(generator, softwareCell, result));

            return result;
        }


        private static void MethodParameterSignatureForStreamOutput(SyntaxGenerator generator, SoftwareCell softwareCell,
            List<SyntaxNode> result)
        {
            MethodParameterSignatureFromInputs(generator, softwareCell, result);
            var outgoingDataNames = softwareCell.OutputStreams.First().DataNames;
            var nametypes = DataStreamParser.GetOutputPart(outgoingDataNames);
            nametypes.Where(nt => nt != null).ToList().ForEach(nt =>
                {
                    var name = nt.Name != null ? $"on{Helper.FirstCharToUpper(nt.Name)}" : $"on{nt.Type}";
                    var typeExpression = generator.IdentifierName($"Action<{nt.Type}>");
                    result.Add(generator.ParameterDeclaration(name, typeExpression));
                });
        }


        private static void MethodParameterSignatureFromInputs(SyntaxGenerator generator, SoftwareCell softwareCell,
            List<SyntaxNode> result)
        {
            if (!softwareCell.InputStreams.Any())
                return;

            var inputDataNames = softwareCell.InputStreams.First().DataNames;
            var i = 0;
            var nametypes = DataStreamParser.GetInputPart(inputDataNames);
            nametypes
                .Where(nameType => DataTypeParser.ConvertToTypeExpression(generator, nameType.Type) != null)
                .ToList().ForEach(nametype =>
                {
                    ++i;
                    var name = GenerateParameterName(nametype);
                    var typeExpression = DataTypeParser.ConvertNameTypeToTypeExpression(generator, nametype);
                    result.Add(generator.ParameterDeclaration(name, typeExpression));
                });
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
                generator.ThrowStatement(generator.IdentifierName(" new NotImplementedException()"))
            };
        }
    }

}