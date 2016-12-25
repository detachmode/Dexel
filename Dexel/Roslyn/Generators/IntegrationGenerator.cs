using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Roslyn.Generators;
using Roslyn.Parser;
using Dexel.Library;

namespace Roslyn
{

    public static class IntegrationGenerator
    {
        private static int _methodsToGenerateCount;


        public static SyntaxNode[] CreateIntegrationBody(SyntaxGenerator generator, MainModel mainModel,
            FunctionUnit integration)
        {
            var result = new List<SyntaxNode>();
            var body = CreateNewIntegrationBody(mainModel.Connections, integration);
            AddIntegrationParameterToLocalScope(body, integration);
            FindParameterDependencies(body);
            AddLambdaBodies(body, mainModel);
            GenerateBody(body, result.Add, mainModel);

            return result.ToArray();
        }

        private static void AddLambdaBodies(IntegrationBody integrationBody, MainModel mainModel)
        {
            integrationBody.LambdaBodies = new List<IntegrationGenerator.LambdaBody>();

            TravelIntegration(integrationBody.Integration, mainModel,
                onInLambdaBody: integrationBody.LambdaBodies.Add);
        }


        private static void AddIntegrationParameterToLocalScope(IntegrationBody integrationBody, FunctionUnit integration)
        {
            var nametypes = DataStreamParser.GetInputPart(integration.InputStreams.First().DataNames);
            nametypes.ToList().ForEach(nametype =>
            {
                var name = MethodsGenerator.GenerateParameterName(nametype);
                integrationBody.LocalVariables.Add(new GeneratedLocalVariable
                {
                    VariableName = name,
                    Source = integration,
                    NameTypes = new[] { nametype }
                });
            });

            nametypes = DataStreamParser.GetOutputPart(integration.OutputStreams.First().DataNames);
            nametypes.ToList().ForEach(nametype =>
            {
                var name = MethodsGenerator.GenerateParameterName(nametype);
                integrationBody.LocalVariables.Add(new GeneratedLocalVariable
                {
                    VariableName = name,
                    Source = integration,
                    NameTypes = new[] { nametype }
                });
            });
        }




        public static void TravelOutputs(FunctionUnit currentFunctionUnit, object bodytoReturnTo, MainModel mainModel,
            Action<DataStreamDefinition, FunctionUnit> onAsReturn,
            Action<DataStreamDefinition, FunctionUnit> onAsAction)
        {



            var outputSignature = DataTypeParser.AnalyseOutputs(currentFunctionUnit);

            var asAction = outputSignature
                .Where(sig => sig.ImplementWith == DataTypeParser.DataFlowImplementationStyle.AsAction)
                .ToList();

            foreach (var signaturePart in asAction)
            {

                var stream = MainModelManager.FindDataStream(signaturePart.DSD, mainModel);
                if (stream != null)
                {
                    onAsAction(signaturePart.DSD, stream.Destinations.First().Parent);
                    TravelOutputs(stream.Destinations.First().Parent, signaturePart.DSD, mainModel, onAsReturn: onAsReturn, onAsAction: onAsAction);
                }
            }

            var asReturn = outputSignature.FirstOrDefault(
                sig => sig.ImplementWith == DataTypeParser.DataFlowImplementationStyle.AsReturn);


            if (asReturn != null)
            {

                var stream = MainModelManager.FindDataStream(asReturn.DSD, mainModel);
                if (stream != null)
                {
                    DataStreamDefinition dsd = null;
                    bodytoReturnTo.TryCast<DataStreamDefinition>(def => dsd = def);
                    onAsReturn(dsd, stream.Destinations.First().Parent);
                    TravelOutputs(stream.Destinations.First().Parent, bodytoReturnTo, mainModel, onAsReturn: onAsReturn, onAsAction: onAsAction);
                }
            }


            //var call = new Call();


            //if (asReturn != null)
            //{
            //    var var = new Var();
            //    var.Type = DataStreamParser.GetOutputPart(asReturn.DSD);
            //    var.VariableName = "var";
            //    call.ReturnToVar = var;
            //}


            //var dep = IntegrationBody.CallDependecies;



        }



        private static void GenerateBody(IntegrationBody integrationBody, Action<SyntaxNode> onSyntaxNode, MainModel mainModel)
        {
            var beginning = MainModelManager.GetBeginningOfFlow(integrationBody.Integration.IsIntegrating.First(),mainModel) as DataStreamDefinition;
            var syntaxnode = CreateMethodCall(beginning?.Parent, integrationBody);
            onSyntaxNode(syntaxnode);

            TravelOutputs(beginning?.Parent, null, mainModel, onAsAction: (definition, unit) => { },
                onAsReturn: (dsd, fu) =>
                {
                    syntaxnode = CreateMethodCall(fu, integrationBody);
                    onSyntaxNode(syntaxnode);
                } );
        }

