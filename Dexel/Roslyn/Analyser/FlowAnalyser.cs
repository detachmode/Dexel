using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Library;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;

namespace Roslyn.Analyser
{
    public static class FlowAnalyser
    {
        public static void TravelIntegration(FunctionUnit integration, MainModel mainModel, Action<LambdaBody> onInLambdaBody)
        {

            var body = IntegrationAnalyser.CreateNewIntegrationBody(mainModel.Connections, integration);
            IntegrationGenerator.AddIntegrationInputParameterToLocalScope(body, integration);
            IntegrationAnalyser.AnalyseParameterDependencies(body);


            FunctionUnit beginning = null;

            MainModelManager.GetBeginningOfFlow(integration.IsIntegrating.First(), mainModel)
                .TryCast<DataStreamDefinition>(dsd => beginning = dsd.Parent);

            onInLambdaBody(new LambdaBody { FunctionUnit = beginning, InsideLambdaOf = null });

            var outputSignature = OutputAnalyser.AnalyseOutputs(beginning);

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


        public static void TravelOutputs(FunctionUnit currentFunctionUnit, object bodytoReturnTo, MainModel mainModel,
            Action<DataStreamDefinition, FunctionUnit> onAsReturn,
            Action<DataStreamDefinition, FunctionUnit> onAsAction)
        {

            var outputSignature = OutputAnalyser.AnalyseOutputs(currentFunctionUnit);

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
                .Select<NameType, Parameter>(nt => FindOneParameter(nt, parent, connections, ofFunctionUnit, true)).ToList();


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
                    if (!Enumerable.Any<NameType>(found) || parameter.FoundFlag != Found.NotFound)
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
                var found = inputnametypes.Where(nt => IntegrationGenerator.IsMatchingNameType(parameter.NeededNameType, nt)).ToList();

                if (found.Any())
                    parameter.FoundFlag = Found.FromParent;
            }
            else
            {
                var foundInParent = parent?.InputStreams.Select(dsd =>
                {
                    var inputnametypes = DataStreamParser.GetOutputPart(dsd.DataNames).ToList();
                    var found = inputnametypes
                        .Where(nt => IntegrationGenerator.IsMatchingNameType(parameter.NeededNameType, nt))
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
                .Where(nt => IntegrationGenerator.IsMatchingNameType(lookingForNameType, nt))
                .ToList();

            return found;
        }
    }
}
