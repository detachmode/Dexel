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



        private static Timer aTimer;

        private static readonly DataStreamManager DataStreamManager = new DataStreamManager();
        private static readonly SoftwareCellsManager SoftwareCellsManager = new SoftwareCellsManager();
        private static readonly MainModelManager MainModelManager = new MainModelManager(SoftwareCellsManager, DataStreamManager);






        public static void AddNewIOCell(Point pos, IMainModel mainModel)
        {
            var softwareCell = SoftwareCellsManager.CreateNew();
            pos.X -= 100;
            pos.Y -= 20;
            softwareCell.Position = new Point(pos.X, pos.Y);
            softwareCell.InputStreams.Add(DataStreamManager.CreateNewDefinition(softwareCell, "input"));
            softwareCell.OutputStreams.Add(DataStreamManager.CreateNewDefinition(softwareCell, "output"));

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
            MainModelManager.Connect(dataStream.Sources.First().Parent, newdestination, dataStream.DataNames, mainModel,
                dataStream.ActionName);

            ViewRedraw();
        }


        public static void AddNewOutput(ISoftwareCell softwareCell, string datanames)
        {
            MainModelManager.AddNewOutput(softwareCell, datanames);
            ViewRedraw();
        }


        public static void AddNewInput(ISoftwareCell softwareCell, string datanames)
        {
            MainModelManager.AddNewInput(softwareCell, datanames);
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

            dataStream.Sources.ForEach(dsd => dsd.Connected = false);
            dataStream.Destinations.ForEach(dsd => dsd.Connected = false);
          
            MainModelManager.RemoveConnection(dataStream, mainModel);
            ViewRedraw();
        }


        public static void ConnectTwoDangelingConnections(IDataStreamDefinition sourceDSD, IDataStreamDefinition destinationDSD, IMainModel mainModel)
        {
            var datastream = DataStreamManager.CreateNew(DataStreamManager.MergeDataNames(sourceDSD, destinationDSD));
            datastream.Sources.Add(sourceDSD);
            datastream.Destinations.Add(destinationDSD);
            mainModel.Connections.Add(datastream);

            destinationDSD.Connected = true;
            sourceDSD.Connected = true;

            ViewRedraw();
        }


        public static void ConnectDangelingConnectionAndSoftwareCell(IDataStreamDefinition defintionDSD, ISoftwareCell destination, IMainModel mainModel)
        {

            //SoftwareCellsManager.RemoveDefinitionsFromSourceAndDestination(defintion, source, destination);
            var dataStream = DataStreamManager.CreateNew(DataStreamManager.MergeDataNames(defintionDSD, null), defintionDSD.ActionName);
            mainModel.Connections.Add(dataStream);

            defintionDSD.Connected = true;
            dataStream.Sources.Add(defintionDSD);

            var inputDefinition = DataStreamManager.CreateNewDefinition(destination, "");
            inputDefinition.Connected = true;
            destination.InputStreams.Add(inputDefinition);
            dataStream.Destinations.Add(inputDefinition);




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
            var gen = new MyGenerator();
            Console.Clear();
            gen.GenerateCodeAndPrint(mainModel);
        }


        public static void DebugPrint(IMainModel mainModel)
        {
            Console.Clear();
            DebugPrinter.PrintConnections(mainModel);
            DebugPrinter.PrintSoftwareCells(mainModel);
        }

        public static void AutoPrintOff()
        {
            aTimer.Dispose();
        }


        public static void AutoPrint(IMainModel mainModel, Action<IMainModel> printAction)
        {
            aTimer?.Dispose();
            aTimer = new Timer(state => { printAction(mainModel); }, null, 0, 200);
        }


        public static void ChangeConnectionDatanames(IDataStream datastream, string newDatanames)
        {
            DataStreamManager.ChangeDatanames(datastream, newDatanames);
            
        }
    }
}