        private static SyntaxNode CreateMethodCall(FunctionUnit functionUnit, IntegrationBody integrationBody)
        {
            SyntaxNode res = null;

            var parameter = new List<SyntaxNode>();
            var methoddepencies = integrationBody.CallDependecies.First(x => x.OfFunctionUnit == functionUnit);
            AssignmentParameter(integrationBody, methoddepencies.Parameters, parameter.Add);
            var methodname = MethodsGenerator.GetMethodName(functionUnit);

            DetectLocalVariableNeeded(integrationBody, functionUnit, (localname, nametypes) =>
            {
                res = GenerateLocalMethodCall(integrationBody.Generator, methodname, parameter.ToArray(), localname);
                RegisterLocalVaribale(integrationBody, functionUnit, localname, nametypes);
            },
            notVariableNeeded: () => res = GenerateLocalMethodCall(integrationBody.Generator, methodname, parameter.ToArray(), null));

            return res;
        }


        private static SyntaxNode[] GenerateLambdaParameter(IntegrationBody integrationBody, DataStreamDefinition streamingOutput)
        {
            var inputs = DataStreamParser.GetOutputPart(streamingOutput.DataNames);
            var lambdaNames = new List<string>();
            inputs.ForEach(nt =>
            {
                var localvar = new GeneratedLocalVariable
                {
                    NameTypes = new List<NameType> { nt },
                    Source = streamingOutput.Parent,
                    VariableName = GetUniqueLocalVariableName(integrationBody.LocalVariables, nt)
                };
                integrationBody.LocalVariables.Add(localvar);
                lambdaNames.Add(localvar.VariableName);
            });
            return lambdaNames.Select(name => integrationBody.Generator.LambdaParameter(name)).ToArray();
        }


        private static void MakeLocalMethodCallWithLambda(IntegrationBody integrationBody, FunctionUnit toGenerate,
            SyntaxNode[] lambdaParameter, SyntaxNode[] lambdaBody, Action<SyntaxNode> onNodeCreated)
        {
            CanBeGenerated(integrationBody, toGenerate, method =>
            {
                var parameter = new List<SyntaxNode>();
                AssignmentParameter(integrationBody, method.Parameters, parameter.Add);
                GenerateLambdaExpression(integrationBody, lambdaParameter, lambdaBody, parameter.Add);

                var methodname = MethodsGenerator.GetMethodName(method.OfFunctionUnit);
                onNodeCreated(GenerateLocalMethodCall(integrationBody.Generator, methodname, parameter.ToArray(), null));

            }, cannnotCreate: errorFu => Debug.WriteLine($"Couldn't create {errorFu.Name}"));
        }


        private static void GenerateLambdaExpression(IntegrationBody integrationBody, SyntaxNode[] lambdaParameter, SyntaxNode[] lambdaBody,
            Action<SyntaxNode> onCreated)
        {
            onCreated(integrationBody.Generator.VoidReturningLambdaExpression(lambdaParameter, lambdaBody));
        }


        private static void RemoveConnectionAndFunctionUnit(IntegrationBody integrationBody, FunctionUnit functionUnit)
        {
            integrationBody.Connections.RemoveAll(c => c.Sources.Any(dsd => dsd.Parent == functionUnit));
            integrationBody.ChildrenToGenerate.RemoveAll(x => functionUnit.ID == x.ID);
        }


        private static IntegrationBody CreateNewIntegrationBody(List<DataStream> connections,
            FunctionUnit integration)
        {
            var body = new IntegrationBody
            {
                LocalVariables = new List<GeneratedLocalVariable>(),
                Connections = connections.ToList(),
                ChildrenToGenerate = integration.IsIntegrating.ToList(),
                Integration = integration
            };
            return body;
        }


