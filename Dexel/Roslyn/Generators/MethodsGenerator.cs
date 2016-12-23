using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Roslyn.Parser;

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
                statements: body ?? new SyntaxNode[] { });
        }


        public static SyntaxNode GetReturnPart(SyntaxGenerator generator, FunctionUnit functionUnit)
        {

            var signature = DataTypeParser.AnalyseOutputs(functionUnit);
            var returnSignature = signature.FirstOrDefault(sig => sig.ImplementWith == DataTypeParser.DataFlowImplementationStyle.AsReturn);

            return returnSignature != null ? 
                DataTypeParser.ConvertToType(generator, nametypes: DataStreamParser.GetOutputPart(returnSignature.Datanames)) 
                : null;
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
                statements: body ?? new SyntaxNode[] { });
        }


        public static IEnumerable<SyntaxNode> GetParameters(SyntaxGenerator generator, FunctionUnit functionUnit)
        {
            var result = new List<SyntaxNode>();
            MethodParameterSignatureFromInputs(generator, functionUnit, result.Add);

            var outputSignature = DataTypeParser.AnalyseOutputs(functionUnit);

            outputSignature
                .Where( sig => sig.ImplementWith != DataTypeParser.DataFlowImplementationStyle.AsReturn).ToList()
                .ForEach( sig =>
                {
                    MakeActionSignature(generator, sig, result.Add);
                });

            return result;
        }

        private static void MakeActionSignature(SyntaxGenerator generator, DataTypeParser.MethodSignaturePart sig, Action<SyntaxNode> onSyntaxNode)
        {
            var nametypes = DataStreamParser.GetOutputPart(sig.Datanames);

            var name = GetNameOfAction(sig, nametypes);
            var types = string.Join(",", nametypes.Select(nt => nt.Type));
           var typeExpression = generator.IdentifierName($"Action<{types}>");
            onSyntaxNode(generator.ParameterDeclaration(name, typeExpression));

        }


        private static string GetNameOfAction(DataTypeParser.MethodSignaturePart sig, List<NameType> nametypes)
        {
            if (!string.IsNullOrWhiteSpace(sig.ActionNames))
            {
                return sig.ActionNames.Replace(".", string.Empty);
            }
            if (nametypes.Count == 1)
            {
                var nt = nametypes.First();

                if (string.IsNullOrWhiteSpace(nt.Name))
                    return $"on{Helper.FirstCharToUpper(nt.Type)}";
                return $"on{Helper.FirstCharToUpper(nt.Name)}";
            }
            return "continueWith";
        }

        private static void MethodParameterSignatureFromInputs(SyntaxGenerator generator, FunctionUnit functionUnit,
           Action<SyntaxNode> onSyntaxNode)
        {
            if (!functionUnit.InputStreams.Any())
                return;

            var inputDataNames = functionUnit.InputStreams.First().DataNames;
            var nametypes = DataStreamParser.GetInputPart(inputDataNames);
            nametypes.ToList().ForEach(nametype =>
                {
                    var name = GenerateParameterName(nametype);
                    var typeExpression = DataTypeParser.ConvertNameTypeToTypeExpression(generator, nametype);
                    onSyntaxNode(generator.ParameterDeclaration(name, typeExpression));
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