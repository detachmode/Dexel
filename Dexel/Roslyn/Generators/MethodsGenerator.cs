using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
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


        public static SyntaxNode GetReturnPart(SyntaxGenerator generator, FunctionUnit functionUnit)
        {
            SyntaxNode result = null;

            DataTypeParser.AnalyseOutputs(functionUnit,
                isComplexOutput: () => { },
                isSimpleOutput: () =>
                {
                    var outputStream = functionUnit.OutputStreams.First();
                    result = DataTypeParser.ConvertToType(generator,
                        DataStreamParser.GetOutputPart(outputStream.DataNames));
                });

            return result;
        }


        public static SyntaxNode GenerateStaticMethod(SyntaxGenerator generator, FunctionUnit functionUnit,
            SyntaxNode[] body = null)
        {
            var methodName = GetMethodName(functionUnit);
            var returntype = GetReturnPart(generator, functionUnit);
            var parameters = GetParameters(generator, functionUnit);

            return MethodDeclaration(generator, body, methodName, parameters, returntype);
        }


        private static SyntaxNode MethodDeclaration(SyntaxGenerator generator, SyntaxNode[] body, string methodName, IEnumerable<SyntaxNode> parameters, SyntaxNode returntype)
        {
            return generator.MethodDeclaration(methodName, parameters,
                null, returntype,
                Accessibility.Public,
                DeclarationModifiers.Static,
                statements: body ?? new SyntaxNode[] {});
        }


        public static IEnumerable<SyntaxNode> GetParameters(SyntaxGenerator generator, FunctionUnit functionUnit)
        {
            var result = new List<SyntaxNode>();
            DataTypeParser.AnalyseOutputs(functionUnit,
                isComplexOutput: () => MethodParameterSignatureForComplexOutput(generator, functionUnit, result),
                isSimpleOutput: () => MethodParameterSignatureFromInputs(generator, functionUnit, result));

            return result;
        }


        private static void MethodParameterSignatureForComplexOutput(SyntaxGenerator generator, FunctionUnit functionUnit,
            List<SyntaxNode> result)
        {
            MethodParameterSignatureFromInputs(generator, functionUnit, result);
            functionUnit.OutputStreams.ToList().ForEach(dsd =>
            {
                var outgoingDataNames = dsd.DataNames;
                var outgoingActionName = dsd.ActionName;
                var nametypes = DataStreamParser.GetOutputPart(outgoingDataNames);
                nametypes.Where(nt => nt != null).ToList().ForEach(nt =>
                {
                    var name = GetNameOfStream(nt, outgoingActionName);
                    var typeExpression = generator.IdentifierName($"Action<{nt.Type}>");
                    result.Add(generator.ParameterDeclaration(name, typeExpression));
                });
            });
        }


        private static string GetNameOfStream(NameType nt, string outgoingActionName)
        {
            return outgoingActionName?.Replace(".", string.Empty) ?? $"on{nt.Type}";
        }


        private static void MethodParameterSignatureFromInputs(SyntaxGenerator generator, FunctionUnit functionUnit,
            List<SyntaxNode> result)
        {
            if (!functionUnit.InputStreams.Any())
                return;

            var inputDataNames = functionUnit.InputStreams.First().DataNames;
            var nametypes = DataStreamParser.GetInputPart(inputDataNames);
            nametypes.ToList().ForEach(nametype =>
                {
                    var name = GenerateParameterName(nametype);
                    var typeExpression = DataTypeParser.ConvertNameTypeToTypeExpression(generator, nametype);
                    result.Add(generator.ParameterDeclaration(name, typeExpression));
                });
        }


        public static string GenerateParameterName(NameType nametype)
        {
            var lower = nametype.Type.ToLower();
            if (nametype.IsArray || nametype.IsList)
                lower += "s";
            return nametype.Name ?? $"a{lower}";
        }


        public static string GetMethodName(FunctionUnit functionUnit)
        {
            if (string.IsNullOrEmpty(functionUnit.Name))
                throw new Exception("FunctionUnit has no name");
           
            return
                functionUnit.Name.Split(' ')
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(Helper.FirstCharToUpper)
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