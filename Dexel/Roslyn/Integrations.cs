using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn
{
    public static class Integrations
    {
        private static int _methodsToGenerateCount;

        public static SyntaxNode[] CreateIntegrationBody(SyntaxGenerator generator, List<DataStream> connections, SoftwareCell integration)
        {
            _methodsToGenerateCount = 0;
            var generated = new List<GeneratedLocalVariable>();

            var methodWithParameterDependencieses = FindParameterDependencies(integration, connections);
            return CreateAllDependenciesAvailable(generator, integration, methodWithParameterDependencieses,
                generated, new List<SyntaxNode>()).ToArray();
        }


        private static List<SyntaxNode> CreateAllDependenciesAvailable(SyntaxGenerator generator, SoftwareCell integration, List<MethodWithParameterDependencies> methodsToGenerate, List<GeneratedLocalVariable> generated, List<SyntaxNode> result)
        {
            if (!methodsToGenerate.Any() || methodsToGenerate.Count == _methodsToGenerateCount)
                return result;

            // to detect if no more methods can be generated 
            _methodsToGenerateCount = methodsToGenerate.Count;

            var nodes = methodsToGenerate
                .Where(methodWithParameterDependencies => CanBeGenerated(generated, methodWithParameterDependencies))
                .Select(methodWithParameterDependencies =>
                       GenerateLocalMethodCall(generator, generated, methodWithParameterDependencies)).ToList();

            result.AddRange(nodes);
            methodsToGenerate.RemoveAll(x => generated.Any(y => y.Source == x.OfSoftwareCell));

            return CreateAllDependenciesAvailable(generator, integration, methodsToGenerate, generated, result);
        }


        private static bool CanBeGenerated(List<GeneratedLocalVariable> generated,
            MethodWithParameterDependencies methodWithParameterDependencies)
        {
            return methodWithParameterDependencies.Parameters.TrueForAll(
                    param => param.Source == null || generated.Any(c => c.Source == param.Source));
        }


        private static SyntaxNode GenerateLocalMethodCall(SyntaxGenerator generator, List<GeneratedLocalVariable> generated,
            MethodWithParameterDependencies x)
        {
            return LocalMethodCall(generator, x.OfSoftwareCell,
                CreateParameterAssignment(generator, generated, x.Parameters),
                generated);
        }


        private static SyntaxNode[] CreateParameterAssignment(SyntaxGenerator generator,
            List<GeneratedLocalVariable> generated, List<Parameter> parameters)
        {
            return parameters.Select(p =>
            {
                var variablename = p.Source == null ? MethodsGenerator.GenerateParameterName(p.NameType)
                    : generated.First(x => x.Source == p.Source).VariableName;

                return generator.IdentifierName(variablename);
            }).ToArray();
        }


        public static List<MethodWithParameterDependencies> FindParameterDependencies(SoftwareCell integration,
            List<DataStream> connections)
        {
            return integration.Integration.Select(sc => new MethodWithParameterDependencies
            {
                OfSoftwareCell = sc,
                Parameters = FindParameters(connections,integration, sc)
            }).ToList();
        }


        public static List<Parameter> FindParameters(List<DataStream> connections, SoftwareCell integration, SoftwareCell ofSoftwareCell)
        {
            var nameTypes =
                DataStreamParser.GetInputPart(ofSoftwareCell.InputStreams.First().DataNames);

            return
                nameTypes.Where(nt => nt.Type != "")
                    .Select(nt => FindOneParameter(nt, integration, connections, ofSoftwareCell))
                    .ToList();
        }


        public static Parameter FindOneParameter(NameType lookingForNameType, SoftwareCell parent, List<DataStream> connections, SoftwareCell ofSoftwareCell)
        {
            var parameter = new Parameter
            {
                FoundFlag = false,   
                NameType = lookingForNameType,            
                Source = null
            };

            var foundInParent =  parent?.InputStreams.Select(dsd =>
            {
                var inputnametypes = DataStreamParser.GetOutputPart(dsd.DataNames).ToList();
                var found = inputnametypes
                    .Where(nt => IsMatchingNameType(lookingForNameType, nt))
                    .ToList();

                return found.Any() ? found.First() : null;
            }).Where(x => x != null).ToList();

            if (foundInParent != null && foundInParent.Any())
            {
                parameter.FoundFlag = true;
                return parameter;               
            }


            while (true)
            {            
                var dataStream = GetInputDataStream(connections, ofSoftwareCell);
                if (dataStream == null)
                {
                    return parameter;
                }

                var found = FindTypeInDataStream(lookingForNameType, dataStream);
                if (found.Any())
                {
                    parameter.FoundFlag = true;
                    parameter.Source = dataStream.Sources.First().Parent;
                    return parameter;
                }

                ofSoftwareCell = dataStream.Sources.First().Parent;
            }
        }


        private static List<NameType> FindTypeInDataStream(NameType lookingForNameType, DataStream dataStream)
        {
            var outputNametypes = DataStreamParser.GetOutputPart(dataStream.DataNames).ToList();

            var found = outputNametypes
                .Where(nt => IsMatchingNameType(lookingForNameType, nt))
                .ToList();

            return found;
        }


        private static bool IsMatchingNameType(NameType lookingForNameType, NameType nt)
        {
            return nt.Type == lookingForNameType.Type
                && nt.IsArray == lookingForNameType.IsArray
                && nt.IsList == lookingForNameType.IsList
                && nt.Name == lookingForNameType.Name;
        }


        private static DataStream GetInputDataStream(List<DataStream> connections, SoftwareCell ofSoftwareCell)
        {
            var found = connections.Where(c => c.Destinations.Any(x => x.Parent == ofSoftwareCell)).ToList();
            return found.Any() ? found.First() : null;
        }


        public static SyntaxNode LocalMethodCall(SyntaxGenerator generator, SoftwareCell softwareCell, SyntaxNode[] parameter,
            List<GeneratedLocalVariable> generated)
        {
            var firstouttype = DataStreamParser.GetOutputPart(softwareCell.OutputStreams.First().DataNames).First();
            var localType = DataTypeParser.ConvertToTypeExpression(generator, firstouttype);
            var localName = GenerateLocalVariableName(firstouttype);

            generated.Add(new GeneratedLocalVariable
            {
                VariableName = localName,
                Source = softwareCell
            });

            var methodname = MethodsGenerator.GetMethodName(softwareCell);
            return GenerateLocalMethodCall(generator, methodname, parameter, firstouttype, localType, localName);
        }


        private static string GenerateLocalVariableName(NameType firstouttype)
        {
            return firstouttype.Name ?? "a" + firstouttype.Type;
        }


        private static SyntaxNode GenerateLocalMethodCall(SyntaxGenerator generator, string name,
            SyntaxNode[] parameter, NameType nameType, SyntaxNode localType, string localName)
        {
            var invocationExpression = generator.InvocationExpression(
                generator.IdentifierName(name), parameter ?? new SyntaxNode[] {});

            // When no type then it is void method and no assignemnt to local variable is needed
            if (nameType.Type == "") return invocationExpression;

            return generator.LocalDeclarationStatement(localType, localName, invocationExpression);
        }
    }
}
