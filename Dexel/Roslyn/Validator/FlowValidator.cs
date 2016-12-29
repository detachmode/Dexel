using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Roslyn.Analyser;
using Roslyn.Generators;

namespace Roslyn.Validator
{
    public static class FlowValidator
    {
        public static void Validate(MainModel mainModel, Action<ValidationError> onErrorOrWarning)
        {
            mainModel.FunctionUnits.Where(sc => sc.IsIntegrating.Count > 0).ToList().ForEach(isc =>
            {
                Validate(isc, mainModel, onErrorOrWarning);
            });
        }

        private static void Validate(FunctionUnit integration, MainModel mainModel, Action<ValidationError> onError)
        {
            // integrationbody object for storing analysed and generated information
            var integrationBody = IntegrationAnalyser.CreateNewIntegrationBody(mainModel.Connections, integration);
            IntegrationGenerator.AddIntegrationInputParameterToLocalScope(integrationBody, integration);

            // analyse data flow before generation 
            IntegrationAnalyser.AnalyseParameterDependencies(integrationBody);
            IntegrationAnalyser.AnalyseLambdaBodies(integrationBody, mainModel);
            IntegrationAnalyser.AnalyseMatchingOutputOfIntegration(integrationBody, mainModel);
            IntegrationAnalyser.AnalyseReturnToLocalReturnVariable(integrationBody, mainModel);

            var toplevelCalls = integrationBody.LambdaBodies.Where(x => x.InsideLambdaOf == null).ToList();
            toplevelCalls.ForEach(c =>
            {
                ValidateMethodCall(c.FunctionUnit, integrationBody, onError);
            });
        }


        private static void ValidateMethodCall(FunctionUnit functionUnit, IntegrationBody integrationBody, Action<ValidationError> onErrorOrWarning)
        {
            ValidateInputs(functionUnit, integrationBody, onErrorOrWarning);

            FunctionUnitAnalyser.GetAllActionOutputs(functionUnit,
                onUnconnected: sig => ValidateUnconnectedActionOutput(integrationBody, sig, onErrorOrWarning),
                onConnected: sig =>
                {
                    IntegrationAnalyser.GetAllFunctionUnitsThatAreInsideThisLambda(integrationBody, sig,
                        fu => ValidateMethodCall(fu, integrationBody, onErrorOrWarning));
                });

            FunctionUnitAnalyser.GetDsdThatReturns(functionUnit, returndsd =>
            {
                if (returndsd.DataNames.Trim() == "()")
                    return; // no data 

                IntegrationAnalyser.IsOutputOfIntegration(integrationBody, returndsd, onNotFound: () =>
                 {
                     var res = integrationBody.CallDependecies.FirstOrDefault(cd => cd.Parameters.Any(p => p.Source == returndsd));
                     if (res == null)
                         onErrorOrWarning(new ValidationWarningUnusedVariable(returndsd));
                 });
            });
        }


        private static void ValidateUnconnectedActionOutput(IntegrationBody integrationBody, MethodSignaturePart sig, Action<ValidationError> onErrorOrWarning)
        {
            IntegrationAnalyser.GetMatchingActionFromIntegration(integrationBody, sig,
                onError: () => onErrorOrWarning(new ValidationErrorUnnconnectedOutput(sig.DSD, integrationBody.Integration)));


        }


        private static void ValidateInputs(FunctionUnit functionUnit, IntegrationBody integrationBody, Action<ValidationError> onError)
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
    public enum TypeOfError
    {
        Warning,
        Error
    }

    public abstract class ValidationError
    {
        public TypeOfError TypeOfError = TypeOfError.Error; 
        public List<Tuple<object, string>> HighlightObjects = new List<Tuple<object, string>>();
        public void Add(object obj, string message) => HighlightObjects.Add(new Tuple<object, string>(obj, message));
    }


    public class ValidationErrorUnnconnectedOutput : ValidationError
    {
        public ValidationErrorUnnconnectedOutput(DataStreamDefinition dataStreamDefinition, FunctionUnit integration)
        {
            UnnconnectedOutput = dataStreamDefinition;
            Integration = integration;

            var dsd = UnnconnectedOutput;
            var msg = "Unconnected output inside integration:\n" +
                      $"function unit: {dsd.Parent.Name} \n" +
                      $"output:{dsd.DataNames} actionname:{dsd.ActionName}\n";

            Add(dataStreamDefinition, "Needs to be connected or matching integration output");
            Add(integration, msg);

        }

        public FunctionUnit Integration { get; set; }
        public DataStreamDefinition UnnconnectedOutput { get; set; }

    }

    public class ValidationWarningUnusedVariable : ValidationError
    {
        public ValidationWarningUnusedVariable(DataStreamDefinition dataStreamDefinition)
        {
            TypeOfError = TypeOfError.Warning;
            UnusedOutput = dataStreamDefinition;
            Add(dataStreamDefinition, "Unused variable");
        }
        public DataStreamDefinition UnusedOutput { get; set; }

    }



    public class ValidationErrorInputMissing : ValidationError
    {

        public ValidationErrorInputMissing(FunctionUnit functionUnit, NameType neededNt)
        {
            
            invalidFunctionUnit = functionUnit;
            notFoundNameType = neededNt;
            var nt = notFoundNameType;
            var container = nt.IsList ? "*" : nt.IsArray ? "[]" : "";
            var msg = $"Missing input data: \n ->  {nt.Name}:{nt.Type}{container}\n";
            Add(functionUnit, msg);

        }

        public NameType notFoundNameType { get; set; }
        public FunctionUnit invalidFunctionUnit { get; set; }

    }
}
