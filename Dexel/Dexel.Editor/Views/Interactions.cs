using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Dexel.Editor.Common;
using Dexel.Editor.FileIO;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views.CustomControls;
using Dexel.Library;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Roslyn;
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


        public static void ChangeToPrintTheme()
        {
            MainViewModel.Instance().ChangeTheme("Views/Themes/Print.xaml", @"Views/Themes/FlowDesignColorPrint.xshd");
            App.SetConfig("Theme", "Print");
            
        }

        public static void ChangeToDarkTheme()
        {
            MainViewModel.Instance().ChangeTheme("Views/Themes/DarkColorfull.xaml", @"Views/Themes/FlowDesignColorDark.xshd");
            App.SetConfig("Theme", "Dark");
        }


        public static FunctionUnit AddNewFunctionUnit(Point pos, MainModel mainModel)
        {
            var functionUnit = FunctionUnitManager.CreateNew();
            pos.X -= 100;
            pos.Y -= 20;
            functionUnit.Position = new Point(pos.X, pos.Y);
            functionUnit.InputStreams.Add(DataStreamManager.NewDefinition(functionUnit, "()"));
            functionUnit.OutputStreams.Add(DataStreamManager.NewDefinition(functionUnit, "()"));

            mainModel.FunctionUnits.Add(functionUnit);
            ViewRedraw();
            return functionUnit;
        }


        public static void ViewRedraw()
        {
            Validate(MainViewModel.Instance().Model);
            MainViewModel.Instance().Reload();
        }


        public static void ViewRedraw(MainModel mainModel)
        {
            MainViewModel.Instance().Model = mainModel;
            MainViewModel.Instance().Reload();
        }


        public static void ChangeConnectionDestination(DataStream dataStream, FunctionUnit newdestination,
            MainModel mainModel)
        {

            DeConnect(dataStream, mainModel);
            MainModelManager.ConnectTwoDefintions(dataStream.Sources.First(), newdestination.InputStreams.First(), mainModel);

            ViewRedraw();
        }


        public static void AddNewOutput(FunctionUnit functionUnit, string datanames)
        {
            MainModelManager.AddNewOutput(functionUnit, datanames);
            ViewRedraw();
        }


        public static void AddNewInput(FunctionUnit functionUnit, string datanames)
        {
            MainModelManager.AddNewInput(functionUnit, datanames);
            ViewRedraw();
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


        public static void DeConnect(DataStream dataStream, MainModel mainModel)
        {
            DataStreamManager.SetConnectedState(dataStream, false);
            MainModelManager.RemoveConnection(dataStream, mainModel);
            MainModelManager.RemoveFromIntegrationsIncludingChildren(dataStream, mainModel);

            ViewRedraw();
        }


        public static void DragDroppedTwoDangelingConnections(DataStreamDefinition sourceDSD,
            DataStreamDefinition destinationDSD, MainModel mainModel)
        {
            DataStreamManager.IsInSameCollection(sourceDSD, destinationDSD,
                onTrue: list => DataStreamManager.SwapDataStreamDefinitons(sourceDSD, destinationDSD, list),
                onFalse: () => CheckAreBothInputs(sourceDSD, destinationDSD,
                    isTrue: () => MainModelManager.MakeIntegrationIncludingChildren(sourceDSD.Parent, destinationDSD.Parent, mainModel),
                    isFalse: () => MainModelManager.ConnectTwoDefintions(sourceDSD, destinationDSD, mainModel)));

            ViewRedraw();
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


        public static void ConnectDangelingConnectionAndFunctionUnit(DataStreamDefinition defintionDSD,
            FunctionUnit destination, MainModel mainModel)
        {
            var inputDefinition = FunctionUnitManager.NewInputDef(destination, "", null);
            MainModelManager.ConnectTwoDefintions(defintionDSD, inputDefinition, mainModel);

            ViewRedraw();
        }


        public static void DeleteDatastreamDefiniton(DataStreamDefinition dataStreamDefinition,
            FunctionUnit functionUnit)
        {
            functionUnit.InputStreams.RemoveAll(x => x == dataStreamDefinition);
            functionUnit.OutputStreams.RemoveAll(x => x == dataStreamDefinition);
            ViewRedraw();
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
            SafeExecute(sleepBeforeErrorPrint:true, safeExecute: () =>
            {
                var gen = new MyGenerator();
                gen.GenerateCodeWithNamespace(mainModel, Outputs.ClearPrintToConsole);
            });
        }


        private static void SafeExecute(bool sleepBeforeErrorPrint = true , Action safeExecute = null)
        {
            try
            {
                safeExecute?.Invoke();
            }

            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine(@"...");
                if (sleepBeforeErrorPrint) Thread.Sleep(400);// when same message pops up the user will see that the same error occured.           
                Console.WriteLine(ex.Message);
            }

        }

        public static void GenerateCodeToClipboard(MainModel mainModel)
        {
            SafeExecute(sleepBeforeErrorPrint:true, safeExecute:() =>
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


        public static void ChangeConnectionDatanames(DataStream datastream, string newDatanames)
        {
            DataStreamManager.ChangeDatanames(datastream, newDatanames);
        }


        public static void SaveToFile(string fileName, MainModel model)
        {
            var saver = FileSaveLoad.GetFileSaver(fileName);
            saver?.Invoke(fileName, model);
        }


        public static void LoadFromFile(string fileName, MainModel model)
        {
            var loader = FileSaveLoad.GetFileLoader(fileName);
            var loadedMainModel = loader?.Invoke(fileName);


            if (loadedMainModel != null)
                ViewRedraw(loadedMainModel);
        }


        public static void MergeFromFile(string fileName, MainModel mainModel)
        {
            var loader = FileSaveLoad.GetFileLoader(fileName);
            var loadedMainModel = loader?.Invoke(fileName);

            if (loadedMainModel != null)
            {
                mainModel.Connections.AddRange(loadedMainModel.Connections);
                mainModel.FunctionUnits.AddRange(loadedMainModel.FunctionUnits);
            }

            ViewRedraw();
        }


        public static void Delete(IEnumerable<FunctionUnit> functionUnits, MainModel mainModel)
        {
            functionUnits.ForEach(sc => MainModelManager.DeleteFunctionUnit(sc, mainModel));
            ViewRedraw();
        }


        public static void Copy(List<FunctionUnit> functionUnits, MainModel mainModel)
        {
            CopiedFunctionUnits.Clear();
            functionUnits.ForEach(CopiedFunctionUnits.Add);
        }


        public static void Paste(Point positionClicked, MainModel mainModel)
        {
            if (CopiedFunctionUnits.Count == 0)
                return;

            var copiedList = MainModelManager.Duplicate(CopiedFunctionUnits, mainModel);
            MainModelManager.MoveFunctionUnitsToClickedPosition(positionClicked, copiedList);

            ViewRedraw();
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


        public static FunctionUnit DuplicateFunctionUnitIncludingChildrenAndIntegrated(FunctionUnit functionUnit, MainModel mainModel)
        {
            _toMove.Clear();
            _toMove = MainModelManager.GetChildrenAndIntegrated(functionUnit, _toMove, mainModel);

            var copiedFus = MainModelManager.Duplicate(_toMove, mainModel);

            ViewRedraw();

            return copiedFus.First(x => x.OriginGuid == functionUnit.ID).NewFunctionUnit;
        }




        public static void SetPickedIntegration(FunctionUnit functionUnit, MainModel mainModel)
        {
            MainModelManager.MakeIntegrationIncludingAllConnected(_startPickingAt, functionUnit, mainModel);
            ViewRedraw();
        }


        public static void RemoveFromIntegration(FunctionUnit functionUnit, MainModel mainModel)
        {
            MainModelManager.RemoveAllConnectedFromIntegration(functionUnit, mainModel);
            ViewRedraw();
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


        public static void Cut(List<FunctionUnit> functionUnits, MainModel mainModel)
        {

            CopiedFunctionUnits.Clear();
            functionUnits.ForEach(CopiedFunctionUnits.Add);

            functionUnits.ForEach(sc => MainModelManager.DeleteFunctionUnit(sc, mainModel));
            ViewRedraw();

        }


        public static object AppendNewFunctionUnit(FunctionUnit currentFunctionUnit, double offsetX, DataStreamDefinition outputToConnect, MainModel mainModel)
        {
            var newFunctionUnit = FunctionUnitManager.CreateNew();

            newFunctionUnit.Position = currentFunctionUnit.Position;
            newFunctionUnit.MoveX(offsetX);

            // default IO of new function unit: input of new function unit = output to connect to of current function unit
            newFunctionUnit.InputStreams.Add(DataStreamManager.NewDefinition(newFunctionUnit, outputToConnect.DataNames));
            newFunctionUnit.OutputStreams.Add(DataStreamManager.NewDefinition(newFunctionUnit, "()"));
            MainModelManager.ConnectTwoDefintions(outputToConnect, newFunctionUnit.InputStreams.First(), mainModel);

            mainModel.FunctionUnits.Add(newFunctionUnit);

            ViewRedraw();

            return newFunctionUnit;
        }

        /// <summary>
        /// If the functionUnit is a integration it returns the first functionUnit of the integrated functionUnits.
        /// If the functionUnit is not a integration it will add a new functionUnit below it and add it to its integrated functionUnits.
        /// </summary>
        /// <param name="currentFunctionUnit">the functionUnit that is currently selected</param>
        /// <param name="mainModel">the mainmodel from the view</param>
        /// <returns>the model that was created or the first integrated functionUnit if it already had one</returns>
        public static FunctionUnit CreateNewOrGetFirstIntegrated(FunctionUnit currentFunctionUnit, MainModel mainModel)
        {
            FunctionUnit @return = null;

            currentFunctionUnit.IsIntegration(
                isIntegration: () => @return = MainModelManager.GetFirstOfIntegrated(currentFunctionUnit, mainModel),
                isNotIntegration: () =>
                {
                    var newFunctionUnit = FunctionUnitManager.CreateNew();
                    newFunctionUnit.Position = currentFunctionUnit.Position;
                    newFunctionUnit.MoveY(100);

                    newFunctionUnit.InputStreams.Add(DataStreamManager.NewDefinition(newFunctionUnit, currentFunctionUnit.InputStreams.First()));
                    newFunctionUnit.OutputStreams.Add(DataStreamManager.NewDefinition(newFunctionUnit, "()"));

                    currentFunctionUnit.IsIntegrating.Add(newFunctionUnit);

                    mainModel.FunctionUnits.Add(newFunctionUnit);

                    @return = newFunctionUnit;
                    ViewRedraw();
                });

            return @return;
        }


        public static void DeleteDataTypeDefinition(CustomDataType customDataType, MainModel mainModel)
        {
            mainModel.DataTypes.Remove(customDataType);
            ViewRedraw();
        }


        public static CustomDataType AddDataTypeDefinition(MainModel mainModel)
        {
            var dataType = new CustomDataType
            {
                Name = "",
                SubDataTypes = null
            };

            mainModel.DataTypes.Add(dataType);

            ViewRedraw();
            return dataType;
        }


        public static void AddMissingDataTypes(MainModel mainModel)
        {
            var res = DataTypeManager.GetUndefinedTypenames(mainModel).Where(x => !TypeConverter.IsSystemType(x));
            res.ForEach(name =>
            {
                var dataType = new CustomDataType
                {
                    Name = name,
                    SubDataTypes = null
                };
                mainModel.DataTypes.Add(dataType);
            });
            ViewRedraw();
        }


        public static void UpdateMissingDataTypesCounter(MainModel mainModel)
        {
            MainViewModel.Instance().MissingDataTypes = DataTypeManager.GetUndefinedTypenames(mainModel).Where(x => !TypeConverter.IsSystemType(x)).ToList().Count;
        }



        public static void SwapDataStreamOrder(DataStreamDefinition dsd1, DataStreamDefinition dsd2, MainModel mainModel)
        {
            DataStreamManager.IsInSameCollection(dsd1, dsd2, list =>
            {
                DataStreamManager.SwapDataStreamDefinitons(dsd1, dsd2, list);
            });

            ViewRedraw();

        }


        public static FunctionUnit GetFirstIntegrated(FunctionUnit functionUnit, MainModel mainModel)
        {
            return MainModelManager.GetFirstOfIntegrated(functionUnit, mainModel);
        }


        public static void Validate(MainModel mainModel)
        {
            List<ValidationError> errorsAndWarnings = new List<ValidationError>();

            try
            {
                FlowValidator.Validate(mainModel, obj => errorsAndWarnings.Add(obj));
            }
            catch {}

            MainViewModel.Instance().ShowValidationResult(errorsAndWarnings);
        }
    }

}