using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn
{

    public static class IntegrationGenerator
    {
        private static int _methodsToGenerateCount;


        public static SyntaxNode[] CreateIntegrationBody(SyntaxGenerator generator, List<DataStream> connections,
            FunctionUnit integration)
        {
            var body = CreateNewBody(generator, connections, integration);
            AddIntegrationParameterToLocalScope(body, integration);
            FindParameterDependencies(body);
            var result = new List<SyntaxNode>();
            GenerateBody(body, result.Add);

            return result.ToArray();
        }


        private static void AddIntegrationParameterToLocalScope(Body body, FunctionUnit integration)
        {
            var nametypes = DataStreamParser.GetInputPart(integration.InputStreams.First().DataNames);
            nametypes.ToList().ForEach(nametype =>
            {
                var name = MethodsGenerator.GenerateParameterName(nametype);
                body.LocalVariables.Add(new GeneratedLocalVariable
                {
                    VariableName = name,
                    Source = integration,
                    NameTypes = new[] {nametype}
                });
            });

            nametypes = DataStreamParser.GetOutputPart(integration.OutputStreams.First().DataNames);
            nametypes.ToList().ForEach(nametype =>
            {
                var name = MethodsGenerator.GenerateParameterName(nametype);
                body.LocalVariables.Add(new GeneratedLocalVariable
                {
                    VariableName = name,
                    Source = integration,
                    NameTypes = new[] {nametype}
                });
            });
        }


        private static void GenerateBody(Body body, Action<SyntaxNode> onSyntaxNode)
        {
            GetNextFunctionUnit(body,
                outputAndInputIsStream: streamingFu =>
                {
                    RemoveConnectionAndFunctionUnit(body, streamingFu);
                    CreateNonStreamMethod(body, streamingFu, onSyntaxNode);
                    GenerateBody(body, onSyntaxNode);
                },
                outputIsStream: streamingFu =>
                {
                    RemoveConnectionAndFunctionUnit(body, streamingFu);

                    var lambdaBodyObject = CreateNewBody(body.Generator, body.Connections, streamingFu);
                    lambdaBodyObject.CallDependecies = body.CallDependecies;
                    lambdaBodyObject.LocalVariables = body.LocalVariables;
                    lambdaBodyObject.ChildrenToGenerate = body.ChildrenToGenerate;

                    var lambdaParameter = GenerateLambdaParameter(body, streamingFu);

                    var lambdaBody = new List<SyntaxNode>();
                    GenerateBody(lambdaBodyObject, lambdaBody.Add);

                    MakeLocalMethodCallWithLambda(body, streamingFu, lambdaParameter.ToArray(), lambdaBody.ToArray(),
                        onSyntaxNode);
                },
                inputIsStream: functionUnit =>
                {
                    RemoveConnectionAndFunctionUnit(body, functionUnit);
                    CreateNonStreamMethod(body, functionUnit, onSyntaxNode);
                    GenerateBody(body, onSyntaxNode);
                },
                noStream: functionUnit =>
                {
                    RemoveConnectionAndFunctionUnit(body, functionUnit);
                    CreateNonStreamMethod(body, functionUnit, onSyntaxNode);
                    GenerateBody(body, onSyntaxNode);
                });
        }


        private static SyntaxNode[] GenerateLambdaParameter(Body body, FunctionUnit streamingFu)
        {
            var inputs = DataStreamParser.GetOutputPart(streamingFu.OutputStreams.First().DataNames);
            var lambdaNames = new List<string>();
            inputs.ForEach(nt =>
            {
                var localvar = new GeneratedLocalVariable
                {
                    NameTypes = new List<NameType> {nt},
                    Source = streamingFu,
                    VariableName = GetUniqueLocalVariableName(body.LocalVariables, nt)
                };
                body.LocalVariables.Add(localvar);
                lambdaNames.Add(localvar.VariableName);
            });
            return lambdaNames.Select(name => body.Generator.LambdaParameter(name)).ToArray();
        }


        private static void MakeLocalMethodCallWithLambda(Body body, FunctionUnit toGenerate,
            SyntaxNode[] lambdaParameter, SyntaxNode[] lambdaBody, Action<SyntaxNode> onNodeCreated)
        {
            CanBeGenerated(body, toGenerate, method =>
            {
                var parameter = new List<SyntaxNode>();
                AssignmentParameter(body.Generator, body.LocalVariables, method.Parameters, parameter.Add);
                GenerateLambdaExpression(body, lambdaParameter, lambdaBody, parameter.Add);
               
                var methodname = MethodsGenerator.GetMethodName(method.OfFunctionUnit);
                onNodeCreated(GenerateLocalMethodCall(body.Generator, methodname, parameter.ToArray(), null));
              
            }, cannnotCreate: errorFu => Debug.WriteLine($"Couldn't create {errorFu.Name}"));
        }


        private static void GenerateLambdaExpression(Body body, SyntaxNode[] lambdaParameter, SyntaxNode[] lambdaBody,
            Action<SyntaxNode> onCreated)
        {
            onCreated(body.Generator.VoidReturningLambdaExpression(lambdaParameter, lambdaBody));
        }


        private static void RemoveConnectionAndFunctionUnit(Body body, FunctionUnit functionUnit)
        {
            body.Connections.RemoveAll(c => c.Sources.Any(dsd => dsd.Parent == functionUnit));
            body.ChildrenToGenerate.RemoveAll(x => functionUnit.ID == x.ID);
        }


        private static Body CreateNewBody(SyntaxGenerator generator, List<DataStream> connections,
            FunctionUnit integration)
        {
            var body = new Body
            {
                Generator = generator,
                LocalVariables = new List<GeneratedLocalVariable>(),
                Connections = connections.ToList(),
                ChildrenToGenerate = integration.IsIntegrating.ToList(),
                CurrentParent = integration
            };
            return body;
        }


        private static void GetNextFunctionUnit(Body body, Action<FunctionUnit> outputAndInputIsStream,
            Action<FunctionUnit> outputIsStream,
            Action<FunctionUnit> inputIsStream,
            Action<FunctionUnit> noStream)
        {
            if (!body.ChildrenToGenerate.Any()) return;

            var first = body.ChildrenToGenerate.First();
            MainModelManager.TraverseChildrenBackwards(first,
                (fu, conn) => first = fu, body.Connections);

            DataTypeParser.OutputOrInputIsStream(first,
                bothAreStreams: () => outputAndInputIsStream(first),
                onOutputIsStream: () => outputIsStream(first),
                onInputIsStream: () => inputIsStream(first),
                noStream: () => noStream(first));
        }


        public static string GetUniqueLocalVariableName(List<GeneratedLocalVariable> generated, NameType nt)
        {
            var defaultname = nt.Name ?? nt.Type.ToLower();
            var result = defaultname;
            var i = 1;
            while (generated.Any(x => x.VariableName == result))
            {
                i++;
                result = defaultname + i;
            }
            return result;
        }


        private static void CreateNonStreamMethod(Body body, FunctionUnit functionUnit, Action<SyntaxNode> onGenerated)
        {
            CanBeGenerated(body, functionUnit, method =>
            {
                var parameter = new List<SyntaxNode>();
                AssignmentParameter(body.Generator, body.LocalVariables, method.Parameters, parameter.Add);
                var methodname = MethodsGenerator.GetMethodName(functionUnit);

                DetectLocalVariableNeeded(body, functionUnit, (localname, nametypes) =>
                {                  
                    onGenerated(GenerateLocalMethodCall(body.Generator, methodname, parameter.ToArray(), localname));
                    RegisterLocalVaribale(body, functionUnit, localname, nametypes);
                },
                notVariableNeeded:() => onGenerated(GenerateLocalMethodCall(body.Generator, methodname, parameter.ToArray(), null)));
            }, 
            cannnotCreate: errorFu => Debug.WriteLine($"Couldn't create {errorFu.Name}"));
        }


        private static void RegisterLocalVaribale(Body body, FunctionUnit functionUnit, string localname,
            List<NameType> nametypes)
        {
            body.LocalVariables.Add(new GeneratedLocalVariable
            {
                VariableName = localname,
                Source = functionUnit,
                NameTypes = nametypes
            });
        }


        private static void DetectLocalVariableNeeded(Body body, FunctionUnit functionUnit,
            Action<string, List<NameType>> onLocalName, Action notVariableNeeded = null)
        {
            var output = DataStreamParser.GetOutputPart(functionUnit.OutputStreams.First().DataNames);
            if (!output.Any())
            {
                notVariableNeeded?.Invoke();
                return;
            }

            var localName = GenerateLocalVariableName(output);

            onLocalName(localName, output);
        }


        private static void CanBeGenerated(Body body, FunctionUnit functionUnit,
            Action<MethodWithParameterDependencies> doAction, Action<FunctionUnit> cannnotCreate = null)
        {
            var methodWithParameterDependencies = body.CallDependecies.First(x => x.OfFunctionUnit == functionUnit);
            if (methodWithParameterDependencies == null) return;
            if (methodWithParameterDependencies.Parameters.TrueForAll(param => IsInBody(body, param)))
                doAction(methodWithParameterDependencies);
            else
            {
                cannnotCreate?.Invoke(methodWithParameterDependencies.OfFunctionUnit);
            }
        }


        private static bool IsInBody(Body body, Parameter param)
        {
            return body.LocalVariables.Any(c => c.Source == param.Source);
        }


        public static void AssignmentParameter(SyntaxGenerator generator,
            List<GeneratedLocalVariable> generated, List<Parameter> parameters, Action<SyntaxNode> onNode)
        {
            parameters.Where(x => !x.AsOutput).ToList().ForEach(p =>
            {
                var variablename = generated.First(x => x.Source == p.Source).VariableName;
                //ParameterSource(p,
                //    fromParent: () => variablename = generated.First(x => x.),
                //    fromChild: () => variablename = generated.First(x => x.Source == p.Source).VariableName);

                //if (p.AsOutput)                
                //        onNode(generator.IdentifierName(generated.First(localvar => localvar.Source == p.Source).VariableName));
                //else
                onNode(generator.IdentifierName(variablename));
            });
        }


        private static void ParameterSource(Parameter parameter, Action fromParent, Action fromChild)
        {
            if (parameter.FoundFlag == Found.FromParent)
                fromParent();

            if (parameter.FoundFlag == Found.FoundInPreviousChild)
                fromChild();
        }


        public static void FindParameterDependencies(Body body)
        {
            body.CallDependecies = body.ChildrenToGenerate.Select(sc => new MethodWithParameterDependencies
            {
                OfFunctionUnit = sc,
                Parameters = FindParameters(sc, body.Connections, body.CurrentParent)
            }).ToList();
        }


        public static MethodWithParameterDependencies FindParameterDependencie(FunctionUnit parent, FunctionUnit ofFu,
            List<DataStream> connections)
        {
            return new MethodWithParameterDependencies
            {
                OfFunctionUnit = ofFu,
                Parameters = FindParameters(ofFu, connections, parent)
            };
        }


        public static List<Parameter> FindParameters(FunctionUnit ofFunctionUnit, List<DataStream> connections,
            FunctionUnit parent
            )
        {
            var inputNameTypes = DataStreamParser.GetInputPart(ofFunctionUnit.InputStreams.First().DataNames);
            var result = inputNameTypes.Where(nt => nt.Type != "")
                .Select(nt => FindOneParameter(nt, parent, connections, ofFunctionUnit, true)).ToList();


            DataTypeParser.AnalyseOutputs(ofFunctionUnit,
                isComplexOutput: () =>
                {
                    var outputNameTypes = DataStreamParser.GetOutputPart(ofFunctionUnit.OutputStreams.First().DataNames);

                    outputNameTypes.Where(nt => nt.Type != "").Select(
                        nt => FindOneParameter(nt, parent, connections, ofFunctionUnit, false))
                        .ToList().ForEach(result.Add);
                });

            return result;
        }


        public static Parameter FindOneParameter(NameType lookingForNameType, FunctionUnit parent,
            List<DataStream> connections, FunctionUnit ofFunctionUnit, bool isInput)
        {
            var parameter = new Parameter
            {
                FoundFlag = Found.NotFound,
                AsOutput = isInput == false && lookingForNameType.IsInsideStream,
                NeededNameType = lookingForNameType,
                Source = parent
            };

            CheckParentForMatchingInputOutputs(parameter, parent, onNotFound: () =>
            {
                MainModelManager.TraverseChildrenBackwards(ofFunctionUnit, (child, conn) =>
                {
                    var found = FindTypeInDataStream(lookingForNameType, conn);
                    if (!found.Any() || parameter.FoundFlag != Found.NotFound)
                        return; // set state only when found or not already set

                    parameter.FoundFlag = Found.FoundInPreviousChild;
                    parameter.Source = child;
                }, connections);
            });
            return parameter;
        }


        private static void CheckParentForMatchingInputOutputs(Parameter parameter, FunctionUnit parent,
            Action onNotFound)
        {
            if (parameter.AsOutput)
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
            if (parameter.FoundFlag == Found.NotFound)
            {
                onNotFound();
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


        private static string GenerateLocalVariableName(List<NameType> output)
        {
            if (output.Count == 0)
            {
                return null;
            }
            if (output.Count > 1)
            {
                return "tupel";
            }
            return output.First().Name ?? "a" + output.First().Type;
        }


        public static SyntaxNode GenerateLocalMethodCall(SyntaxGenerator generator, string name, SyntaxNode[] parameter,
            string localname)
        {
            var invocationExpression = generator.InvocationExpression(
                generator.IdentifierName(name), parameter ?? new SyntaxNode[] {});

            // When no type then it is void method and no assignemnt to local variable is needed
            if (localname == null) return invocationExpression;

            return generator.LocalDeclarationStatement(localname, invocationExpression);
        }
    }


    public class Body
    {
        public List<FunctionUnit> ChildrenToGenerate;
        public List<DataStream> Connections;
        public FunctionUnit CurrentParent;
        public SyntaxGenerator Generator;
        public List<GeneratedLocalVariable> LocalVariables;
        public List<MethodWithParameterDependencies> CallDependecies { get; set; }
    }

}