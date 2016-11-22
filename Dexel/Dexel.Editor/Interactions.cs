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


        private static readonly List<SoftwareCell> _copiedCells = new List<SoftwareCell>();
        public static bool PickState;


        private static SoftwareCell StartPickingAt;


        public static SoftwareCell AddNewIOCell(Point pos, MainModel mainModel)
        {
            var softwareCell = SoftwareCellsManager.CreateNew();
            pos.X -= 100;
            pos.Y -= 20;
            softwareCell.Position = new Point(pos.X, pos.Y);
            softwareCell.InputStreams.Add(DataStreamManager.NewDefinition(softwareCell, "()"));
            softwareCell.OutputStreams.Add(DataStreamManager.NewDefinition(softwareCell, "()"));

            mainModel.SoftwareCells.Add(softwareCell);
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


        public static void ChangeConnectionDestination(DataStream dataStream, SoftwareCell newdestination,
            MainModel mainModel)
        {
            //MainModelManager.RemoveConnection(dataStream, mainModel);
            DeConnect(dataStream, mainModel);
            MainModelManager.ConnectTwoCells(dataStream.Sources.First().Parent, newdestination, dataStream.DataNames, "",
                mainModel,
                dataStream.ActionName);

            ViewRedraw();
        }


        public static void AddNewOutput(SoftwareCell softwareCell, string datanames)
        {
            MainModelManager.AddNewOutput(softwareCell, datanames);
            ViewRedraw();
        }


        public static void AddNewInput(SoftwareCell softwareCell, string datanames)
        {
            MainModelManager.AddNewInput(softwareCell, datanames);
            ViewRedraw();
        }


        public static void MoveSoftwareCell(SoftwareCell model, double horizontalChange, double verticalChange)
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
            CheckAreBothInputs(sourceDSD, destinationDSD,
                () =>
                    MainModelManager.MakeIntegrationIncludingChildren(sourceDSD.Parent, destinationDSD.Parent, mainModel),
                () => MainModelManager.ConnectTwoDefintions(sourceDSD, destinationDSD, mainModel));

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


        public static void ConnectDangelingConnectionAndSoftwareCell(DataStreamDefinition defintionDSD,
            SoftwareCell destination, MainModel mainModel)
        {
            var inputDefinition = SoftwareCellsManager.NewInputDef(destination, "", null);
            MainModelManager.ConnectTwoDefintions(defintionDSD, inputDefinition, mainModel);

            ViewRedraw();
        }


        public static void DeleteDatastreamDefiniton(DataStreamDefinition dataStreamDefinition,
            SoftwareCell softwareCell)
        {
            softwareCell.InputStreams.RemoveAll(x => x == dataStreamDefinition);
            softwareCell.OutputStreams.RemoveAll(x => x == dataStreamDefinition);
            ViewRedraw();
        }


        public static void ConsolePrintGeneratedCode(MainModel mainModel)
        {
            var gen = new MyGenerator();
            Console.Clear();
            gen.GenerateCodeAndPrint(mainModel);
        }


        public static void DebugPrint(MainModel mainModel)
        {
            Console.Clear();
            DebugPrinter.PrintConnections(mainModel);
            DebugPrinter.PrintSoftwareCells(mainModel);
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
            mainModel.SoftwareCells.AddRange(loadedMainModel.SoftwareCells);

            ViewRedraw();
        }


        public static void Delete(IEnumerable<SoftwareCell> softwareCells, MainModel mainModel)
        {
            softwareCells.ForEach(sc => MainModelManager.DeleteCell(sc, mainModel));
            ViewRedraw();
        }


        public static void Copy(List<SoftwareCell> softwareCells, MainModel mainModel)
        {
            _copiedCells.Clear();
            softwareCells.ForEach(_copiedCells.Add);
        }


        public static void Paste(Point positionClicked, MainModel mainModel)
        {
            if (_copiedCells.Count == 0)
                return;

            var copiedList = MainModelManager.Duplicate(_copiedCells, mainModel);
            MainModelManager.MoveCellsToClickedPosition(positionClicked, copiedList);

            ViewRedraw();
        }


        public static void StartPickIntegration(SoftwareCell softwareCell)
        {
            PickState = true;
            Mouse.OverrideCursor = Cursors.Cross;
            StartPickingAt = softwareCell;
        }


        private static List<SoftwareCell> _toMove = new List<SoftwareCell>();
        public static void MoveIOCellIncludingChildrenAndIntegrated(SoftwareCell softwareCell, Vector dragDelta,
            MainModel mainModel)
        {
            _toMove.Clear();
            _toMove = MainModelManager.GetChildrenAndIntegrated(softwareCell, _toMove, mainModel);

            _toMove.ForEach(sc => sc.MovePosition(dragDelta));


        }

       
        public static SoftwareCell DuplicateIOCellIncludingChildrenAndIntegrated(SoftwareCell softwareCell,MainModel mainModel)
        {
            _toMove.Clear();
            _toMove = MainModelManager.GetChildrenAndIntegrated(softwareCell, _toMove, mainModel);

            var copiedcells =  MainModelManager.Duplicate(_toMove, mainModel);

            ViewRedraw();

            return copiedcells.First(x => x.OriginGuid == softwareCell.ID).NewCell;
        }




        public static void SetPickedIntegration(SoftwareCell softwareCell, MainModel mainModel)
        {
            MainModelManager.MakeIntegrationIncludingAllConnected(StartPickingAt, softwareCell, mainModel);
            ViewRedraw();
        }


        public static void RemoveFromIntegration(SoftwareCell softwareCell, MainModel mainModel)
        {
            MainModelManager.RemoveAllConnectedFromIntegration(softwareCell, mainModel);
            ViewRedraw();
        }


        public static object TabStopGetNext(object focusedModel, MainModel mainModel)
        {
            object result = null;
            focusedModel.TryCast<SoftwareCell>(cell =>
            {
                if (cell.OutputStreams.Any(dsd => dsd.Connected))
                {
                    var connectedDsd = cell.OutputStreams.First(dsd => dsd.Connected);
                    result = mainModel.Connections.First(c => c.Sources.Contains(connectedDsd));
                }
                else if (cell.OutputStreams.Any())
                {
                    result = cell.OutputStreams.First();
                }
            });

            focusedModel.TryCast<DataStream>(stream =>
            {
                if (stream.Destinations.Any())
                    result = stream.Destinations.First().Parent;
            });

            focusedModel.TryCast<DataStreamDefinition>(dsd =>
            {
                var softwareCell = dsd.Parent;

                if (dsd.IsInput())
                    result = softwareCell;

                if (dsd.IsOutput())
                    if (softwareCell.InputStreams.Any(x => x.Connected))
                        MainModelManager.TraverseChildrenBackwards(softwareCell, cell =>
                        {
                            if (cell.InputStreams.Any())
                                result = cell.InputStreams.First();
                            else
                                result = cell;

                        }, mainModel);


                    else if (softwareCell.InputStreams.Any())
                        result = softwareCell.InputStreams.First();

            });

            return result;
        }

        public static object TabStopGetPrevious(object focusedModel, MainModel mainModel)
        {
            object result = null;
            focusedModel.TryCast<SoftwareCell>(cell =>
            {
                if (cell.InputStreams.Any(dsd => dsd.Connected))
                {
                    var connectedDsd = cell.InputStreams.First(dsd => dsd.Connected);
                    result = mainModel.Connections.First(c => c.Destinations.Contains(connectedDsd));
                }
                else if (cell.InputStreams.Any())
                {
                    result = cell.InputStreams.First();
                }
            });

            focusedModel.TryCast<DataStream>(stream =>
            {
                if (stream.Sources.Any())
                    result = stream.Sources.First().Parent;
            });

            focusedModel.TryCast<DataStreamDefinition>(dsd =>
            {
                var softwareCell = dsd.Parent;

                if (dsd.IsOutput())
                    result = softwareCell;

                if (dsd.IsInput())
                    if (softwareCell.OutputStreams.Any(x => x.Connected))
                        MainModelManager.TraverseChildren(softwareCell, cell =>
                        {
                            if (cell.OutputStreams.Any())
                                result = cell.OutputStreams.First();
                            else
                                result = cell;

                        }, mainModel);
                    else if (softwareCell.OutputStreams.Any())
                        result = softwareCell.OutputStreams.First();

            });

            return result;
        }


        public static void Cut(List<SoftwareCell> softwareCells, MainModel mainModel)
        {
            
            _copiedCells.Clear();
            softwareCells.ForEach(_copiedCells.Add);

            softwareCells.ForEach(sc => MainModelManager.DeleteCell(sc, mainModel));
            ViewRedraw();

    }


        public static object AppendNewCell(SoftwareCell focusedcell, double width, DataStreamDefinition dataStreamDefinition, MainModel mainModel)
        {
            var softwareCell = SoftwareCellsManager.CreateNew();

            var  pos = focusedcell.Position;
            pos.X += width;
            softwareCell.Position = pos;

            softwareCell.InputStreams.Add(DataStreamManager.NewDefinition(softwareCell, dataStreamDefinition));
            softwareCell.OutputStreams.Add(DataStreamManager.NewDefinition(softwareCell, "()"));

            MainModelManager.ConnectTwoDefintions(dataStreamDefinition, softwareCell.InputStreams.First(), mainModel);

            mainModel.SoftwareCells.Add(softwareCell);
            ViewRedraw();
            return softwareCell;
        }


        public static object NewOrFirstIntegrated(SoftwareCell focusedcell, MainModel mainModel)
        {
            if (focusedcell.Integration.Any())
                return focusedcell.Integration.First();

            var softwareCell = SoftwareCellsManager.CreateNew();
            var pos = focusedcell.Position;
            pos.Y += 100;
            softwareCell.Position = pos;

            softwareCell.InputStreams.Add(DataStreamManager.NewDefinition(softwareCell, focusedcell.InputStreams.First()));
            softwareCell.OutputStreams.Add(DataStreamManager.NewDefinition(softwareCell, "()"));

            focusedcell.Integration.AddUnique(softwareCell);
            mainModel.SoftwareCells.Add(softwareCell);

            ViewRedraw();
            return softwareCell;
            
        }
    }

}