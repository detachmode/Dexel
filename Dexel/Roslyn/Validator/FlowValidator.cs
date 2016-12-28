using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Roslyn.Generators;

namespace Roslyn.Validator
{
    public static class FlowValidator
    {
        public static void Validate(MainModel mainModel, Action<object> onError)
        {
            mainModel.FunctionUnits.Where(sc => sc.IsIntegrating.Count > 0).ToList().ForEach(isc =>
            {
                Validate(isc, mainModel, onError);
            });
        }

        private static void Validate(FunctionUnit integration, MainModel mainModel, Action<object> onError)
        {
            // integrationbody object for storing analysed and generated information
            var integrationBody = IntegrationGenerator.CreateNewIntegrationBody(mainModel.Connections, integration);
            IntegrationGenerator.AddIntegrationInputParameterToLocalScope(integrationBody, integration);

            // analyse data flow before generation 
            IntegrationGenerator.AnalyseParameterDependencies(integrationBody);
            IntegrationGenerator.AnalyseLambdaBodies(integrationBody, mainModel);
            IntegrationGenerator.AnalyseMatchingOutputOfIntegration(integrationBody, mainModel);
            IntegrationGenerator.AnalyseReturnToLocalReturnVariable(integrationBody, mainModel);

            var toplevelCalls = integrationBody.LambdaBodies.Where(x => x.InsideLambdaOf == null).ToList();
            toplevelCalls.ForEach(c =>
            {
                ValidateMethodCall(c.FunctionUnit, integrationBody, onError);
            });
        }


        private static void ValidateMethodCall(FunctionUnit functionUnit, IntegrationBody integrationBody, Action<object> onError)
        {
           ValidateInputs(functionUnit, integrationBody, onError);

            IntegrationGenerator.GetAllActionOutputs(functionUnit,
                onUnconnected: sig => ValidateUnconnectedOutput(integrationBody, sig, onError),
                onConnected: sig =>
                {
                    IntegrationGenerator.GetAllFunctionUnitsThatAreInsideThisLambda(integrationBody, sig, 
                        fu => ValidateMethodCall(fu, integrationBody, onError));
                });
        }


        private static void ValidateUnconnectedOutput(IntegrationBody integrationBody, MethodSignaturePart sig, Action<object> onError)
        {
            IntegrationGenerator.GetMatchingActionFromIntegration(integrationBody, sig, 
                onError: () => onError(new ValidationErrorUnnconnectedOutput(sig.DSD, integrationBody.Integration)) );
              
        }


        private static void ValidateInputs(FunctionUnit functionUnit, IntegrationBody integrationBody, Action<object> onError)
        {
            var inputsDsd = functionUnit.InputStreams.First();
            var inputs = DataStreamParser.GetInputPart(inputsDsd.DataNames);
            var dependecies = integrationBody.CallDependecies.First(x => x.OfFunctionUnit == functionUnit);

            inputs.ToList().ForEach(neededNt =>
            {
                var foundParameter = dependecies.Parameters.FirstOrDefault(x => IntegrationGenerator.IsMatchingNameType(x.NeededNameType, neededNt));
                if (foundParameter?.FoundFlag != Found.NotFound) return;

                var error = new ValidationErrorInputMissing(functionUnit, neededNt);
                onError(error);
            });
        }
    }


    public class ValidationErrorUnnconnectedOutput
    {
        public ValidationErrorUnnconnectedOutput(DataStreamDefinition dataStreamDefinition, FunctionUnit integration)
        {
            UnnconnectedOutput = dataStreamDefinition;
            Integration = integration;
        }

        public FunctionUnit Integration { get; set; }
        public DataStreamDefinition UnnconnectedOutput { get; set; }
    }


    public class ValidationErrorInputMissing
    {

        public ValidationErrorInputMissing(FunctionUnit functionUnit, NameType neededNt)
        {
            invalidFunctionUnit = functionUnit;
            notFoundNameType = neededNt;
        }

        public NameType notFoundNameType { get; set; }
        public FunctionUnit invalidFunctionUnit { get; set; }
    }
}
