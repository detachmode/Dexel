using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Roslyn.Parser;
using Dexel.Library;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Exceptions;
using Roslyn.Generators;

namespace Roslyn
{

    public static class IntegrationGenerator
    {
        public static SyntaxNode[] GenerateIntegrationBody(SyntaxGenerator generator, MainModel mainModel,
            FunctionUnit integration, Action<IntegrationBody> getIntegrationBody = null)
        {
            // integrationbody object for storing analysed and generated information
            var integrationBody = CreateNewIntegrationBody(mainModel.Connections, integration);
            AddIntegrationInputParameterToLocalScope(integrationBody, integration);

            // analyse data flow before generation 
            AnalyseParameterDependencies(integrationBody);
            AnalyseLambdaBodies(integrationBody, mainModel);
            AnalyseMatchingOutputOfIntegration(integrationBody, mainModel);
            AnalyseReturnToLocalReturnVariable(integrationBody, mainModel);

            // generation
            var result = new List<SyntaxNode>();
            integrationBody.Generator = generator;
            GenerateBody(integrationBody, result.Add);

            // when generating integration signature, some information from the body is needed: is returning local variable nullable type
            getIntegrationBody?.Invoke(integrationBody);

            return result.ToArray();
        }


        public static void AnalyseReturnToLocalReturnVariable(IntegrationBody integrationBody, MainModel mainModel)
        {
            integrationBody.ReturnToLocalReturnVariable = 
                integrationBody.LambdaBodies.Where(x => x.InsideLambdaOf != null).Select(x =>
                {
                    
                    var sigs = DataStreamParser.AnalyseOutputs(x.FunctionUnit);
                    var sigsOfIntegration = DataStreamParser.AnalyseOutputs(integrationBody.Integration);
                    var dsdThatReturnsFromSubFunctionUnit  =  sigs.FirstOrDefault(y => y.ImplementWith == DataFlowImplementationStyle.AsReturn)?.DSD;
                    var dsdThatReturnsFromIntegration = sigsOfIntegration.FirstOrDefault(y => y.ImplementWith == DataFlowImplementationStyle.AsReturn)?.DSD;

                    var found = integrationBody.OutputOfIntegration
                        .FirstOrDefault(matchingOutputs => 
                            matchingOutputs.SubFunctionUnitOutput == dsdThatReturnsFromSubFunctionUnit 
                            && matchingOutputs.IntegrationOutput == dsdThatReturnsFromIntegration);

                    return found?.SubFunctionUnitOutput;

                }).Where( x => x != null).ToList();
        }


        public static void AnalyseMatchingOutputOfIntegration(IntegrationBody body, MainModel mainModel)
        {
            var sigs = DataStreamParser.AnalyseOutputs(body.Integration);

            var allNotConnectedOutputs = body.Integration.IsIntegrating
                 .SelectMany(fu => fu.OutputStreams.Where(x => !x.Connected)).ToList();

            var allmatches = sigs.SelectMany(sig =>
            {
                var actionOfIntegration = sig.DSD;
                var allmatching = allNotConnectedOutputs
                .Where(dsd => !(string.IsNullOrWhiteSpace(dsd.ActionName) && dsd.DataNames.Trim() == "()"))
                .Where(dsd => AreEqualDsds(dsd, actionOfIntegration));

                return allmatching.Select(dsd => new MatchingOutputs
                {
                    IntegrationOutput = actionOfIntegration,
                    SubFunctionUnitOutput = dsd
                });

            }).ToList();

            body.OutputOfIntegration = allmatches;
        }


        private static bool AreEqualDsds(DataStreamDefinition dsd, DataStreamDefinition actionOfIntegration)
        {
            return (dsd.ActionName == actionOfIntegration.ActionName ||
                    (string.IsNullOrWhiteSpace(dsd.ActionName) && string.IsNullOrWhiteSpace(actionOfIntegration.ActionName)))
                   && dsd.DataNames == actionOfIntegration.DataNames;
        }


        public static void AnalyseLambdaBodies(IntegrationBody integrationBody, MainModel mainModel)
        {
            integrationBody.LambdaBodies = new List<LambdaBody>();

            TravelIntegration(integrationBody.Integration, mainModel,
                onInLambdaBody: integrationBody.LambdaBodies.Add);
        }


        public static void AddIntegrationInputParameterToLocalScope(IntegrationBody integrationBody, FunctionUnit integration)
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

            var outputSignature = DataStreamParser.AnalyseOutputs(currentFunctionUnit);

