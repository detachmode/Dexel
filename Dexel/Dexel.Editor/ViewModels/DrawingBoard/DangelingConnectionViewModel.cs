using System;
using System.Collections.Generic;
using Dexel.Editor.CustomControls;
using Dexel.Editor.DragAndDrop;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels.DrawingBoard
{
    [ImplementPropertyChanged]
    public class DangelingConnectionViewModel : IDragable, IDropable, IInputOutputViewModel
    {
        public DangelingConnectionViewModel()
        {
            DataNames = "param";
        }

        public Guid ID;
        public DataStreamDefinition Model { get; set; }
        public FunctionUnit Parent { get; set; }
        public string DataNames { get; set; }
        public string Actionname { get; set; }
        public double Width { get; set; }

        Type IDragable.DataType => typeof (DangelingConnectionViewModel);


        public void LoadFromModel(FunctionUnit parent, DataStreamDefinition dataStream)
        {
            ID = dataStream.ID;
            Model = dataStream;
            Parent = parent;
            DataNames = dataStream.DataNames;
            Actionname = dataStream.ActionName;
        }


        public static void LoadFromModel(DangelingConnectionViewModel vm, FunctionUnit parent,
            DataStreamDefinition dataStream)
        {
            vm.LoadFromModel(parent, dataStream);
        }

        public List<Type> AllowedDropTypes => new List<Type> { typeof(DangelingConnectionViewModel), typeof(ConnectionAdapterViewModel)};
       


        public void Drop(object data)
        {
            data.TryCast<DangelingConnectionViewModel>(
                dangConnVm => Interactions.DragDroppedTwoDangelingConnections(dangConnVm.Model, this.Model, MainViewModel.Instance().Model));
            data.TryCast<ConnectionAdapterViewModel>(
               dangConnVm => Interactions.SwapDataStreamOrder(dangConnVm.Model, this.Model, MainViewModel.Instance().Model));
        }
    }
}