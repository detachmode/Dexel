using System;
using PropertyChanged;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.Model;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class DangelingConnectionViewModel : IDragable
    {
        public DangelingConnectionViewModel()
        {
            DataNames = "param";
        }
        public Guid ID;
        public SoftwareCell Parent { get; set; }
        public string DataNames { get; set; }
        public string Actionname { get; set; }

        Type IDragable.DataType => typeof (DangelingConnectionViewModel);

        void IDragable.Remove(object i)
        {
            Interactions.RemoveDangelingConnection(Parent.ID, ID);
        }

        public void LoadFromModel(SoftwareCell parent, DataStream dataStream)
        {
            ID = dataStream.ID;
            Parent = parent;
            DataNames = dataStream.DataNames;
            Actionname = dataStream.ActionName;
        }
    }
}