            var asAction = outputSignature
                .Where(sig => sig.ImplementWith == DataFlowImplementationStyle.AsAction)
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
                sig => sig.ImplementWith == DataFlowImplementationStyle.AsReturn);


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



        private static void GenerateBody(IntegrationBody integrationBody, Action<SyntaxNode> onSyntaxNode)
        {
            NeedsLocalReturnVariable(integrationBody, () => GenerateReturnLocalVariable(integrationBody, onSyntaxNode));

            var toplevelCalls = integrationBody.LambdaBodies.Where(x => x.InsideLambdaOf == null).ToList();
            toplevelCalls.ForEach(c =>
            {
                var syntaxnode = CreateMethodCall(c.FunctionUnit, integrationBody);
                onSyntaxNode(syntaxnode);
            });

            NeedsLocalReturnVariable(integrationBody, () => ReturnLocalReturnVariable(integrationBody, onSyntaxNode));
        }


        private static void ReturnLocalReturnVariable(IntegrationBody integrationBody, Action<SyntaxNode> onSyntaxNode)
        {
            var returnVar = integrationBody.Generator.IdentifierName("@return");
            onSyntaxNode(integrationBody.Generator.ReturnStatement(returnVar));
        }


        private static void GenerateReturnLocalVariable(IntegrationBody integrationBody, Action<SyntaxNode> onSyntaxNode)
        {
            var returnType = integrationBody.ReturnToLocalReturnVariable.First();
            var nametypes = DataStreamParser.GetOutputPart(returnType.DataNames);

            var nullabletype = TypeConverter.ConvertToType(integrationBody.Generator, nametypes, isNullable:true);


            var nullLiteral = integrationBody.Generator.NullLiteralExpression();
            onSyntaxNode(integrationBody.Generator.LocalDeclarationStatement(nullabletype, "@return", nullLiteral));
        }


        public static void NeedsLocalReturnVariable(IntegrationBody integrationBody, Action onNeeded, Action onNotNeeded = null)
        {
            if (integrationBody.ReturnToLocalReturnVariable.Count > 0)
                onNeeded();
            else
                onNotNeeded?.Invoke();
        }


        private static SyntaxNode CreateMethodCall(FunctionUnit functionUnit, IntegrationBody integrationBody)
        {
            SyntaxNode @return = null;
            var parameter = new List<SyntaxNode>();
            var methodname = MethodsGenerator.GetMethodName(functionUnit);

            AssignmentParameter_IncludingLambdaBodiesRecursive(integrationBody, functionUnit, parameter.Add);

            GetDsdThatReturns(functionUnit, returndsd =>
            {
                IsOutputOfIntegration(integrationBody, returndsd, matchingOutputDsdOfIntegration =>
                {
                    var methodcall = GenerateLocalMethodCall(integrationBody.Generator, methodname, parameter.ToArray(), null);
                    AnalayseDsd(integrationBody, matchingOutputDsdOfIntegration,
                        implementByAction: sig =>
                        {
                            var nameofAction = MethodsGenerator.GetNameOfAction(sig.DSD);
                            @return = CallAction(integrationBody.Generator, nameofAction, methodcall);
                        },
                        implementByReturn: () =>
                        {
                            NeedsLocalReturnVariable( integrationBody, 
                                onNeeded: () => @return = CallAndAssignToReturnVariable(integrationBody.Generator, methodcall),
                                onNotNeeded: () => @return = CallAndReturn(integrationBody.Generator, methodcall));
                           
                        });
                },
                onNotFound: () => // if method returns but is not output for integration -> save returned value in local variable
                {
                    var output = DataStreamParser.GetOutputPart(returndsd?.DataNames);
                    string localvariablename = GenerateLocalVariableName(output);
                    RegisterLocalVaribale(integrationBody, returndsd, localvariablename, output);
                    @return = GenerateLocalMethodCall(integrationBody.Generator, methodname, parameter.ToArray(), localvariablename);
                });
            },
            onNoReturn: () =>
                @return = GenerateLocalMethodCall(integrationBody.Generator, methodname, parameter.ToArray(), null));

            return @return;
        }


