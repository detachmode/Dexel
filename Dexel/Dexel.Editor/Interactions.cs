using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Dexel.Editor.CustomControls;
using Dexel.Editor.DebuggingHelper;
using Dexel.Editor.ViewModels;
using Dexel.Library;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Roslyn;

namespace Dexel.Editor
{

    public static class Interactions
    {
        public enum DragMode
        {
            Single,
            Multiple
        }


        private static Timer aTimer;


        private static readonly List<FunctionUnit> _copiedFunctionUnits = new List<FunctionUnit>();
        public static bool PickState;


        private static FunctionUnit StartPickingAt;


        public static FunctionUnit AddNewFunctionUnit(Point pos, MainModel mainModel)
        {
            var softwareCell = FunctionUnitManager.CreateNew();
            pos.X -= 100;
            pos.Y -= 20;
            softwareCell.Position = new Point(pos.X, pos.Y);
            softwareCell.InputStreams.Add(DataStreamManager.NewDefinition(softwareCell, "()"));
            softwareCell.OutputStreams.Add(DataStreamManager.NewDefinition(softwareCell, "()"));

            mainModel.FunctionUnits.Add(softwareCell);
            ViewRedraw();
            return softwareCell;
        }


        public static void ViewRedraw()
        {
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
            //MainModelManager.RemoveConnection(dataStream, mainModel);
            DeConnect(dataStream, mainModel);
            MainModelManager.ConnectTwoFunctionUnits(dataStream.Sources.First().Parent, newdestination, dataStream.DataNames, "",
                mainModel);

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




        public static void ConsolePrintGeneratedCode(MainModel mainModel)
        {
            try
            {
                var gen = new MyGenerator();
                Console.Clear();
                gen.GenerateCodeAndPrint(mainModel);
            }
            catch (Exception ex)
            {
                PrintError(ex);
            }
        }


        private static void PrintError(Exception exception)
        {
            Console.WriteLine(exception.Message);
        }


        public static void GenerateCodeToClipboard(MainModel mainModel)
        {
            try
            {
                var gen = new MyGenerator();
                var methods = gen.GenerateAllMethods(mainModel);
                var datatypes = gen.GenerateDataTypes(mainModel);
                var text = gen.CompileToString(datatypes.Concat(methods).ToList());
                Clipboard.SetText(text);
            }
            catch (Exception ex)
            {
                PrintError(ex);
            }
        }


        public static void DebugPrint(MainModel mainModel)
        {
            Console.Clear();
            DebugPrinter.PrintConnections(mainModel);
            DebugPrinter.PrintFunctionUnits(mainModel);
        }


        public static void AutoOutputTimerDispose()
        {
            aTimer?.Dispose();
        }


        public static void AutoPrint(MainModel mainModel, Action<MainModel> printAction)
        {
            aTimer?.Dispose();
            aTimer = new Timer(state => { printAction(mainModel); }, null, 0, 200);
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
            {
                ViewRedraw(loadedMainModel);
            }
        }


        public static void MergeFromFile(string fileName, MainModel mainModel)
        {
            var loader = FileSaveLoad.GetFileLoader(fileName);
            var loadedMainModel = loader?.Invoke(fileName);

            mainModel.Connections.AddRange(loadedMainModel.Connections);
            mainModel.FunctionUnits.AddRange(loadedMainModel.FunctionUnits);

            ViewRedraw();
        }


        public static void Delete(IEnumerable<FunctionUnit> softwareCells, MainModel mainModel)
        {
            softwareCells.ForEach(sc => MainModelManager.DeleteFunctionUnit(sc, mainModel));
            ViewRedraw();
        }


        public static void Copy(List<FunctionUnit> softwareCells, MainModel mainModel)
        {
            _copiedFunctionUnits.Clear();
            softwareCells.ForEach(_copiedFunctionUnits.Add);
        }


        public static void Paste(Point positionClicked, MainModel mainModel)
        {
            if (_copiedFunctionUnits.Count == 0)
                return;

            var copiedList = MainModelManager.Duplicate(_copiedFunctionUnits, mainModel);
            MainModelManager.MoveFunctionUnitsToClickedPosition(positionClicked, copiedList);

            ViewRedraw();
        }


        public static void StartPickIntegration(FunctionUnit functionUnit)
        {
            PickState = true;
            Mouse.OverrideCursor = Cursors.Cross;
            StartPickingAt = functionUnit;
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

            var copiedcells = MainModelManager.Duplicate(_toMove, mainModel);

            ViewRedraw();

            return copiedcells.First(x => x.OriginGuid == functionUnit.ID).NewFunctionUnit;
        }




        public static void SetPickedIntegration(FunctionUnit functionUnit, MainModel mainModel)
        {
            MainModelManager.MakeIntegrationIncludingAllConnected(StartPickingAt, functionUnit, mainModel);
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
                if (fu.OutputStreams.Any(dsd => dsd.Connected))
                {
                    var connectedDsd = fu.OutputStreams.First(dsd => dsd.Connected);
                    @return = mainModel.Connections.First(c => c.Sources.Contains(connectedDsd));
                } 
                else if (fu.OutputStreams.Any()) 
                {
                    @return = fu.OutputStreams.First();
                }
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
                dsd.Check(
                    isInput: () => @return = dsd.Parent,
                    isOutput: () =>
                    {
                        dsd.Parent.OutputStreams.GetFirstConnected(
                            foundConnected: connectedInput =>
                            {
                                MainModelManager.TraverseChildrenBackwards(connectedInput.Parent, fu =>
                                {
                                    if (fu.OutputStreams.Any())
                                        @return = fu.OutputStreams.First();
                                    else
                                        @return = fu;

                                }, mainModel);
                            },
                            noConnected: () => @return = dsd.Parent.OutputStreams.First());
                    });
            });
            return @return;
        }

