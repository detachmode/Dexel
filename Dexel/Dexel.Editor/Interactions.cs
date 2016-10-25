using System;
using System.Linq;
using System.Threading;
using System.Windows;
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
            MainModelManager.ConnectTwoCells(dataStream.Sources.First().Parent, newdestination, dataStream.DataNames, "", mainModel,
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
            MainModelManager.RemoveFromIntegrationIncludingChildren(dataStream, mainModel);

            ViewRedraw();
        }


        public static void ConnectTwoDangelingConnections(DataStreamDefinition sourceDSD, DataStreamDefinition destinationDSD, MainModel mainModel)
        {
            MainModelManager.ConnectTwoDefintions(sourceDSD, destinationDSD, mainModel);
            ViewRedraw();
        }


        public static void ConnectDangelingConnectionAndSoftwareCell(DataStreamDefinition defintionDSD, SoftwareCell destination, MainModel mainModel)
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


        public static void SaveToXML(string fileName, MainModel model)
        {
            model.SaveToXML(fileName);

        }


        public static void LoadFromXml(string fileName, MainModel model)
        {
            var loadedMainModel = XMLSaveLoad.FromXML<MainModel>(fileName);
            
            MainModelManager.SetParents(loadedMainModel);
            MainModelManager.SolveConnectionReferences(loadedMainModel);
            MainModelManager.SolveIntegrationReferences(loadedMainModel);

            ViewRedraw(loadedMainModel);
        }
    }
}