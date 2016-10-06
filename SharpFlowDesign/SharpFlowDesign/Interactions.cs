using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using SharpFlowDesign.Model;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign
{

    public static class Interactions
    {
        public enum DragMode
        {
            Single,
            Multiple
        }


        public static void AddNewIOCell(Point pos)
        {
            var softwareCell = SoftwareCellsManager.CreateNew();
            pos.X -= 100;
            pos.Y -= 20;
            softwareCell.Position  = new Point(pos.X, pos.Y);

            MainModel.Get().SoftwareCells.Add(softwareCell);  
            ViewRedraw();        
        }


        public static void ViewRedraw()
        {
            MainViewModel.Instance().LoadFromModel(MainModel.Get());
        }


        public static void ChangeConnectionDestination(DataStream dataStream, SoftwareCell newdestination, MainModel mainModel)
        {
            Interactions.RemoveConnection(dataStream, mainModel);
            var originalSourceID = dataStream.Sources.First().ID;
            Interactions.Connect(originalSourceID, newdestination.ID, dataStream.DataNames, mainModel,
                actionName: dataStream.ActionName);
            Interactions.ViewRedraw();
        }



        public static void ChangeConnectionDestination(Guid connectionID, Guid newdestinationID, MainModel mainModel)
        {
            var datastream = DataStreamManager.GetFirst(connectionID, mainModel);
            

        }

        internal static void RemoveDangelingConnection(Guid softwareCellID, Guid dataStreamId)
        {
            var softwareCell = SoftwareCellsManager.GetFristByID(softwareCellID, MainModel.Get());
            softwareCell.OutputStreams.RemoveAll(x => x.ID.Equals(dataStreamId));
            softwareCell.InputStreams.RemoveAll(x => x.ID.Equals(dataStreamId));
            MainViewModel.Instance().LoadFromModel(MainModel.Get());
        }


        //public static void DeselectAll()
        //{
        //    MainViewModel.Instance().SoftwareCells.ToList().ForEach( i => i.Deselect());
        //}

        internal static void OnItemDragged(IOCellViewModel cellvm, DragDeltaEventArgs dragDeltaEventArgs)
        {
            //cellvm.Move(dragDeltaEventArgs.HorizontalChange, dragDeltaEventArgs.VerticalChange);
        }


        public static void DragSelection(DragDeltaEventArgs e)
        {
            //GetSelection().ToList().ForEach(c => c.Move(e.HorizontalChange,e.VerticalChange));
        }


        public static IEnumerable<IOCellViewModel> GetSelection()
        {
            return MainViewModel.Instance().SoftwareCells.Where(c => c.IsSelected);
        }


        public static void AddNewConnectionNoDestination(IOCellViewModel ioCellViewModel)
        {
            MainViewModel.Instance().TemporaryConnection = new ConnectionViewModel
            {
                IsDragging = true
            };
        }


        public static void RemoveConnection(Guid id, MainModel mainModel)
        {
            var datastream =  DataStreamManager.GetFirst(id, mainModel);
            RemoveConnection(datastream, mainModel);


        }

        public static void RemoveConnection(DataStream dataStream, MainModel mainModel)
        {
            dataStream.Sources.ForEach(cell => cell.OutputStreams.RemoveAll(x => x.ID.Equals(dataStream.ID)));
            dataStream.Destinations.ForEach(cell => cell.InputStreams.RemoveAll(x => x.ID.Equals(dataStream.ID)));
            MainModel.Get().Connections.RemoveAll(x => x.ID.Equals(dataStream.ID));

        }


        public static Guid AddNewSoftwareCell(string name, MainModel mainModel)
        {
            var softwareCell = new SoftwareCell();
            softwareCell.ID = Guid.NewGuid();
            softwareCell.Name = name;
            mainModel.SoftwareCells.Add(softwareCell);
            return softwareCell.ID;
        }


        public static void AddNewInput(Guid softwareCellID, string datanames, MainModel mainModel, string actionName = "")
        {
            SoftwareCellsManager.GetAll(softwareCellID, mainModel).ToList()
                .ForEach(softwareCell =>
                {
                    var dataStream = DataStreamManager.CreateNew(datanames, actionName);
                    dataStream.Destinations.Add(softwareCell);
                    softwareCell.InputStreams.Add(dataStream);

                });      
        }


        public static Guid CheckForStreamWithSameName(SoftwareCell source, SoftwareCell destination,
            DataStream tempStream, MainModel mainModel,
            Action<SoftwareCell, SoftwareCell, DataStream> onFound,
            Action<SoftwareCell, SoftwareCell, DataStream, MainModel> onNotFound)
        {
            var found = source.OutputStreams.Where(x => x.DataNames.Equals(tempStream.DataNames)).ToList();
            if (found.Any())
            {
                onFound(source, destination, found.First());
                return found.First().ID;
            }

            onNotFound(source, destination, tempStream, mainModel);
            return tempStream.ID;
        }


        public static void AddToExistingConnection(SoftwareCell source, SoftwareCell destination, DataStream foundStream)
        {
            foundStream.Sources.Add(source);
            foundStream.Destinations.Add(destination);
        }


        public static void AddNewConnection(SoftwareCell source, SoftwareCell destination, DataStream dataStream,
            MainModel mainModel)
        {
            mainModel.Connections.Add(dataStream);
            dataStream.Destinations.Add(destination);
            dataStream.Sources.Add(source);
            destination.InputStreams.Add(dataStream);
            source.OutputStreams.Add(dataStream);
        }


        public static Guid Connect(Guid sourceID, Guid destinationID, string datanames, MainModel mainModel,
            string actionName = null)
        {
            var tempStream = new DataStream
            {
                ID = Guid.NewGuid(),
                ActionName = actionName,
                DataNames = datanames
            };


            var source = SoftwareCellsManager.GetFirst(sourceID, mainModel);
            var destination = SoftwareCellsManager.GetFirst(destinationID, mainModel);

            return CheckForStreamWithSameName(source, destination, tempStream, mainModel,
                AddToExistingConnection,
                AddNewConnection);
        }


        public static void AddNewOutput(Guid softwareCellID, string datanames, MainModel mainModel)
        {
          
            SoftwareCellsManager.GetAll(softwareCellID, mainModel).ToList()
                .ForEach(softwareCell =>
                {
                    var dataStream = DataStreamManager.CreateNew(datanames);
                    dataStream.Sources.Add(softwareCell);
                    softwareCell.OutputStreams.Add(dataStream);
                   
                });

            ViewRedraw();

        }


        public static void MoveSoftwareCell(SoftwareCell model, double horizontalChange, double verticalChange)
        {
            var pos = model.Position;
            pos.X += horizontalChange;
            pos.Y += verticalChange;
            model.Position = pos;
        }


        public static void ConnectDangelingConnection(DataStream dataStream, SoftwareCell softwareCell, MainModel mainModel)
        {

            softwareCell.InputStreams.Add(dataStream);
            dataStream.Destinations.Add(softwareCell);          
            mainModel.Connections.Add(dataStream);

            ViewRedraw();
        }


        public static void DeConnect(DataStream dataStream, MainModel mainModel)
        {
            dataStream.Sources.ForEach( x => AddNewOutput(x.ID, dataStream.DataNames, mainModel));
            dataStream.Destinations.ForEach(x => AddNewInput(x.ID, dataStream.DataNames, mainModel));
            RemoveConnection(dataStream, mainModel);
            ViewRedraw();
        }


        public static void ConnectTwoDangelingConnections(DataStream sourceDataStream, DataStream destinationDataStream, MainModel mainModel)
        {
            sourceDataStream.Sources.First().OutputStreams.RemoveAll(x => x.ID.Equals(sourceDataStream.ID));
            destinationDataStream.Destinations.First().InputStreams.RemoveAll(x => x.ID.Equals(destinationDataStream.ID));

            Connect(sourceDataStream.Sources.First().ID, destinationDataStream.Destinations.First().ID,
                sourceDataStream.DataNames, mainModel, actionName: sourceDataStream.ActionName);
            ViewRedraw();
        }
    }

}