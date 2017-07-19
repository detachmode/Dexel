using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using CodeAnalyser;
using Dexel.Editor.Common;
using Dexel.Editor.FileIO;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views.CustomControls;
using Dexel.Library;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Roslyn;
using Roslyn.Generators;
using Roslyn.Parser;
using Roslyn.Validator;

namespace Dexel.Editor.Views
{




    public static class Interactions
    {
        public enum DragMode
        {
            Single,
            Multiple
        }


        private static Timer _aTimer;


        private static readonly List<FunctionUnit> CopiedFunctionUnits = new List<FunctionUnit>();
        public static bool PickState;


        private static FunctionUnit _startPickingAt;


        public static void ChangeToPrintTheme(MainViewModel mainViewModel)
        {
            mainViewModel.ChangeTheme("Views/Themes/Print.xaml", @"Views/Themes/FlowDesignColorPrint.xshd");
            App.SetConfig("Theme", "Print");

        }


        public static void ChangeToDarkTheme(MainViewModel mainViewModel)
        {
            mainViewModel.ChangeTheme("Views/Themes/DarkColorfull.xaml", @"Views/Themes/FlowDesignColorDark.xshd");
            App.SetConfig("Theme", "Dark");
        }


        public static FunctionUnit AddNewFunctionUnit(MainViewModel mainViewModel, Point pos)
        {
            var functionUnit = FunctionUnitManager.CreateNew();
            pos.X -= 100;
            pos.Y -= 20;
            functionUnit.Position = new Point(pos.X, pos.Y);
            functionUnit.InputStreams.Add(DataStreamManager.NewDefinition(functionUnit, "()"));
            functionUnit.OutputStreams.Add(DataStreamManager.NewDefinition(functionUnit, "()"));

            mainViewModel.Model.FunctionUnits.Add(functionUnit);
            ViewRedraw(mainViewModel);
            return functionUnit;
        }


        public static void ViewRedraw(MainViewModel mainViewModel)
        {
            Validate(mainViewModel);
            mainViewModel.Reload();
        }


        public static void ViewRedraw(MainViewModel mainViewModel, MainModel newModel)
        {
            mainViewModel.Model = newModel;
            mainViewModel.Reload();
        }


        public static void ChangeConnectionDestination(MainViewModel mainViewModel, DataStream dataStream, FunctionUnit newdestination)
        {

            DeConnect(mainViewModel, dataStream);
            MainModelManager.ConnectTwoDefintions(dataStream.Sources.First(), newdestination.InputStreams.First(),
                mainViewModel.Model);

            ViewRedraw(mainViewModel);
        }


        public static void AddNewOutput(MainViewModel mainViewModel, FunctionUnit functionUnit, string datanames)
        {
            MainModelManager.AddNewOutput(functionUnit, datanames);
            ViewRedraw(mainViewModel);
        }


        public static void AddNewInput(MainViewModel mainViewModel, FunctionUnit functionUnit, string datanames)
        {
            MainModelManager.AddNewInput(functionUnit, datanames);
            ViewRedraw(mainViewModel);
        }


        public static void MoveFunctionUnit(FunctionUnit model, double horizontalChange, double verticalChange)
        {
            if (model == null)
            {
                return;
            }
            var pos = model.Position;
            pos.X += horizontalChange;
            pos.Y += verticalChange;
            model.Position = pos;
        }


        public static void DeConnect(MainViewModel mainViewModel, DataStream dataStream)
        {
            DataStreamManager.SetConnectedState(dataStream, false);
            MainModelManager.RemoveConnection(dataStream, mainViewModel.Model);
            MainModelManager.RemoveFromIntegrationsIncludingChildren(dataStream, mainViewModel.Model);

            ViewRedraw(mainViewModel);
        }


