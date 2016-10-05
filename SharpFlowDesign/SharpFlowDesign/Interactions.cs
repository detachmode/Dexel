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
            var cell = new IOCellViewModel {IsSelected = true};

            pos.X -= 100;
            pos.Y -= 20;
            //cell.Position = new Point(pos.X, pos.Y);

            MainViewModel.Instance().SoftwareCells.Add(cell);
        }


        internal static void RemoveDangelingConnection(Guid softwareCellID, Guid dataStreamId)
        {
            var softwareCell = SoftwareCellManager.GetFristByID(softwareCellID, MainModel.Get());
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


        public static void RemoveConnection(Guid id)
        {
            throw new NotImplementedException();
        }


        public static Guid AddNewSoftwareCell(string name, MainModel mainModel)
        {
            var softwareCell = new SoftwareCell();
            softwareCell.ID = Guid.NewGuid();
            softwareCell.Name = name;
            mainModel.SoftwareCells.Add(softwareCell);
            return softwareCell.ID;
        }


        public static Guid AddNewInput(Guid softwareCellID, string datanames, MainModel mainModel)
        {
            var dataStream = DataStreamManager.CreateNew(datanames);
            SoftwareCellsManager.GetAll(softwareCellID, mainModel).ToList().ForEach(x => x.InputStreams.Add(dataStream));
            return dataStream.ID;
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


        public static Guid AddNewOutput(Guid softwareCellID, string datanames, MainModel mainModel)
        {
            var dataStream = DataStreamManager.CreateNew(datanames);
            SoftwareCellsManager.GetAll(softwareCellID, mainModel).ToList()
                .ForEach(x => x.OutputStreams.Add(dataStream));
            return dataStream.ID;
        }



    }

}