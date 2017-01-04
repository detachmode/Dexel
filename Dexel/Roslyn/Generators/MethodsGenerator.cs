using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Roslyn.Analyser;
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

            var signature = OutputAnalyser.AnalyseOutputs(functionUnit);
            var returnSignature = signature.FirstOrDefault(sig => sig.ImplementWith == DataFlowImplementationStyle.AsReturn);

            return returnSignature != null ? 
                TypeConverter.ConvertToType(generator, DataStreamParser.GetOutputPart(returnSignature.DSD.DataNames), isNullable)  
                : null;
        }




        public static SyntaxNode GenerateStaticMethod(SyntaxGenerator generator, FunctionUnit functionUnit, SyntaxNode[] body = null,
            bool nullableReturn = false)
        {
            var methodName = Names.MethodName(functionUnit);
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

            var outputSignature = OutputAnalyser.AnalyseOutputs(functionUnit);

            outputSignature
                .Where( sig => sig.ImplementWith != DataFlowImplementationStyle.AsReturn).ToList()
                .ForEach( sig => MakeActionSignature(generator, sig, result.Add));

            return result;
        }

        private static void MakeActionSignature(SyntaxGenerator generator, MethodSignaturePart sig, Action<SyntaxNode> onSyntaxNode)
        {
            var nametypes = DataStreamParser.GetOutputPart(sig.DSD.DataNames);

            var nameOfAction = Names.NewAction(sig.DSD);
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


        private static void MethodParameterSignatureFromInputs(SyntaxGenerator generator, FunctionUnit functionUnit,
           Action<SyntaxNode> onSyntaxNode)
        {
            if (!functionUnit.InputStreams.Any())
                return;

            var inputDataNames = functionUnit.InputStreams.First().DataNames;
            var nametypes = DataStreamParser.GetInputPart(inputDataNames);
            nametypes.ToList().ForEach(nametype =>
                {
                    var name = Names.ParameterName(nametype);
                    var typeExpression = TypeConverter.ConvertNameTypeToTypeExpression(generator, nametype);
                    onSyntaxNode(generator.ParameterDeclaration(name, typeExpression));
                });
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