        public static void DragDroppedTwoDangelingConnections(MainViewModel mainViewModel, DataStreamDefinition sourceDSD,
            DataStreamDefinition destinationDSD)
        {
            DataStreamManager.IsInSameCollection(sourceDSD, destinationDSD,
                onTrue: list => DataStreamManager.SwapDataStreamDefinitons(sourceDSD, destinationDSD, list),
                onFalse: () => CheckAreBothInputs(sourceDSD, destinationDSD,
                    isTrue:
                    () =>
                        MainModelManager.MakeIntegrationIncludingChildren(sourceDSD.Parent, destinationDSD.Parent,
                            mainViewModel.Model),
                    isFalse: () => MainModelManager.ConnectTwoDefintions(sourceDSD, destinationDSD, mainViewModel.Model)));

            ViewRedraw(mainViewModel);
        }


        private static void CheckAreBothInputs(DataStreamDefinition sourceDSD, DataStreamDefinition destinationDSD,
            Action isTrue, Action isFalse)
        {
            var sourceIsInput = sourceDSD.Parent.InputStreams.Contains(sourceDSD);
            var destinationIsInput = destinationDSD.Parent.InputStreams.Contains(destinationDSD);
            if (sourceIsInput && destinationIsInput)
                isTrue();
            else
                isFalse();
        }


        public static void ConnectDangelingConnectionAndFunctionUnit(MainViewModel mainViewModel, DataStreamDefinition defintionDSD,
            FunctionUnit destination)
        {

            MainModelManager.ConnectTwoDefintions(defintionDSD, destination.InputStreams.First(), mainViewModel.Model);

            ViewRedraw(mainViewModel);
        }


        public static void DeleteDatastreamDefiniton(MainViewModel mainViewModel, DataStreamDefinition dataStreamDefinition,
            FunctionUnit functionUnit)
        {
            functionUnit.InputStreams.RemoveAll(x => x == dataStreamDefinition);
            functionUnit.OutputStreams.RemoveAll(x => x == dataStreamDefinition);
            ViewRedraw(mainViewModel);
        }




        public static void GeneratedCodeToDesktop(bool sleepBeforeErrorPrint, MainModel mainModel)
        {
            SafeExecute(sleepBeforeErrorPrint, safeExecute: () =>
            {
                var gen = new MyGenerator();
                gen.GenerateCodeWithNamespace(mainModel, generatedCode =>
                {
                    Outputs.WriteToDesktopFile(generatedCode);
                    Outputs.ClearPrintToConsole(generatedCode);
                });
            });
        }


        public static void GenerateCodeToConsole(MainModel mainModel)
        {
            SafeExecute(sleepBeforeErrorPrint: true, safeExecute: () =>
            {
                var gen = new MyGenerator();
                gen.GenerateCodeWithNamespace(mainModel, Outputs.ClearPrintToConsole);
            });
        }


        private static void SafeExecute(bool sleepBeforeErrorPrint = true, Action safeExecute = null)
        {
            try
            {
                safeExecute?.Invoke();
            }

            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine(@"...");
                if (sleepBeforeErrorPrint)
                    Thread.Sleep(400);
                        // when same message pops up the user will see that the same error occured.           
                Console.WriteLine(ex.Message);
            }

        }


        public static void GenerateCodeToClipboard(MainModel mainModel)
        {
            SafeExecute(sleepBeforeErrorPrint: true, safeExecute: () =>
            {
                var gen = new MyGenerator();
                var methods = gen.GenerateAllMethods(mainModel);
                var datatypes = gen.GenerateDataTypes(mainModel);
                var text = gen.CompileToString(datatypes.Concat(methods).ToList());
                SetClipboardText(text);
                Outputs.ClearPrintToConsole(text);
            });

        }


        private static void SetClipboardText(string text)
        {
            Clipboard.SetText(text);
        }



        public static void DebugPrint(MainModel mainModel)
        {
            Console.Clear();
            DebugPrinter.PrintConnections(mainModel);
            DebugPrinter.PrintFunctionUnits(mainModel);
        }


        public static void AutoOutputTimerDispose()
        {
            _aTimer?.Dispose();
        }


        public static void AutoPrint(MainModel mainModel, Action<MainModel> printAction)
        {
            _aTimer?.Dispose();
            _aTimer = new Timer(state => { printAction(mainModel); }, null, 0, 200);
        }


        public static void StartAutoSave()
        {
            AutoSave.Start();
        }





