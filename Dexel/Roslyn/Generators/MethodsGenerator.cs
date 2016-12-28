using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Roslyn.Parser;

namespace Roslyn.Generators
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


        public static SyntaxNode GetReturnPart(SyntaxGenerator generator, FunctionUnit functionUnit, bool isNullable)
        {

            var signature = DataStreamParser.AnalyseOutputs(functionUnit);
            var returnSignature = signature.FirstOrDefault(sig => sig.ImplementWith == DataFlowImplementationStyle.AsReturn);

            return returnSignature != null ? 
                TypeConverter.ConvertToType(generator, DataStreamParser.GetOutputPart(returnSignature.DSD.DataNames), isNullable)  
                : null;
        }




        public static SyntaxNode GenerateStaticMethod(SyntaxGenerator generator, FunctionUnit functionUnit, SyntaxNode[] body = null,
            bool nullableReturn = false)
        {
            var methodName = GetMethodName(functionUnit);
            var returntype = GetReturnPart(generator, functionUnit, nullableReturn);
            var parameters = GetParameters(generator, functionUnit);

            return MethodDeclaration(generator, body, methodName, parameters, returntype);
        }


        public static SyntaxNode MethodDeclaration(SyntaxGenerator generator, SyntaxNode[] body, string methodName, IEnumerable<SyntaxNode> parameters, SyntaxNode returntype)
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

            var outputSignature = DataStreamParser.AnalyseOutputs(functionUnit);

            outputSignature
                .Where( sig => sig.ImplementWith != DataFlowImplementationStyle.AsReturn).ToList()
                .ForEach( sig => MakeActionSignature(generator, sig, result.Add));

            return result;
        }

        private static void MakeActionSignature(SyntaxGenerator generator, MethodSignaturePart sig, Action<SyntaxNode> onSyntaxNode)
        {
            var nametypes = DataStreamParser.GetOutputPart(sig.DSD.DataNames);

            var nameOfAction = GetNameOfAction(sig.DSD);
            if (nametypes.Count == 0)
            {
                onSyntaxNode(generator.ParameterDeclaration(nameOfAction, generator.IdentifierName("Action")));
            }
            else
            {
                var types = nametypes.Select(nt => TypeConverter.ConvertNameTypeToTypeExpression(generator, nt));
                var typeExpression = generator.GenericName("Action", types);
                onSyntaxNode(generator.ParameterDeclaration(nameOfAction, typeExpression));
            }

           
        }


        public static string GetNameOfAction(DataStreamDefinition dsd)
        {
            string @return = null;
            IntegrationGenerator.IsActionNameDefined(dsd, 
                onDefined: () => @return = dsd.ActionName.Replace(".", string.Empty),
                onUndefined: () => @return = GenerateNameOfActionByTypes(dsd.DataNames));

            return @return;
        }


        private static string GenerateNameOfActionByTypes(string rawdatanames)
        {
            var nametypes = DataStreamParser.GetOutputPart(rawdatanames);
            if (nametypes.Count == 1)
            {
                var nt = nametypes.First();
                if (string.IsNullOrWhiteSpace(nt.Name))
                    return  $"on{Helper.FirstCharToUpper(nt.Type)}";
                return $"on{Helper.FirstCharToUpper(nt.Name)}";
            }
            return  "continueWith";

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
                    var typeExpression = TypeConverter.ConvertNameTypeToTypeExpression(generator, nametype);
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
                generator.ThrowStatement(
                   generator.ObjectCreationExpression(
                       generator.IdentifierName("NotImplementedException")))
            };
        }
    }

}