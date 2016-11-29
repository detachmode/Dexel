using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn
{
    public static class Integrations
    {
        private static int _methodsToGenerateCount;


        public static SyntaxNode[] CreateIntegrationBody(SyntaxGenerator generator, List<DataStream> connections,
            SoftwareCell integration)
        {

            var result = CreateBody(generator, connections.ToList(), integration, integration.Integration.ToList(), new List<SyntaxNode>());
            return result.ToArray();
        }



        public static List<SyntaxNode> CreateBody(SyntaxGenerator generator, List<DataStream> connections,
            SoftwareCell parent, List<SoftwareCell> innerCells, List<SyntaxNode> result)
        {
            FirstIsStream(connections, innerCells,
            isStream: (softwarecell) =>
            {
                connections.RemoveAll(c => c.Sources.Any(dsd => dsd.Parent == softwarecell));
                innerCells.RemoveAll(x => softwarecell.ID == x.ID);

                var restBody = CreateBody(generator, connections, parent, innerCells, new List<SyntaxNode>());               
                var node = MakeLocalMethodCallWithLambda(generator, connections, softwarecell, innerCells, restBody);
                result.Add(node);
            },
            isNotStream: (softwarecell) =>
            {
                var generated = new List<GeneratedLocalVariable>();
                var methodcall = CreateNonStreamMethod(generator, generated, softwarecell, connections, parent);
                result.Add(methodcall);
            });

            return result;

        }


        private static void FirstIsStream(List<DataStream> connections, List<SoftwareCell> innerCells, Action<SoftwareCell> isStream, Action<SoftwareCell> isNotStream)
        {
            if (!innerCells.Any()) return;

            SoftwareCell first = innerCells.First();
            MainModelManager.TraverseChildrenBackwards(first,
                (cell, conn) => first = cell, connections);

            DataTypeParser.OutputIsStream(first, () => isStream(first), () => isNotStream(first));
        }


        private static SyntaxNode MakeLocalMethodCallWithLambda(SyntaxGenerator generator, List<DataStream> connections, SoftwareCell thisMethod, List<SoftwareCell> integrated, List<SyntaxNode> body)
        {
            SyntaxNode result = null;
            var inputs = DataStreamParser.GetOutputPart(thisMethod.OutputStreams.First().DataNames);
            var lambdatypes = inputs.Select(nt => generator.LambdaParameter("a" + nt.Type));
            result = generator.InvocationExpression(generator.IdentifierName(thisMethod.Name),
                   generator.VoidReturningLambdaExpression(lambdatypes, body.ToArray()));

            return result;
        }


        private static SyntaxNode CreateNonStreamMethod(SyntaxGenerator generator, List<GeneratedLocalVariable> generated, SoftwareCell softwarecell, List<DataStream> connections, SoftwareCell integration)
        {
            SyntaxNode result = null;
            var oneMethod = FindParameterDependencie(integration, softwarecell, connections);
            if (CanBeGenerated(generated, oneMethod))
                result = GenerateLocalMethodCall(generator, generated, oneMethod);

            return result;

        }


        private static void AddReturnStatement(SyntaxGenerator generator, SoftwareCell integration,
            List<GeneratedLocalVariable> generated, List<SyntaxNode> body)
        {
            var lookingforNameType = DataStreamParser.GetOutputPart(integration.OutputStreams.First().DataNames);

            FoundInLocalVariable(lookingforNameType, generated,
                nametype => body.Add(CreateReturn(generator, nametype.VariableName)),
                () =>
                {
                    FoundInParentInputParameter(lookingforNameType, integration, nametype =>
                    {
                        var variableName = MethodsGenerator.GenerateParameterName(nametype);
                        body.Add(CreateReturn(generator, variableName));
                    });
                });
        }


        private static SyntaxNode CreateReturn(SyntaxGenerator generator, string variableName)
        {
            return generator.ReturnStatement(generator.IdentifierName(variableName));
        }


        private static void FoundInParentInputParameter(List<NameType> lookingforNameTypes, SoftwareCell integration,
            Action<NameType> doAction)
        {
            var inputNameTypes = DataStreamParser.GetInputPart(integration.InputStreams.First().DataNames);
            var foundInParameter = inputNameTypes.FirstOrDefault(nt => lookingforNameTypes.All(searchNt => IsMatchingNameType(nt, searchNt)));
            if (foundInParameter != null)
                doAction(foundInParameter);
        }


        private static void FoundInLocalVariable(List<NameType> lookingforNameTypes, List<GeneratedLocalVariable> generated,
            Action<GeneratedLocalVariable> doAction, Action notFound)
        {
            if (generated.Any(gen => gen.NameTypes.All(genNt => lookingforNameTypes.All(searchNt => IsMatchingNameType(genNt, searchNt)))))
            {
                var foundVariable = generated.First(gen => gen.NameTypes.All(genNt => lookingforNameTypes.All(searchNt => IsMatchingNameType(genNt, searchNt))));
                doAction(foundVariable);
            }
            else
                notFound();
        }


        private static void HasOutput(SoftwareCell integration, Action doAction)
        {
            if (integration.OutputStreams.Any(x => DataStreamParser.GetOutputPart(x.DataNames).Any()))
                doAction();
        }


        //private static List<SyntaxNode> CreateAllDependenciesAvailable(SyntaxGenerator generator,
        //    SoftwareCell thisMethod, List<MethodWithParameterDependencies> methodsToGenerate,
        //    List<GeneratedLocalVariable> generated)
        //{
        //    if (!methodsToGenerate.Any() || (methodsToGenerate.Count == _methodsToGenerateCount))
        //        return result;

        //    // to detect if no more methods can be generated 
        //    _methodsToGenerateCount = methodsToGenerate.Count;

        //    var nodes = methodsToGenerate
        //        .Where(methodWithParameterDependencies => CanBeGenerated(generated, methodWithParameterDependencies))
        //        .Select(methodWithParameterDependencies =>
        //                GenerateLocalMethodCall(generator, generated, methodWithParameterDependencies)).ToList();

        //    result.AddRange(nodes);
        //    methodsToGenerate.RemoveAll(x => generated.Any(y => y.Source == x.OfSoftwareCell));

        //    return CreateAllDependenciesAvailable(generator, thisMethod, methodsToGenerate, generated);
        //}


        private static bool CanBeGenerated(List<GeneratedLocalVariable> generated,
            MethodWithParameterDependencies methodWithParameterDependencies)
        {
            return methodWithParameterDependencies.Parameters.TrueForAll(
                param => (param.Source == null) || generated.Any(c => c.Source == param.Source));
        }


        private static SyntaxNode GenerateLocalMethodCall(SyntaxGenerator generator,
            List<GeneratedLocalVariable> generated,
            MethodWithParameterDependencies x)
        {
            return LocalMethodCall(generator, x.OfSoftwareCell,
                CreateParameterAssignment(generator, generated, x.Parameters),
                generated);
        }


        public static SyntaxNode[] CreateParameterAssignment(SyntaxGenerator generator,
            List<GeneratedLocalVariable> generated, List<Parameter> parameters)
        {
            return parameters.Select(p =>
            {
                string variablename = "";
                ParameterSource(p,
                    fromParent: () => variablename = MethodsGenerator.GenerateParameterName(p.NeededNameType),
                    fromChild: () => variablename = generated.First(x => x.Source == p.Source).VariableName);

                if (p.AsAction)
                    return CreateLambdaExpression(generator, p, variablename);

                return generator.IdentifierName(variablename);
            }).ToArray();
        }


        private static SyntaxNode CreateLambdaExpression(SyntaxGenerator generator, Parameter parameter, string variablename)
        {
            return generator.IdentifierName($"() => {variablename}()");
        }

        private static void ParameterSource(Parameter parameter, Action fromParent, Action fromChild)
        {
            if (parameter.FoundFlag == Found.FromParent)
                fromParent();

            if (parameter.FoundFlag == Found.FoundInPreviousChild)
                fromChild();

        }


        public static List<MethodWithParameterDependencies> FindParameterDependencies(SoftwareCell integration,
            List<DataStream> connections)
        {
            return integration.Integration.Select(sc => new MethodWithParameterDependencies
            {
                OfSoftwareCell = sc,
                Parameters = FindParameters(sc, connections, integration)
            }).ToList();
        }

        public static MethodWithParameterDependencies FindParameterDependencie(SoftwareCell parent, SoftwareCell ofCell,
           List<DataStream> connections)
        {
            
            return new MethodWithParameterDependencies
            {
                OfSoftwareCell = ofCell,
                Parameters = FindParameters(ofCell, connections, parent )
            };

        }


        public static List<Parameter> FindParameters(SoftwareCell ofSoftwareCell, List<DataStream> connections,  SoftwareCell parent
           )
        {

            var inputNameTypes = DataStreamParser.GetInputPart(ofSoftwareCell.InputStreams.First().DataNames);
            var result = inputNameTypes.Where(nt => nt.Type != "")
                .Select(nt => FindOneParameter(nt, parent, connections, ofSoftwareCell)).ToList();


            DataTypeParser.OutputIsStream(ofSoftwareCell,
                isStream: () =>
                {
                    var outputNameTypes = DataStreamParser.GetOutputPart(ofSoftwareCell.OutputStreams.First().DataNames);

                    outputNameTypes.Where(nt => nt.Type != "").Select(
                        nt => FindOneParameter(nt, parent, connections, ofSoftwareCell))
                        .ToList().ForEach(result.Add);
                });

            return result;
        }


        public static Parameter FindOneParameter(NameType lookingForNameType, SoftwareCell parent,
            List<DataStream> connections, SoftwareCell ofSoftwareCell)
        {
            var parameter = new Parameter
            {
                FoundFlag = Found.NotFound,
                AsAction = lookingForNameType.IsInsideStream,
                NeededNameType = lookingForNameType,
                Source = null
            };

            CheckParentForMatchingInputOutputs(parameter, parent, onNotFound: () =>
            {
                MainModelManager.TraverseChildrenBackwards(ofSoftwareCell, (child, conn) =>
                {
                    var found = FindTypeInDataStream(lookingForNameType, conn);
                    if (!found.Any() || parameter.FoundFlag != Found.NotFound) return;  // set state only when found or not already set

                    parameter.FoundFlag = Found.FoundInPreviousChild;
                    parameter.Source = child;
                }, connections);
            });
            return parameter;
        }


        private static void CheckParentForMatchingInputOutputs(Parameter parameter, SoftwareCell parent, Action onNotFound)
        {
            if (parameter.AsAction)
            {
                if (!parent.OutputStreams.Any())
                    return;
                var output = parent.OutputStreams.First();

                var inputnametypes = DataStreamParser.GetOutputPart(output.DataNames);
                var found = inputnametypes.Where(nt => IsMatchingNameType(parameter.NeededNameType, nt)).ToList();

                if (found.Any())
                    parameter.FoundFlag = Found.FromParent;
            }
            else
            {
                var foundInParent = parent?.InputStreams.Select(dsd =>
                {
                    var inputnametypes = DataStreamParser.GetOutputPart(dsd.DataNames).ToList();
                    var found = inputnametypes
                        .Where(nt => IsMatchingNameType(parameter.NeededNameType, nt))
                        .ToList();

                    return found.Any() ? found.First() : null;
                }).Where(x => x != null).ToList();

                if ((foundInParent != null) && foundInParent.Any())
                    parameter.FoundFlag = Found.FromParent;
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
            return (nt.Type == lookingForNameType.Type)
                   && (nt.IsArray == lookingForNameType.IsArray)
                   && (nt.IsList == lookingForNameType.IsList)
                   && (nt.Name == lookingForNameType.Name);
        }


        private static DataStream GetInputDataStream(List<DataStream> connections, SoftwareCell ofSoftwareCell)
        {
            var found = connections.Where(c => c.Destinations.Any(x => x.Parent == ofSoftwareCell)).ToList();
            return found.Any() ? found.First() : null;
        }


        public static SyntaxNode LocalMethodCall(SyntaxGenerator generator, SoftwareCell softwareCell,
            SyntaxNode[] parameter,
            List<GeneratedLocalVariable> generated)
        {

            var output = DataStreamParser.GetOutputPart(softwareCell.OutputStreams.First().DataNames);
            var localType = DataTypeParser.ConvertToTypeExpression(generator, output);
            var localName = GenerateLocalVariableName(output);

            generated.Add(new GeneratedLocalVariable
            {
                VariableName = localName,
                Source = softwareCell,
                NameTypes = output
            });

            var methodname = MethodsGenerator.GetMethodName(softwareCell);
            return GenerateLocalMethodCall(generator, methodname, parameter, output, localType, localName);
        }


        private static string GenerateLocalVariableName(List<NameType> output)
        {
            if (output.Count == 0)
            {
                return null;
            }
            if (output.Any(x => x.IsInsideStream))
            {
                return "datastream";
            }
            if (output.Count > 1)
            {
                return "tupel";
            }
            return output.First().Name ?? "a" + output.First().Type;
        }

        private static SyntaxNode GenerateLocalMethodCallWithLambdaBody(SyntaxGenerator generator, string name,
           SyntaxNode[] parameter, List<NameType> nameType, SyntaxNode localType, string localName, List<SoftwareCell> integratedCells, List<DataStream> connections)
        {
            var invocationExpression = generator.InvocationExpression(
                generator.IdentifierName(name), parameter ?? new SyntaxNode[] { });

            // When no type then it is void method and no assignemnt to local variable is needed
            if (nameType.Count == 0) return invocationExpression;

            return generator.LocalDeclarationStatement(localType, localName, invocationExpression);
        }




        private static SyntaxNode GenerateLocalMethodCall(SyntaxGenerator generator, string name,
            SyntaxNode[] parameter, List<NameType> nameType, SyntaxNode localType, string localName)
        {
            var invocationExpression = generator.InvocationExpression(
                generator.IdentifierName(name), parameter ?? new SyntaxNode[] { });

            // When no type then it is void method and no assignemnt to local variable is needed
            if (nameType.Count == 0) return invocationExpression;

            return generator.LocalDeclarationStatement(localType, localName, invocationExpression);
        }
    }
}