using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dexel.Library;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis.Editing;
using Roslyn.Analyser;

namespace Roslyn
{
    public enum ImplementationMatch
    {
        BothAction,
        BothReturn,
        OnlySubFunctionAction,
        OnlyIntegrationAction

    }

    public class MatchingOutputs
    {
        public DataStreamDefinition IntegrationOutput;
        public DataStreamDefinition SubFunctionUnitOutput;
        public ImplementationMatch ImplementationMatch;
    }

    public class LambdaBody
    {
        public DataStreamDefinition InsideLambdaOf;
        public FunctionUnit FunctionUnit;
    }

    public class IntegrationBody
    {
        public List<FunctionUnit> ChildrenToGenerate;
        public List<DataStream> Connections;
        public FunctionUnit Integration;
        public SyntaxGenerator Generator;
        public List<GeneratedLocalVariable> LocalVariables;
        public List<MethodWithParameterDependencies> CallDependecies { get; set; }
        public List<LambdaBody> LambdaBodies { get; set; }
        public List<MatchingOutputs> OutputOfIntegration { get; set; }
        public List<DataStreamDefinition> ReturnToLocalReturnVariable { get; set; }
    }


    public static class IntegrationAnalyser
    {
        public static void AnalyseParameterDependencies(IntegrationBody integrationBody)
        {
            integrationBody.CallDependecies = integrationBody.ChildrenToGenerate.Select(sc => new MethodWithParameterDependencies
            {
                OfFunctionUnit = sc,
                Parameters = FlowAnalyser.FindParameters(sc, integrationBody.Connections, integrationBody.Integration)
            }).ToList();
        }


        public static void AnalyseLambdaBodies(IntegrationBody integrationBody, MainModel mainModel)
        {
            integrationBody.LambdaBodies = new List<LambdaBody>();

            FlowAnalyser.TravelIntegration(integrationBody.Integration, mainModel,
                onInLambdaBody: integrationBody.LambdaBodies.Add);
        }



        public static void AnalyseMatchingOutputOfIntegration(IntegrationBody body, MainModel mainModel)
        {
            var sigs = OutputAnalyser.AnalyseOutputs(body.Integration);

            var allNotConnectedOutputs = body.Integration.IsIntegrating
                .SelectMany(fu => fu.OutputStreams.Where(x => !x.Connected)).ToList();

            var allmatches = sigs.SelectMany(sig =>
            {
                var actionOfIntegration = sig.DSD;
                var implementationIntegrationOut = sig.ImplementWith;

                var allmatching = allNotConnectedOutputs
                    .Where(dsd => !(String.IsNullOrWhiteSpace(dsd.ActionName) && dsd.DataNames.Trim() == "()"))
                    .Where(dsd => AreEqualDsdsOutputs(dsd, actionOfIntegration));

                return allmatching.Select(dsd =>
                {
                    var resultImplementMatch = CompareImplementationStyles(dsd, implementationIntegrationOut);

                    return new MatchingOutputs
                    {
                        IntegrationOutput = actionOfIntegration,
                        SubFunctionUnitOutput = dsd,
                        ImplementationMatch = resultImplementMatch                      
                    };
                });

            }).ToList();

            body.OutputOfIntegration = allmatches;
        }


        private static ImplementationMatch CompareImplementationStyles(DataStreamDefinition dsd,
            DataFlowImplementationStyle implementationIntegrationOut)
        {
            var sigOfSub = OutputAnalyser.AnalyseOutputs(dsd.Parent);
            var implementSubOut = sigOfSub.First(s => s.DSD == dsd).ImplementWith;
            ImplementationMatch resultImplementMatch = ImplementationMatch.BothAction;
            if (implementationIntegrationOut == implementSubOut)
            {
                if (implementSubOut == DataFlowImplementationStyle.AsReturn)
                {
                    resultImplementMatch = ImplementationMatch.BothReturn;
                }
            }
            else
            {
                resultImplementMatch = implementSubOut == DataFlowImplementationStyle.AsAction
                    ? ImplementationMatch.OnlySubFunctionAction
                    : ImplementationMatch.OnlyIntegrationAction;
            }
            return resultImplementMatch;
        }