        public static void ChangeConnectionDatanames(DataStream datastream, string newDatanames)
        {
            DataStreamManager.ChangeDatanames(datastream, newDatanames);
        }


        public static void SaveToFile(string fileName, MainModel model)
        {
            var saver = FileSaveLoad.GetFileSaver(fileName);
            saver?.Invoke(fileName, model);
        }


        public static void LoadFromFile(MainViewModel mainViewModel, string fileName)
        {
            var loader = FileSaveLoad.GetFileLoader(fileName);
            var loadedMainModel = loader?.Invoke(fileName);


            if (loadedMainModel != null)
                ViewRedraw(mainViewModel, loadedMainModel);
        }


        public static void MergeFromFile(MainViewModel mainViewModel, string fileName, MainModel mainModel)
        {
            var loader = FileSaveLoad.GetFileLoader(fileName);
            var loadedMainModel = loader?.Invoke(fileName);

            if (loadedMainModel != null)
            {
                mainModel.Connections.AddRange(loadedMainModel.Connections);
                mainModel.FunctionUnits.AddRange(loadedMainModel.FunctionUnits);
            }

            ViewRedraw(mainViewModel, loadedMainModel);
        }


        public static void Delete(MainViewModel mainViewModel, IEnumerable<FunctionUnit> functionUnits)
        {
            functionUnits.ForEach(sc => MainModelManager.DeleteFunctionUnit(sc, mainViewModel.Model));
            ViewRedraw(mainViewModel);
        }


        public static void Copy(List<FunctionUnit> functionUnits)
        {
            CopiedFunctionUnits.Clear();
            functionUnits.ForEach(CopiedFunctionUnits.Add);
        }


        public static void Paste(MainViewModel mainViewModel, Point positionClicked)
        {
            if (CopiedFunctionUnits.Count == 0)
                return;

            var copiedList = MainModelManager.Duplicate(CopiedFunctionUnits, mainViewModel.Model);
            MainModelManager.MoveFunctionUnitsToClickedPosition(positionClicked, copiedList);

            ViewRedraw(mainViewModel);
        }


        public static void StartPickIntegration(FunctionUnit functionUnit)
        {
            PickState = true;
            Mouse.OverrideCursor = Cursors.Cross;
            _startPickingAt = functionUnit;
        }


        private static List<FunctionUnit> _toMove = new List<FunctionUnit>();
        public static void MoveFunctionUnitIncludingChildrenAndIntegrated(FunctionUnit functionUnit, Vector dragDelta,
            MainModel mainModel)
        {
            _toMove.Clear();
            _toMove = MainModelManager.GetChildrenAndIntegrated(functionUnit, _toMove, mainModel);

            _toMove.ForEach(sc => sc.MovePosition(dragDelta));


        }


        public static FunctionUnit DuplicateFunctionUnitIncludingChildrenAndIntegrated(MainViewModel mainViewModel, FunctionUnit functionUnit)
        {
            _toMove.Clear();
            _toMove = MainModelManager.GetChildrenAndIntegrated(functionUnit, _toMove, mainViewModel.Model);

            var copiedFus = MainModelManager.Duplicate(_toMove, mainViewModel.Model);

            ViewRedraw(mainViewModel);

            return copiedFus.First(x => x.OriginGuid == functionUnit.ID).NewFunctionUnit;
        }




        public static void SetPickedIntegration(MainViewModel mainViewModel, FunctionUnit functionUnit)
        {
            MainModelManager.MakeIntegrationIncludingAllConnected(_startPickingAt, functionUnit, mainViewModel.Model);
            ViewRedraw(mainViewModel);
        }


        public static void RemoveFromIntegration(MainViewModel mainViewModel, FunctionUnit functionUnit)
        {
            MainModelManager.RemoveAllConnectedFromIntegration(functionUnit, mainViewModel.Model);
            ViewRedraw(mainViewModel);
        }


        public static object TabStopGetNext(object focusedModel, MainModel mainModel)
        {
            object @return = null;