        private static void GetNextFunctionUnit(IntegrationBody integrationBody, Action<FunctionUnit> outputAndInputIsStream,
            Action<FunctionUnit> outputIsStream,
            Action<FunctionUnit> inputIsStream,
            Action<FunctionUnit> noStream)
        {
            if (!integrationBody.ChildrenToGenerate.Any()) return;

            var first = integrationBody.ChildrenToGenerate.First();
            MainModelManager.TraverseChildrenBackwards(first,
                (fu, conn) => first = fu, integrationBody.Connections);

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




        private static void RegisterLocalVaribale(IntegrationBody integrationBody, FunctionUnit functionUnit, string localname,
            List<NameType> nametypes)
        {
            integrationBody.LocalVariables.Add(new GeneratedLocalVariable
            {
                VariableName = localname,
                Source = functionUnit,
                NameTypes = nametypes
            });
        }


        private static void DetectLocalVariableNeeded(IntegrationBody integrationBody, FunctionUnit functionUnit,
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


        private static void CanBeGenerated(IntegrationBody integrationBody, FunctionUnit functionUnit,
            Action<MethodWithParameterDependencies> doAction, Action<FunctionUnit> cannnotCreate = null)
        {
            var methodWithParameterDependencies = integrationBody.CallDependecies.First(x => x.OfFunctionUnit == functionUnit);
            if (methodWithParameterDependencies == null) return;
            if (methodWithParameterDependencies.Parameters.TrueForAll(param => IsInBody(integrationBody, param)))
                doAction(methodWithParameterDependencies);
            else
            {
                cannnotCreate?.Invoke(methodWithParameterDependencies.OfFunctionUnit);
            }
        }


        private static bool IsInBody(IntegrationBody integrationBody, Parameter param)
        {
            return integrationBody.LocalVariables.Any(c => c.Source == param.Source.Parent);
        }


        public static void AssignmentParameter(IntegrationBody integrationBody, List<Parameter> parameters, Action<SyntaxNode> onParameterGenerated)
        {
            parameters.ToList().ForEach(p =>
            {

                if (p.AsOutput)
                {
                    List<SyntaxNode> lambdaBody = new List<SyntaxNode>();
                    integrationBody.LambdaBodies.Where(lb => lb.InsideLambdaOf == p.Source).ToList().ForEach(lb =>
                    {
                        integrationBody.ChildrenToGenerate.Remove(lb.FunctionUnit);
                        lambdaBody.Add(CreateMethodCall(lb.FunctionUnit, integrationBody));                     
                    });
                    var lambdaParameter = GenerateLambdaParameter(integrationBody, p.Source);
                    GenerateLambdaExpression(integrationBody, lambdaParameter, lambdaBody.ToArray(), onParameterGenerated);
                }
                else
                {
                    var variablename = integrationBody.LocalVariables.First(x => x.Source == p.Source.Parent).VariableName;
                    onParameterGenerated(integrationBody.Generator.IdentifierName(variablename));

                }

            });
        }


        private static void ParameterSource(Parameter parameter, Action fromParent, Action fromChild)
        {
            if (parameter.FoundFlag == Found.FromParent)
                fromParent();

            if (parameter.FoundFlag == Found.FoundInPreviousChild)
                fromChild();
        }


        public static void FindParameterDependencies(IntegrationBody integrationBody)
        {
            integrationBody.CallDependecies = integrationBody.ChildrenToGenerate.Select(sc => new MethodWithParameterDependencies
            {
                OfFunctionUnit = sc,
                Parameters = FindParameters(sc, integrationBody.Connections, integrationBody.Integration)
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
            //var inputNameTypes = DataStreamParser.GetInputPart(ofFunctionUnit.InputStreams.First().DataNames);
            //var result = inputNameTypes.Where(nt => nt.Type != "")
            //    .Select(nt => FindOneParameter(nt, parent, connections, ofFunctionUnit, true)).ToList();


            var signature = DataTypeParser.AnalyseOutputs(ofFunctionUnit);

            List<Parameter> result = new List<Parameter>();
            signature.Where(sig => sig.ImplementWith != DataTypeParser.DataFlowImplementationStyle.AsReturn).ToList().ForEach(
                sig =>
                {
                    var outputNameTypes = DataStreamParser.GetOutputPart(sig.DSD.DataNames);

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
                Source = parent.InputStreams.First()
            };

            CheckParentForMatchingInputOutputs(parameter, parent, onNotFound: () =>
            {
                MainModelManager.TraverseChildrenBackwards(ofFunctionUnit, (child, conn) =>
                {
                    var found = FindTypeInDataStream(lookingForNameType, conn);
                    if (!found.Any() || parameter.FoundFlag != Found.NotFound)
                        return; // set state only when found or not already set

                    parameter.FoundFlag = Found.FoundInPreviousChild;
                    parameter.Source = conn.Sources.First();
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
                generator.IdentifierName(name), parameter ?? new SyntaxNode[] { });

            // When no type then it is void method and no assignemnt to local variable is needed
            if (localname == null) return invocationExpression;

            return generator.LocalDeclarationStatement(localname, invocationExpression);
        }


        public class LambdaBody
        {
            public DataStreamDefinition InsideLambdaOf;
            public FunctionUnit FunctionUnit;
        }

        public static void TravelIntegration(FunctionUnit integration, MainModel mainModel, Action<LambdaBody> onInLambdaBody)
        {



            var body = CreateNewIntegrationBody(mainModel.Connections, integration);
            AddIntegrationParameterToLocalScope(body, integration);
            FindParameterDependencies(body);


            FunctionUnit beginning = null;

            MainModelManager.GetBeginningOfFlow(integration.IsIntegrating.First(), mainModel)
                    .TryCast<DataStreamDefinition>(dsd => beginning = dsd.Parent);

            onInLambdaBody(new LambdaBody {FunctionUnit = beginning, InsideLambdaOf = null});

            var outputSignature = DataTypeParser.AnalyseOutputs(beginning);

            var asAction = outputSignature
                .Where(sig => sig.ImplementWith == DataTypeParser.DataFlowImplementationStyle.AsAction)
                .ToList();

            foreach (var signaturePart in asAction)
            {

                var stream = MainModelManager.FindDataStream(signaturePart.DSD, mainModel);
                if (stream != null)
                {
                    var call = new LambdaBody();
                    call.FunctionUnit = stream.Destinations.First().Parent;
                    call.InsideLambdaOf = signaturePart.DSD;

                    onInLambdaBody(call);
                    TravelOutputs(stream.Destinations.First().Parent, signaturePart.DSD, mainModel,
                        onAsReturn: (output, fu) =>
                        {
                            var subcall = new LambdaBody();
                            subcall.FunctionUnit = fu;
                            subcall.InsideLambdaOf = output;
                            onInLambdaBody(subcall);
                        },
                        onAsAction: (output, fu) =>
                        {
                            var subcall = new LambdaBody();
                            subcall.FunctionUnit = fu;
                            subcall.InsideLambdaOf = output;
                            onInLambdaBody(subcall);
                        });
                }
            }

            var asReturn = outputSignature.FirstOrDefault(
                sig => sig.ImplementWith == DataTypeParser.DataFlowImplementationStyle.AsReturn);


            if (asReturn != null)
            {
                var stream = MainModelManager.FindDataStream(asReturn.DSD, mainModel);
                if (stream != null)
                {
                    var call = new LambdaBody();
                    call.FunctionUnit = stream.Destinations.First().Parent;
                    call.InsideLambdaOf = null;
                    onInLambdaBody(call);

                    TravelOutputs(stream.Destinations.First().Parent, integration, mainModel,
                        onAsAction: (output, fu) =>
                        {
                            var subcall = new LambdaBody();
                            subcall.FunctionUnit = fu;
                            subcall.InsideLambdaOf = output;
                            onInLambdaBody(subcall);
                        },
                        onAsReturn: (output, fu) =>
                        {
                            var subcall = new LambdaBody();
                            subcall.FunctionUnit = fu;
                            subcall.InsideLambdaOf = output;
                            onInLambdaBody(subcall);
                        });
                }
            }

        }

        private static object DetermineLastBody(DataStreamDefinition connectionSource, FunctionUnit functionUnit, IntegrationBody integrationBody, MainModel mainModel)
        {
            object @return = null;

            var calldependecy = integrationBody.CallDependecies.First(cd => cd.OfFunctionUnit == functionUnit);
            if (calldependecy.Parameters.Any(p => p.Source == connectionSource))
            {
                return connectionSource;
            }
            else
            {
                MainModelManager.TraverseChildrenBackwards(connectionSource.Parent, (fu, stream) =>
                {
                    if (calldependecy.Parameters.Any(p => p.Source == stream.Sources.First()))
                    {
                        if (@return == null)
                            @return = stream.Sources.First();
                    }
                }, mainModel.Connections);
            }
            return @return;
        }
    }




    public class IntegrationBody
    {
        public List<FunctionUnit> ChildrenToGenerate;
        public List<DataStream> Connections;
        public FunctionUnit Integration;
        public SyntaxGenerator Generator;
        public List<GeneratedLocalVariable> LocalVariables;
        public List<MethodWithParameterDependencies> CallDependecies { get; set; }
        public List<IntegrationGenerator.LambdaBody> LambdaBodies { get; set; }
    }

}