        public static object TabStopGetPrevious(object focusedModel, MainModel mainModel)
        {
            object @return = null;
            focusedModel.TryCast<FunctionUnit>(fu =>
            {
                if (fu.InputStreams.Any(dsd => dsd.Connected))
                {
                    var connectedDsd = fu.InputStreams.First(dsd => dsd.Connected);
                    @return = mainModel.Connections.First(c => c.Destinations.Contains(connectedDsd));
                }
                else if (fu.InputStreams.Any())
                {
                    @return = fu.InputStreams.First();
                }
            });

            focusedModel.TryCast<DataStream>(stream =>
            {
                if (stream.Sources.Any())
                    @return = stream.Sources.First().Parent;
            });

            focusedModel.TryCast<DataStreamDefinition>(dsd =>
            {
                var softwareCell = dsd.Parent;

                if (dsd.IsOutput())
                    @return = softwareCell;

                if (dsd.IsInput())
                    if (softwareCell.OutputStreams.Any(x => x.Connected))
                        MainModelManager.TraverseChildren(softwareCell, fu =>
                        {
                            if (fu.OutputStreams.Any())
                                @return = fu.OutputStreams.First();
                            else
                                @return = fu;

                        }, mainModel);
                    else if (softwareCell.OutputStreams.Any())
                        @return = softwareCell.OutputStreams.First();

            });

            return @return;
        }


        public static void Cut(List<FunctionUnit> softwareCells, MainModel mainModel)
        {

            _copiedFunctionUnits.Clear();
            softwareCells.ForEach(_copiedFunctionUnits.Add);

            softwareCells.ForEach(sc => MainModelManager.DeleteFunctionUnit(sc, mainModel));
            ViewRedraw();

        }


        public static object AppendNewFunctionUnit(FunctionUnit focusedcell, double width, DataStreamDefinition dataStreamDefinition, MainModel mainModel)
        {
            var softwareCell = FunctionUnitManager.CreateNew();
            softwareCell.Position = focusedcell.Position;
            softwareCell.MoveX(width);

            softwareCell.InputStreams.Add(DataStreamManager.NewDefinition(softwareCell, dataStreamDefinition));
            softwareCell.OutputStreams.Add(DataStreamManager.NewDefinition(softwareCell, "()"));

            MainModelManager.ConnectTwoDefintions(dataStreamDefinition, softwareCell.InputStreams.First(), mainModel);

            mainModel.FunctionUnits.Add(softwareCell);
            ViewRedraw();

            return softwareCell;
        }

        /// <summary>
        /// If the softwarecell is a integration it returns the first softwarecell of the integrated softwarecells.
        /// If the softwarecell is not a integration it will add a new softwarecell below it and add it to its integrated softwarecells.
        /// </summary>
        /// <param name="focusedcell">the softwarecell that is currently selected</param>
        /// <param name="mainModel">the mainmodel from the view</param>
        /// <returns>the model that was created or the first integrated softwarecell if it already had one</returns>
        public static object NewOrFirstIntegrated(FunctionUnit focusedcell, MainModel mainModel)
        {
            object @return = null;

            focusedcell.IsIntegration(
                isIntegration: () => @return = focusedcell.Integration.First(),
                isNotIntegration: () =>
                {
                    var softwareCell = FunctionUnitManager.CreateNew();
                    softwareCell.Position = focusedcell.Position;
                    softwareCell.MoveY(100);

                    softwareCell.InputStreams.Add(DataStreamManager.NewDefinition(softwareCell, focusedcell.InputStreams.First()));
                    softwareCell.OutputStreams.Add(DataStreamManager.NewDefinition(softwareCell, "()"));

                    focusedcell.Integration.AddUnique(softwareCell);
                    mainModel.FunctionUnits.Add(softwareCell);

                    @return = softwareCell;
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
            var res = DataTypeManager.GetUndefinedTypenames(mainModel).Where(x => !DataTypeParser.IsSystemType(x));
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
            MainViewModel.Instance().MissingDataTypes = DataTypeManager.GetUndefinedTypenames(mainModel).Where(x => !DataTypeParser.IsSystemType(x)).ToList().Count;
        }



        public static void SwapDataStreamOrder(DataStreamDefinition dsd1, DataStreamDefinition dsd2, MainModel mainModel)
        {
            DataStreamManager.IsInSameCollection(dsd1, dsd2, list =>
            {
                DataStreamManager.SwapDataStreamDefinitons(dsd1, dsd2, list);
            });

            ViewRedraw();

        }
    }

}