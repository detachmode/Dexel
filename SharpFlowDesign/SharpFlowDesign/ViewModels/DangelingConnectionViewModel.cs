using System;
using System.Collections.Generic;
using FlowDesignModel;
using PropertyChanged;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.CustomControls;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class DangelingConnectionViewModel : IDragable, IDropable
    {
        public DangelingConnectionViewModel()
        {
            DataNames = "param";
        }
        public Guid ID;
        public DataStreamDefinition Model { get; set; }
        public SoftwareCell Parent { get; set; }
        public string DataNames { get; set; }
        public string Actionname { get; set; }

        Type IDragable.DataType => typeof (DangelingConnectionViewModel);


        public void LoadFromModel(SoftwareCell parent, DataStreamDefinition dataStream)
        {
            ID = dataStream.ID;
            Model = dataStream;
            Parent = parent;
            DataNames = dataStream.DataNames;
            Actionname = dataStream.ActionName;
        }


        public List<Type> AllowedDropTypes => new List<Type> { typeof(DangelingConnectionViewModel)};
        public void Drop(object data, int index = -1)
        {
            data.TryCast<DangelingConnectionViewModel>(dangConnVm => Interactions.ConnectTwoDangelingConnections(dangConnVm.Model, dangConnVm.Parent, this.Parent, MainModel.Get()));
        }
    }
}