            focusedModel.TryCast<FunctionUnit>(fu =>
            {
                // prefer connected outputs as next tabstop
                // if no connected take first output defintion if there are any
                fu.OutputStreams.GetFirstConnected(
                    foundConnected: connectedDsd =>
                        @return = MainModelManager.FindDataStream(connectedDsd, mainModel),
                    noConnected: () =>
                        @return = fu.OutputStreams.FirstOrDefault());
            });

            focusedModel.TryCast<DataStream>(stream =>
            {
                // if focus was inside datastream take its destination function unit as next tabstop
                if (stream.Destinations.Any())
                    @return = stream.Destinations.First().Parent;
            });

            focusedModel.TryCast<DataStreamDefinition>(dsd =>
            {
                // if focus was inside definition there are two case:
                // is input definition: next tabstop is the function unit of the definition
                // is output definition: next tabstop is the first input definition
                // of the beginning of the whole Flow Design graph.
                dsd.CheckIsInputOrOutput(
                    isInput: () => @return = dsd.Parent,
                    isOutput: () =>
                    {
                        dsd.Parent.OutputStreams.GetFirstConnected(
                            foundConnected: connectedInput =>
                                @return = MainModelManager.FindDataStream(connectedInput, mainModel),
                            noConnected: () => // loop tabstop focus when the end is reached
                                @return = MainModelManager.GetBeginningOfFlow(dsd.Parent, mainModel));
                    });
            });
            return @return;
        }

        public static object TabStopGetPrevious(object focusedModel, MainModel mainModel)
        {
            object @return = null;

            focusedModel.TryCast<FunctionUnit>(fu =>
            {
                fu.InputStreams.GetFirstConnected(
                    foundConnected: connectedDsd =>
                        @return = MainModelManager.FindDataStream(connectedDsd, mainModel),
                    noConnected: () =>
                        @return = fu.InputStreams.FirstOrDefault());
            });

            focusedModel.TryCast<DataStream>(stream =>
            {
                if (stream.Sources.Any())
                    @return = stream.Sources.First().Parent;
            });

            focusedModel.TryCast<DataStreamDefinition>(dsd =>
            {
                dsd.CheckIsInputOrOutput(
                    isOutput: () => @return = dsd.Parent,
                    isInput: () =>
                    {
                        dsd.Parent.InputStreams.GetFirstConnected(
                            foundConnected: connectedInput =>
                                @return = MainModelManager.FindDataStream(connectedInput, mainModel),
                            noConnected: () => // loop tabstop focus when the beginning is reached
                                @return = MainModelManager.GetEndOfFlow(dsd.Parent, mainModel));
                    });
            });

            return @return;
        }


        public static void Cut(MainViewModel mainViewModel, List<FunctionUnit> functionUnits)
        {

            CopiedFunctionUnits.Clear();
            functionUnits.ForEach(CopiedFunctionUnits.Add);

            functionUnits.ForEach(sc => MainModelManager.DeleteFunctionUnit(sc, mainViewModel.Model));
            ViewRedraw(mainViewModel);

        }


        public static object AppendNewFunctionUnit(MainViewModel mainViewModel, FunctionUnit currentFunctionUnit, double offsetX, DataStreamDefinition outputToConnect)
        {
            var newFunctionUnit = FunctionUnitManager.CreateNew();

            newFunctionUnit.Position = currentFunctionUnit.Position;
            newFunctionUnit.MoveX(offsetX);

            // default IO of new function unit: input of new function unit = output to connect to of current function unit
            newFunctionUnit.InputStreams.Add(DataStreamManager.NewDefinition(newFunctionUnit, outputToConnect.DataNames));
            newFunctionUnit.OutputStreams.Add(DataStreamManager.NewDefinition(newFunctionUnit, "()"));
            MainModelManager.ConnectTwoDefintions(outputToConnect, newFunctionUnit.InputStreams.First(), mainViewModel.Model);

            mainViewModel.Model.FunctionUnits.Add(newFunctionUnit);

            ViewRedraw(mainViewModel);

            return newFunctionUnit;
        }