        private static bool AreEqualDsdsOutputs(DataStreamDefinition dsd, DataStreamDefinition dsd2)
        {
            var nt1 = DataStreamParser.GetOutputPart(dsd.DataNames);
            var nt2 = DataStreamParser.GetOutputPart(dsd2.DataNames);
            if (nt1.Count != nt2.Count)
                return false;

            for (int i = 0; i < nt1.Count; i++)
                if (!IntegrationGenerator.IsMatchingNameType(nt1[i], nt2[i]))
                    return false;

            return dsd.ActionName == dsd2.ActionName ||
                    (string.IsNullOrWhiteSpace(dsd.ActionName) && string.IsNullOrWhiteSpace(dsd2.ActionName));
        }


        public static void AnalyseReturnToLocalReturnVariable(IntegrationBody integrationBody, MainModel mainModel)
        {
            integrationBody.ReturnToLocalReturnVariable =
                integrationBody.LambdaBodies.Where(x => x.InsideLambdaOf != null).Select(x =>
                {
                    var sigs = OutputAnalyser.AnalyseOutputs(x.FunctionUnit);
                    var sigsOfIntegration = OutputAnalyser.AnalyseOutputs(integrationBody.Integration);
                    var dsdThatReturnsFromSubFunctionUnit = sigs.FirstOrDefault(y => y.ImplementWith == DataFlowImplementationStyle.AsReturn)?.DSD;
                    var dsdThatReturnsFromIntegration = sigsOfIntegration.FirstOrDefault(y => y.ImplementWith == DataFlowImplementationStyle.AsReturn)?.DSD;

                    var found = integrationBody.OutputOfIntegration
                        .FirstOrDefault(matchingOutputs =>
                            matchingOutputs.SubFunctionUnitOutput == dsdThatReturnsFromSubFunctionUnit
                            && matchingOutputs.IntegrationOutput == dsdThatReturnsFromIntegration);

                    return found?.SubFunctionUnitOutput;

                }).Where(x => x != null).ToList();

            integrationBody.OutputOfIntegration
                .Where(x => x.ImplementationMatch == ImplementationMatch.OnlySubFunctionAction).ToList()
                .ForEach(x => integrationBody.ReturnToLocalReturnVariable.Add(x.SubFunctionUnitOutput));
        }


        public static void NeedsLocalReturnVariable(IntegrationBody integrationBody, Action onNeeded, Action onNotNeeded = null)
        {
            if (integrationBody.ReturnToLocalReturnVariable.Count > 0)
                onNeeded();
            else
                onNotNeeded?.Invoke();
        }


        public static void IntegrationOutput(IntegrationBody integrationBody, DataStreamDefinition integrationDsd,
            Action<MethodSignaturePart> implementByAction = null, Action implementByReturn = null)
        {
            var signatureOfIntegrationOutput = OutputAnalyser.AnalyseOutputs(integrationBody.Integration)
                .FirstOrDefault(sig => sig.DSD == integrationDsd);
            Debug.Assert(signatureOfIntegrationOutput != null, "signatureOfIntegrationOutput != null");
            if (signatureOfIntegrationOutput.ImplementWith == DataFlowImplementationStyle.AsAction)
                implementByAction?.Invoke(signatureOfIntegrationOutput);
            else
            {
                implementByReturn?.Invoke();
            }

        }


        public static void IsOutputOfIntegration(IntegrationBody integrationBody,
            DataStreamDefinition returndsd, Action<DataStreamDefinition> onfound = null, Action onNotFound = null)
        {
            var matchingOutputDsdOfIntegration = integrationBody.OutputOfIntegration
                .FirstOrDefault(x => x.SubFunctionUnitOutput == returndsd)?.IntegrationOutput;

            if (matchingOutputDsdOfIntegration != null)
            {
                onfound?.Invoke(matchingOutputDsdOfIntegration);
            }
            else
            {
                onNotFound?.Invoke();
            }
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


        public static void GetAllFunctionUnitsThatAreInsideThisLambda(IntegrationBody integrationBody, MethodSignaturePart sig, Action<FunctionUnit> onEach)
        {
            var functionUnitsToGenerateInsideThisLambda =
                integrationBody.LambdaBodies.Where(lb => lb.InsideLambdaOf == sig.DSD).Select(x => x.FunctionUnit).ToList();

            functionUnitsToGenerateInsideThisLambda.ForEach(fu =>
            {
                onEach(fu);
            });

        }


        public static DataStreamDefinition GetMatchingActionFromIntegration(
            IntegrationBody integrationBody, MethodSignaturePart sig, Action onError)
        {
            var tupel = integrationBody.OutputOfIntegration.FirstOrDefault(x => x.SubFunctionUnitOutput == sig.DSD);
            if (tupel == null)
                onError();

            return tupel?.IntegrationOutput;
        }
    }
}
