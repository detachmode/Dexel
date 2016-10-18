using System;
using System.Linq;
using System.Threading;
using System.Windows;
using Dexel.Contracts.Model;
using Dexel.Editor.DebuggingHelper;
using Dexel.Editor.ViewModels;
using Dexel.Model;
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



        private static Timer _aTimer;

        private static DataStreamManager _dataStreamManager = new DataStreamManager();
        private static SoftwareCellsManager _softwareCellsManager = new SoftwareCellsManager();
        private static MainModelManager _mainModelManager = new MainModelManager(_softwareCellsManager, _dataStreamManager);






        public static void AddNewIOCell(Point pos, IMainModel mainModel)
        {
            var softwareCell = _softwareCellsManager.CreateNew();
            pos.X -= 100;
            pos.Y -= 20;
            softwareCell.Position = new Point(pos.X, pos.Y);
            softwareCell.InputStreams.Add(_dataStreamManager.CreateNewDefinition(softwareCell, "input"));
            softwareCell.OutputStreams.Add(_dataStreamManager.CreateNewDefinition(softwareCell, "output"));

            mainModel.SoftwareCells.Add(softwareCell);
            ViewRedraw();
        }


        public static void ViewRedraw()
        {
            MainViewModel.Instance().Reload();

        }


        public static void ChangeConnectionDestination(IDataStream dataStream, ISoftwareCell newdestination,
            IMainModel mainModel)
        {
            //MainModelManager.RemoveConnection(dataStream, mainModel);
            DeConnect(dataStream, mainModel);
            _mainModelManager.Connect(dataStream.Sources.First().Parent, newdestination, dataStream.DataNames, mainModel,
                dataStream.ActionName);

            ViewRedraw();
        }


        public static void AddNewInput(Guid softwareCellID, string datanames, IMainModel mainModel,
            string actionName = "")
        {
            _mainModelManager.AddNewInput(softwareCellID, datanames, mainModel, actionName);
        }


        public static void AddNewOutput(ISoftwareCell softwareCell, string datanames)
        {
            _mainModelManager.AddNewOutput(softwareCell, datanames);
            ViewRedraw();
        }

        internal static void AddNewInput(ISoftwareCell softwareCell, string datanames)
        {
            _mainModelManager.AddNewInput(softwareCell, datanames);
            ViewRedraw();
        }

        public static void MoveSoftwareCell(ISoftwareCell model, double horizontalChange, double verticalChange)
        {
            var pos = model.Position;
            pos.X += horizontalChange;
            pos.Y += verticalChange;
            model.Position = pos;
        }


        public static void DeConnect(IDataStream dataStream, IMainModel mainModel)
        {
            dataStream.Sources.ForEach(streamDefinition =>
                _dataStreamManager.DeConnect(streamDefinition.Parent.OutputStreams, dataStream));
            dataStream.Destinations.ForEach(streamDefinition =>
                _dataStreamManager.DeConnect(streamDefinition.Parent.InputStreams, dataStream));

            _mainModelManager.RemoveConnection(dataStream, mainModel);
            ViewRedraw();
        }


        public static void ConnectTwoDangelingConnections(IDataStreamDefinition defintion, ISoftwareCell source,
            ISoftwareCell destination, IMainModel mainModel)
        {

            _mainModelManager.AddNewConnection(defintion, source, destination, mainModel);

            ViewRedraw();
        }


        public static void ConnectDangelingConnectionAndSoftwareCell(IDataStreamDefinition defintion, ISoftwareCell source,
            ISoftwareCell destination, IMainModel mainModel)
        {

            _mainModelManager.AddNewConnection(defintion, source, destination, mainModel);
            ViewRedraw();
        }


        public static void DeleteDatastreamDefiniton(IDataStreamDefinition dataStreamDefinition,
            ISoftwareCell softwareCell)
        {
            softwareCell.InputStreams.RemoveAll(x => x == dataStreamDefinition);
            softwareCell.OutputStreams.RemoveAll(x => x == dataStreamDefinition);
            ViewRedraw();
        }

        public static void ConsolePrintGeneratedCode(IMainModel mainModel)
        {
            //var gen = new MyGenerator();
            //Console.Clear();
            //gen.GenerateCodeAndPrint(mainModel);
        }


        public static void DebugPrint(IMainModel mainModel)
        {
            Console.Clear();
            DebugPrinter.PrintConnections(mainModel);
            DebugPrinter.PrintSoftwareCells(mainModel);
        }

        public static void AutoPrintOFF()
        {
            _aTimer.Dispose();
        }


        public static void AutoPrint(IMainModel mainModel, Action<IMainModel> printAction)
        {
            _aTimer?.Dispose();
            _aTimer = new Timer(state => { printAction(mainModel); }, null, 0, 200);
        }
    }
}