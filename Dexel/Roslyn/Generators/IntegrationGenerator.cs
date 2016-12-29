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
using Roslyn.Analyser;
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
            var integrationBody = IntegrationAnalyser.CreateNewIntegrationBody(mainModel.Connections, integration);
            AddIntegrationInputParameterToLocalScope(integrationBody, integration);

            // analyse data flow before generation 
            IntegrationAnalyser.AnalyseParameterDependencies(integrationBody);
            IntegrationAnalyser.AnalyseLambdaBodies(integrationBody, mainModel);
            IntegrationAnalyser.AnalyseMatchingOutputOfIntegration(integrationBody, mainModel);
            IntegrationAnalyser.AnalyseReturnToLocalReturnVariable(integrationBody, mainModel);

            // generation
            var result = new List<SyntaxNode>();
            integrationBody.Generator = generator;
            GenerateBody(integrationBody, result.Add);

            // when generating integration signature, some information from the body is needed: is returning local variable nullable type
            getIntegrationBody?.Invoke(integrationBody);

            return result.ToArray();
        }


        public static void AddIntegrationInputParameterToLocalScope(IntegrationBody integrationBody, FunctionUnit integration)
        {
            var nametypes = DataStreamParser.GetInputPart(integration.InputStreams.First().DataNames);
            nametypes.ToList().ForEach(nametype =>
            {
                var name = Names.ParameterName(nametype);
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
                var name = Names.ParameterName(nametype);
                integrationBody.LocalVariables.Add(new GeneratedLocalVariable
                {
                    VariableName = name,
                    Source = integration.InputStreams.First(),
                    NameTypes = new[] { nametype }
                });
            });
        }


        private static void GenerateBody(IntegrationBody integrationBody, Action<SyntaxNode> onSyntaxNode)
        {
            IntegrationAnalyser.NeedsLocalReturnVariable(integrationBody, () => GenerateReturnLocalVariable(integrationBody, onSyntaxNode));

            var toplevelCalls = integrationBody.LambdaBodies.Where(x => x.InsideLambdaOf == null).ToList();
            toplevelCalls.ForEach(c =>
            {
                var syntaxnode = CreateMethodCall(c.FunctionUnit, integrationBody);
                onSyntaxNode(syntaxnode);
            });

            IntegrationAnalyser.NeedsLocalReturnVariable(integrationBody, () => ReturnLocalReturnVariable(integrationBody, onSyntaxNode));
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


        private static SyntaxNode CreateMethodCall(FunctionUnit functionUnit, IntegrationBody integrationBody)
        {
            SyntaxNode @return = null;
            var parameter = new List<SyntaxNode>();
            var methodname = Names.MethodName(functionUnit);

            AssignmentParameter_IncludingLambdaBodiesRecursive(integrationBody, functionUnit, parameter.Add);

            FunctionUnitAnalyser.GetDsdThatReturns(functionUnit, returndsd =>
            {
                IntegrationAnalyser.IsOutputOfIntegration(integrationBody, returndsd, matchingOutputDsdOfIntegration =>
                {
                    var methodcall = GenerateLocalMethodCall(integrationBody.Generator, methodname, parameter.ToArray(), null);
                    IntegrationAnalyser.IntegrationOutput(integrationBody, matchingOutputDsdOfIntegration,
                        implementByAction: sig =>
                        {
                            var nameofAction = Names.NewAction(sig.DSD);
                            @return = CallAction(integrationBody.Generator, nameofAction, methodcall);
                        },
                        implementByReturn: () =>
                        {
                            IntegrationAnalyser.NeedsLocalReturnVariable( integrationBody, 
                                onNeeded: () => @return = CallAndAssignToReturnVariable(integrationBody.Generator, methodcall),
                                onNotNeeded: () => @return = CallAndReturn(integrationBody.Generator, methodcall));
                           
                        });
                },
                onNotFound: () => // if method returns but is not output for integration -> save returned value in local variable
                {
                    var output = DataStreamParser.GetOutputPart(returndsd?.DataNames);
                    string localvariablename = Names.NewLocalVariable(output);
                    RegisterLocalVariable(integrationBody, returndsd, localvariablename, output);
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


        private static SyntaxNode[] GenerateAllLambdaParameter(IntegrationBody integrationBody, DataStreamDefinition streamingOutput, Action<List<string>> getNames = null)
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

            if (lambdaNames.Any())
                getNames?.Invoke(lambdaNames);

            return lambdaNames.Select(name => integrationBody.Generator.LambdaParameter(name)).ToArray();
        }


        private static void GenerateLambdaExpression(SyntaxGenerator generator, SyntaxNode[] lambdaParameter, SyntaxNode[] lambdaBody,
            Action<SyntaxNode> onCreated)
        {
            //integrationBody.Generator.MethodDeclaration()
            onCreated(generator.VoidReturningLambdaExpression(lambdaParameter, lambdaBody));
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




        private static void RegisterLocalVariable(IntegrationBody integrationBody, DataStreamDefinition dsd, string localname,
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

            FunctionUnitAnalyser.GetAllActionOutputs(functionUnit,
                 onUnconnected: sig => 
                    IntegrationAnalyser.IsOutputOfIntegration(integrationBody, sig.DSD, integrationDsd => 
                        IntegrationAnalyser.IntegrationOutput(integrationBody, integrationDsd, 
                            implementByAction: s => AssignActionNameToParameter(integrationBody, sig, onParameterGenerated),
                            implementByReturn: () => LambdaThatAssignsToLocalReturnVariable(integrationBody, sig.DSD, integrationDsd, onParameterGenerated))),               
                 onConnected: sig =>
                 {
                     var lambdaparameter = GenerateAllLambdaParameter(integrationBody, sig.DSD);
                     var lambdabody = new List<SyntaxNode>();
                     IntegrationAnalyser.GetAllFunctionUnitsThatAreInsideThisLambda(integrationBody, sig, fu =>
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
                     OutputAnalyser.IsActionNameDefined(sig.DSD,
                         onDefined: () =>
                         {
                             var argumentName = Names.NewAction(sig.DSD);
                             arg = GenerateArgument(integrationBody.Generator, argumentName, lambdaExpression);
                         },
                         onUndefined: () =>
                            arg = GenerateArgument(integrationBody.Generator, null, lambdaExpression));


                     onParameterGenerated(arg);
                 });
        }


        private static void LambdaThatAssignsToLocalReturnVariable(IntegrationBody integrationBody, DataStreamDefinition subDsd, DataStreamDefinition integrationDsd, Action<SyntaxNode> onParameterGenerated)
        {
            SyntaxNode body = null;
            var lambdaparameter = GenerateAllLambdaParameter(integrationBody, subDsd, 
                getNames: names => body =  FromLambdaParameterToReturnVariable(integrationBody, names));
           
            SyntaxNode lambdaExpression = null;
            GenerateLambdaExpression(integrationBody.Generator, lambdaparameter, new [] {body}, node => lambdaExpression = node);
            onParameterGenerated(GenerateArgument(integrationBody.Generator, null, lambdaExpression));
        }


        private static SyntaxNode FromLambdaParameterToReturnVariable(IntegrationBody integrationBody, List<string> names)
        {
            if (names.Count > 1)
                return integrationBody.Generator.IdentifierName("@return =  // TODO ");

            return integrationBody.Generator.IdentifierName("@return = "+names.First());
        }


        private static SyntaxNode GenerateArgument(SyntaxGenerator generator, string argumentname, SyntaxNode expression)
        {
            return string.IsNullOrWhiteSpace(argumentname)
                ? generator.Argument(expression)
                : generator.Argument(argumentname, RefKind.None, expression);
        }


        private static void AssignActionNameToParameter(IntegrationBody integrationBody, MethodSignaturePart sig, Action<SyntaxNode> onParameterGenerated)
        {
            var integrationActionOutput = IntegrationAnalyser.GetMatchingActionFromIntegration(integrationBody, sig,
                onError: () =>
                {
                    throw new UnnconnectedOutputException(sig.DSD);
                });

            var actionname = Names.NewAction(integrationActionOutput);
            onParameterGenerated(integrationBody.Generator.IdentifierName(actionname));
        }


        public static void GenereteArgumentsFromInput(IntegrationBody integrationBody, FunctionUnit functionUnit,
            Action<SyntaxNode> onParameterGenerated)
        {
            var inputsDsd = functionUnit.InputStreams.First();
            var inputs = DataStreamParser.GetInputPart(inputsDsd.DataNames);
            var dependecies = integrationBody.CallDependecies.First(x => x.OfFunctionUnit == functionUnit);

            inputs.ToList().ForEach(neededNt =>
            {
                var varname = GetLocalVariable(integrationBody, dependecies, neededNt, inputsDsd);
                var arg = integrationBody.Generator.Argument(integrationBody.Generator.IdentifierName(varname));
                onParameterGenerated(arg);
            });

        }


        private static string GetLocalVariable(IntegrationBody integrationBody, MethodWithParameterDependencies dependecies,
            NameType neededNt, DataStreamDefinition inputsDsd)
        {
            var parameter = dependecies.Parameters.FirstOrDefault(x => IsMatchingNameType(x.NeededNameType, neededNt));
            var varname = GetLocalVariable(integrationBody, parameter, neededNt, inputsDsd)?.VariableName;
            return varname;
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


        public static bool IsMatchingNameType(NameType nt1, NameType nt2)
        {
            return (nt2.Type == nt1.Type)
                   && (nt2.IsArray == nt1.IsArray)
                   && (nt2.IsList == nt1.IsList)
                   && (nt2.Name == nt1.Name);
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


        
    }





}