using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
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
            softwareCell.InputStreams.Add(DataStreamManager.NewDefinition(softwareCell, "input"));
            softwareCell.OutputStreams.Add(DataStreamManager.NewDefinition(softwareCell, "output"));

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
    }

}