        /// <summary>
        /// If the functionUnit is a integration it returns the first functionUnit of the integrated functionUnits.
        /// If the functionUnit is not a integration it will add a new functionUnit below it and add it to its integrated functionUnits.
        /// </summary>
        /// <param name="currentFunctionUnit">the functionUnit that is currently selected</param>
        /// <param name="mainModel">the mainmodel from the view</param>
        /// <returns>the model that was created or the first integrated functionUnit if it already had one</returns>
        public static FunctionUnit CreateNewOrGetFirstIntegrated(MainViewModel mainViewModel, FunctionUnit currentFunctionUnit)
        {
            FunctionUnit @return = null;

            currentFunctionUnit.IsIntegration(
                isIntegration: () => @return = MainModelManager.GetFirstOfIntegrated(currentFunctionUnit, mainViewModel.Model),
                isNotIntegration: () =>
                {
                    var newFunctionUnit = FunctionUnitManager.CreateNew();
                    newFunctionUnit.Position = currentFunctionUnit.Position;
                    newFunctionUnit.MoveY(100);

                    newFunctionUnit.InputStreams.Add(DataStreamManager.NewDefinition(newFunctionUnit, currentFunctionUnit.InputStreams.First()));
                    newFunctionUnit.OutputStreams.Add(DataStreamManager.NewDefinition(newFunctionUnit, "()"));

                    currentFunctionUnit.IsIntegrating.Add(newFunctionUnit);

                    mainViewModel.Model.FunctionUnits.Add(newFunctionUnit);

                    @return = newFunctionUnit;
                    ViewRedraw(mainViewModel);
                });

            return @return;
        }


        public static void DeleteDataTypeDefinition(MainViewModel mainViewModel, CustomDataType customDataType)
        {
            mainViewModel.Model.DataTypes.Remove(customDataType);
            ViewRedraw(mainViewModel);
        }


        public static CustomDataType AddDataTypeDefinition(MainViewModel mainViewModel)
        {
            var dataType = new CustomDataType
            {
                Name = "",
                SubDataTypes = null
            };

            mainViewModel.Model.DataTypes.Add(dataType);

            ViewRedraw(mainViewModel);
            return dataType;
        }


        public static void AddMissingDataTypes(MainViewModel mainViewModel)
        {
            var res = DataTypeManager.GetUndefinedTypenames(mainViewModel.Model).Where(x => !TypeConverter.IsSystemType(x));
            res.ForEach(name =>
            {
                var dataType = new CustomDataType
                {
                    Name = name,
                    SubDataTypes = null
                };
                mainViewModel.Model.DataTypes.Add(dataType);
            });
            ViewRedraw(mainViewModel);
        }


        public static void UpdateMissingDataTypesCounter(MainModel mainModel)
        {
            mainModel.Runtime.MissingDataTypes = DataTypeManager.GetUndefinedTypenames(mainModel).Where(x => !TypeConverter.IsSystemType(x)).ToList().Count;
        }



        public static void SwapDataStreamOrder(MainViewModel mainViewModel, DataStreamDefinition dsd1, DataStreamDefinition dsd2)
        {
            DataStreamManager.IsInSameCollection(dsd1, dsd2, list =>
            {
                DataStreamManager.SwapDataStreamDefinitons(dsd1, dsd2, list);
            });

            ViewRedraw(mainViewModel);

        }

        public static void LoadFromCSharp(MainViewModel mainViewModel, string fileName)
        {
            var mainmodel = CSharpToFlowDesign.FromFile(fileName);
            ViewRedraw(mainViewModel, mainmodel);
        }


        public static FunctionUnit GetFirstIntegrated(FunctionUnit functionUnit, MainModel mainModel)
        {
            return MainModelManager.GetFirstOfIntegrated(functionUnit, mainModel);
        }


        public static void Validate(MainViewModel mainViewModel)
        {
            List<ValidationError> errorsAndWarnings = new List<ValidationError>();

            try
            {
                FlowValidator.Validate(mainViewModel.Model, obj => errorsAndWarnings.Add(obj));
            }
            catch {}

            mainViewModel.ShowValidationResult(errorsAndWarnings);
        }
    }

}