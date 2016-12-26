using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Roslyn.Parser;
using Dexel.Library;

namespace Roslyn
{

    public static class IntegrationGenerator
    {
        public static SyntaxNode[] GenerateIntegrationBody(SyntaxGenerator generator, MainModel mainModel,
            FunctionUnit integration)
        {
            var result = new List<SyntaxNode>();

            var body = CreateNewIntegrationBody(mainModel.Connections, integration);
            body.Generator = generator;

            AddIntegrationParameterToLocalScope(body, integration);

            AnalyseParameterDependencies(body);
            AnalyseLambdaBodies(body, mainModel);

            GenerateBody(body, result.Add, mainModel);

            return result.ToArray();
        }

        private static void AnalyseLambdaBodies(IntegrationBody integrationBody, MainModel mainModel)
        {
            integrationBody.LambdaBodies = new List<LambdaBody>();

            TravelIntegration(integrationBody.Integration, mainModel,
                onInLambdaBody: integrationBody.LambdaBodies.Add);
        }


        public static void AddIntegrationParameterToLocalScope(IntegrationBody integrationBody, FunctionUnit integration)
        {
            var nametypes = DataStreamParser.GetInputPart(integration.InputStreams.First().DataNames);
            nametypes.ToList().ForEach(nametype =>
            {
                var name = MethodsGenerator.GenerateParameterName(nametype);
                integrationBody.LocalVariables.Add(new GeneratedLocalVariable
                {
                    VariableName = name,
                    Source = integration.InputStreams.First(),
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
                    Source = integration.InputStreams.First(),
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





        }



        private static void GenerateBody(IntegrationBody integrationBody, Action<SyntaxNode> onSyntaxNode, MainModel mainModel)
        {
            var toplevelCalls = integrationBody.LambdaBodies.Where(x => x.InsideLambdaOf == null).ToList();
            toplevelCalls.ForEach(c =>
            {
                var syntaxnode = CreateMethodCall(c.FunctionUnit, integrationBody);
                onSyntaxNode(syntaxnode);
            });
        }

        private static SyntaxNode CreateMethodCall(FunctionUnit functionUnit, IntegrationBody integrationBody)
        {
            SyntaxNode res = null;

            var parameter = new List<SyntaxNode>();

            AssignmentParameter_IncludingLambdaBodiesRecursive(integrationBody, functionUnit, parameter.Add);

            var methodname = MethodsGenerator.GetMethodName(functionUnit);

            var sigs = DataTypeParser.AnalyseOutputs(functionUnit);
            var returndsd = sigs.FirstOrDefault(sig => sig.ImplementWith == DataTypeParser.DataFlowImplementationStyle.AsReturn)?.DSD;

            string localName = null;
            if (returndsd != null)
            {
                var output = DataStreamParser.GetOutputPart(returndsd?.DataNames);
                localName = GenerateLocalVariableName(output);
                RegisterLocalVaribale(integrationBody, returndsd, localName, output);
            }
            res = GenerateLocalMethodCall(integrationBody.Generator, methodname, parameter.ToArray(), localName);

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
                    Source = streamingOutput,
                    VariableName = GetUniqueLocalVariableName(integrationBody.LocalVariables, nt)
                };
                integrationBody.LocalVariables.Add(localvar);
                lambdaNames.Add(localvar.VariableName);
            });
            return lambdaNames.Select(name => integrationBody.Generator.LambdaParameter(name)).ToArray();
        }


        private static void GenerateLambdaExpression(IntegrationBody integrationBody, SyntaxNode[] lambdaParameter, SyntaxNode[] lambdaBody,
            Action<SyntaxNode> onCreated)
        {
            //integrationBody.Generator.MethodDeclaration()
            onCreated(integrationBody.Generator.VoidReturningLambdaExpression(lambdaParameter, lambdaBody));
        }


        public static IntegrationBody CreateNewIntegrationBody(List<DataStream> connections,
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




        private static void RegisterLocalVaribale(IntegrationBody integrationBody, DataStreamDefinition dsd, string localname,
            List<NameType> nametypes)
        {
            integrationBody.LocalVariables.Add(new GeneratedLocalVariable
            {
                VariableName = localname,
                Source = dsd,
                NameTypes = nametypes
            });
        }


        public static void AssignmentParameter_IncludingLambdaBodiesRecursive(IntegrationBody integrationBody, FunctionUnit functionUnit, Action<SyntaxNode> onParameterGenerated)
        {
            GenereteArgumentsFromInput(integrationBody, functionUnit, onParameterGenerated);

            var sigs = DataTypeParser.AnalyseOutputs(functionUnit);
            var asAction = sigs.Where(x => x.ImplementWith == DataTypeParser.DataFlowImplementationStyle.AsAction);      
            asAction.ForEach(sig =>
            {
                var dsd = sig.DSD;
                
                List<SyntaxNode> lambdabody = new List<SyntaxNode>();
                var functionUnitsToGenerateInsideThisLambda =
                integrationBody.LambdaBodies.Where(lb => lb.InsideLambdaOf == dsd).Select(x => x.FunctionUnit).ToList();
                SyntaxNode[] param  = GenerateLambdaParameter(integrationBody, dsd);
                
                functionUnitsToGenerateInsideThisLambda.ForEach(fu =>
                {      
                    var mc = CreateMethodCall(fu, integrationBody);
                    lambdabody.Add(mc);
                });
                if (functionUnitsToGenerateInsideThisLambda.Any())
                {
                    SyntaxNode lambda = null;
                    GenerateLambdaExpression(integrationBody,param,lambdabody.ToArray(), node => lambda = node);

                    SyntaxNode arg;
                    if (string.IsNullOrWhiteSpace(dsd.ActionName))
                        arg = integrationBody.Generator.Argument(lambda);
                    else
                    {
                        var nts = DataStreamParser.GetOutputPart(sig.DSD.DataNames);
                        var argumentName = MethodsGenerator.GetNameOfAction(sig, nts);
                        arg = integrationBody.Generator.Argument(argumentName, RefKind.None, lambda);
                    }
                    onParameterGenerated(arg);
                }

            });

        }

        public static void GenereteArgumentsFromInput(IntegrationBody integrationBody, FunctionUnit functionUnit,
            Action<SyntaxNode> onParameterGenerated)
        {
            var inputsDsd = functionUnit.InputStreams.First();
            var inputs = DataStreamParser.GetInputPart(inputsDsd.DataNames);
            var dependecies = integrationBody.CallDependecies.First(x => x.OfFunctionUnit == functionUnit);

            List<DataStreamDefinition> alreadyDone = new List<DataStreamDefinition>();
            dependecies.Parameters.ToList().ForEach(parameter =>
            {
                if (alreadyDone.Contains(parameter.Source)) //TODO: not  best solution
                    return;

                var varnames = inputs.Select(nt => GetLocalVariable(integrationBody, parameter, nt)?.VariableName);
                varnames.Where(x => x != null).ToList().ForEach(varname =>
                {
                    var arg = integrationBody.Generator.Argument(integrationBody.Generator.IdentifierName(varname));
                    onParameterGenerated(arg);
                });
                alreadyDone.Add(parameter.Source);
            });
        }

        private static GeneratedLocalVariable GetLocalVariable(IntegrationBody integrationBody, Parameter parameter, NameType needed)
        {
            return integrationBody.LocalVariables.First(y => y.Source == parameter.Source 
            && y.NameTypes.First().Type == needed.Type
            && y.NameTypes.First().Name == needed.Name);
        }


        public static void AnalyseParameterDependencies(IntegrationBody integrationBody)
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
            var inputNameTypes = DataStreamParser.GetInputPart(ofFunctionUnit.InputStreams.First().DataNames);
            var result = inputNameTypes.Where(nt => nt.Type != "")
                .Select(nt => FindOneParameter(nt, parent, connections, ofFunctionUnit, true)).ToList();


            //var signature = DataTypeParser.AnalyseOutputs(ofFunctionUnit);
            //signature.Where(sig => sig.ImplementWith != DataTypeParser.DataFlowImplementationStyle.AsReturn).ToList().ForEach(
            //    sig =>
            //    {
            //        var outputNameTypes = DataStreamParser.GetOutputPart(sig.DSD.DataNames);

            //        outputNameTypes.Where(nt => nt.Type != "").Select(
            //            nt => FindOneParameter(nt, parent, connections, ofFunctionUnit, false))
            //            .ToList().ForEach(result.Add);
            //    });
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
                Source = parent?.InputStreams.First()
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
            AnalyseParameterDependencies(body);


            FunctionUnit beginning = null;

            MainModelManager.GetBeginningOfFlow(integration.IsIntegrating.First(), mainModel)
                    .TryCast<DataStreamDefinition>(dsd => beginning = dsd.Parent);

            onInLambdaBody(new LambdaBody { FunctionUnit = beginning, InsideLambdaOf = null });

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