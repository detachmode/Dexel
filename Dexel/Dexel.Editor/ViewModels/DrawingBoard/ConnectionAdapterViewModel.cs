using System;
using System.Collections.Generic;
using Dexel.Editor.CustomControls;
using Dexel.Editor.DragAndDrop;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels.DrawingBoard
{
    [ImplementPropertyChanged]
    public class ConnectionAdapterViewModel : IDragable, IDropable, IInputOutputViewModel
    {

        public Guid ID;
        public DataStreamDefinition Model { get; set; }
        public SoftwareCell Parent { get; set; }
        public string Actionname { get; set; }
        public double Width { get; set; }

        Type IDragable.DataType => typeof (ConnectionAdapterViewModel);


        public void LoadFromModel(SoftwareCell parent, DataStreamDefinition dataStream)
        {
            ID = dataStream.ID;
            Model = dataStream;
            Parent = parent;
            Actionname = dataStream.ActionName;
        }


        public static void LoadFromModel(ConnectionAdapterViewModel vm, SoftwareCell parent,
            DataStreamDefinition dataStream)
        {
            vm.LoadFromModel(parent, dataStream);
        }

        public List<Type> AllowedDropTypes => new List<Type> { typeof(ConnectionAdapterViewModel), typeof(DangelingConnectionViewModel) };
       

        public void Drop(object data)
        {
            data.TryCast<ConnectionAdapterViewModel>(
                droppedData => Interactions.SwapDataStreamOrder(droppedData.Model, this.Model, MainViewModel.Instance().Model));
            data.TryCast<DangelingConnectionViewModel>(
                droppedData => Interactions.SwapDataStreamOrder(droppedData.Model, this.Model, MainViewModel.Instance().Model));
        }
    }
}