        private static SyntaxNode CallAndAssignToReturnVariable(SyntaxGenerator generator, SyntaxNode methodcall)
        {
           return SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                (ExpressionSyntax)generator.IdentifierName("@return"), (ExpressionSyntax)methodcall);

        }



        private static SyntaxNode CallAndReturn(SyntaxGenerator generator, SyntaxNode methodcall)
        {
            return generator.ReturnStatement(methodcall);
        }


        private static SyntaxNode CallAction(SyntaxGenerator generator, string nameofAction, SyntaxNode methodcall)
        {
            return generator.InvocationExpression(
                generator.IdentifierName(nameofAction), methodcall);
        }


        private static void AnalayseDsd(IntegrationBody integrationBody, DataStreamDefinition matchingOutputDsdOfIntegration,
            Action<MethodSignaturePart> implementByAction, Action implementByReturn)
        {
            var signatureOfIntegrationOutput = DataStreamParser.AnalyseOutputs(integrationBody.Integration)
                .FirstOrDefault(sig => sig.DSD == matchingOutputDsdOfIntegration);
            Debug.Assert(signatureOfIntegrationOutput != null, "signatureOfIntegrationOutput != null");
            if (signatureOfIntegrationOutput.ImplementWith == DataFlowImplementationStyle.AsAction)
                implementByAction(signatureOfIntegrationOutput);
            else
            {
                implementByReturn();
            }

        }


        private static void IsOutputOfIntegration(IntegrationBody integrationBody,
            DataStreamDefinition returndsd, Action<DataStreamDefinition> onfound, Action onNotFound)
        {
            var matchingOutputDsdOfIntegration = integrationBody.OutputOfIntegration
                .FirstOrDefault(x => x.SubFunctionUnitOutput == returndsd)?.IntegrationOutput;

            if (matchingOutputDsdOfIntegration != null)
            {
                onfound(matchingOutputDsdOfIntegration);
            }
            else
            {
                onNotFound();
            }
        }


        private static void GetDsdThatReturns(FunctionUnit functionUnit, Action<DataStreamDefinition> onReturn, Action onNoReturn)
        {
            var sigs = DataStreamParser.AnalyseOutputs(functionUnit);
            var returndsd = sigs.FirstOrDefault(sig => sig.ImplementWith == DataFlowImplementationStyle.AsReturn)?.DSD;
            if (returndsd != null)
            {
                onReturn(returndsd);
            }
            else
            {
                onNoReturn();
            }
        }


        private static SyntaxNode[] GenerateAllLambdaParameter(IntegrationBody integrationBody, DataStreamDefinition streamingOutput)
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


        private static void GenerateLambdaExpression(SyntaxGenerator generator, SyntaxNode[] lambdaParameter, SyntaxNode[] lambdaBody,
            Action<SyntaxNode> onCreated)
        {
            //integrationBody.Generator.MethodDeclaration()
            onCreated(generator.VoidReturningLambdaExpression(lambdaParameter, lambdaBody));
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

            GetAllActionOutputs(functionUnit,
                 onUnconnected: sig => AssignActionNameToParameter(integrationBody, sig, onParameterGenerated),
                 onConnected: sig =>
                 {
                     var lambdaparameter = GenerateAllLambdaParameter(integrationBody, sig.DSD);
                     var lambdabody = new List<SyntaxNode>();
                     GetAllFunctionUnitsThatAreInsideThisLambda(integrationBody, sig, fu =>
                     {
                         var mc = CreateMethodCall(fu, integrationBody);
                         lambdabody.Add(mc);
                     });

                     SyntaxNode lambdaExpression = null;
                     GenerateLambdaExpression(integrationBody.Generator, lambdaparameter, lambdabody.ToArray(), node => lambdaExpression = node);

                     SyntaxNode arg = null;
                     // Minor design decision here:
                     // When action name is provided use "named arguments" when calling this method
                     // if not provided but output needs to be implemented with action don't use named arguments             
                     IsActionNameDefined(sig.DSD,
                         onDefined: () =>
                         {
                             var argumentName = MethodsGenerator.GetNameOfAction(sig.DSD);
                             arg = GenerateArgument(integrationBody.Generator, argumentName, lambdaExpression);
                         },
                         onUndefined: () =>
                            arg = GenerateArgument(integrationBody.Generator, null, lambdaExpression));


                     onParameterGenerated(arg);
                 });
        }


        private static SyntaxNode GenerateArgument(SyntaxGenerator generator, string argumentname, SyntaxNode expression)
        {
            return string.IsNullOrWhiteSpace(argumentname)
                ? generator.Argument(expression)
                : generator.Argument(argumentname, RefKind.None, expression);
        }


        public static void IsActionNameDefined(DataStreamDefinition dsd, Action onDefined, Action onUndefined)
        {
            if (string.IsNullOrWhiteSpace(dsd.ActionName))
                onUndefined();
            else
                onDefined();
        }


        public static void GetAllFunctionUnitsThatAreInsideThisLambda(IntegrationBody integrationBody, MethodSignaturePart sig, Action<FunctionUnit> onEach)
        {
            var functionUnitsToGenerateInsideThisLambda =
                integrationBody.LambdaBodies.Where(lb => lb.InsideLambdaOf == sig.DSD).Select(x => x.FunctionUnit).ToList();

            functionUnitsToGenerateInsideThisLambda.ForEach(fu =>
            {
                onEach(fu);
            });

        }


        private static void AssignActionNameToParameter(IntegrationBody integrationBody, MethodSignaturePart sig, Action<SyntaxNode> onParameterGenerated)
        {
            var integrationActionOutput = GetMatchingActionFromIntegration(integrationBody, sig,
                onError: () =>
                {
                    throw new UnnconnectedOutputException(sig.DSD);
                });

            var actionname = MethodsGenerator.GetNameOfAction(integrationActionOutput);
            onParameterGenerated(integrationBody.Generator.IdentifierName(actionname));
        }


        public static DataStreamDefinition GetMatchingActionFromIntegration(
            IntegrationBody integrationBody, MethodSignaturePart sig, Action onError)
        {
            var tupel = integrationBody.OutputOfIntegration.FirstOrDefault(x => x.SubFunctionUnitOutput == sig.DSD);
            if (tupel == null)
                onError();

            return tupel?.IntegrationOutput;
        }


        public static void GetAllActionOutputs(FunctionUnit functionUnit,
            Action<MethodSignaturePart> onConnected,
            Action<MethodSignaturePart> onUnconnected
            )
        {
            var sigs = DataStreamParser.AnalyseOutputs(functionUnit);
            var asAction = sigs.Where(x => x.ImplementWith == DataFlowImplementationStyle.AsAction);
            asAction.ForEach(sig =>
            {
                if (!sig.DSD.Connected)
                    onUnconnected(sig);
                else
                {
                    onConnected(sig);
                }

            });

        }


        public static void GenereteArgumentsFromInput(IntegrationBody integrationBody, FunctionUnit functionUnit,
            Action<SyntaxNode> onParameterGenerated)
        {
            var inputsDsd = functionUnit.InputStreams.First();
            var inputs = DataStreamParser.GetInputPart(inputsDsd.DataNames);
            var dependecies = integrationBody.CallDependecies.First(x => x.OfFunctionUnit == functionUnit);

            inputs.ToList().ForEach(neededNt =>
            {
                var parameter = dependecies.Parameters.FirstOrDefault(x => IsMatchingNameType(x.NeededNameType, neededNt));
                var varname = GetLocalVariable(integrationBody, parameter, neededNt, inputsDsd)?.VariableName;
                var arg = integrationBody.Generator.Argument(integrationBody.Generator.IdentifierName(varname));
                onParameterGenerated(arg);
            });

        }

        private static GeneratedLocalVariable GetLocalVariable(IntegrationBody integrationBody, Parameter parameter, NameType needed, DataStreamDefinition inputDsd)
        {
            if (parameter == null)
                throw new MissingInputDataException(inputDsd, needed);

            var found = integrationBody.LocalVariables.FirstOrDefault(y => y.Source == parameter.Source
            && y.NameTypes.First().Type == needed.Type
            && y.NameTypes.First().Name == needed.Name);

            if (found == null)
                throw new MissingInputDataException(inputDsd, needed);

            return found;
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


        public static bool IsMatchingNameType(NameType lookingForNameType, NameType nt)
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
            AddIntegrationInputParameterToLocalScope(body, integration);
            AnalyseParameterDependencies(body);


            FunctionUnit beginning = null;

            MainModelManager.GetBeginningOfFlow(integration.IsIntegrating.First(), mainModel)
                    .TryCast<DataStreamDefinition>(dsd => beginning = dsd.Parent);

            onInLambdaBody(new LambdaBody { FunctionUnit = beginning, InsideLambdaOf = null });

            var outputSignature = DataStreamParser.AnalyseOutputs(beginning);

            var asAction = outputSignature
                .Where(sig => sig.ImplementWith == DataFlowImplementationStyle.AsAction)
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
                sig => sig.ImplementWith == DataFlowImplementationStyle.AsReturn);


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
        public List<MatchingOutputs> OutputOfIntegration { get; set; }
        public List<DataStreamDefinition> ReturnToLocalReturnVariable { get; set; }
    }


    public class MatchingOutputs
    {
        public DataStreamDefinition IntegrationOutput;
        public DataStreamDefinition SubFunctionUnitOutput;
    }
}