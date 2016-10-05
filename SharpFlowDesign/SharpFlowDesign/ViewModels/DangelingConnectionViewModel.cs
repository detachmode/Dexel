using System;
using PropertyChanged;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.Model;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class DangelingConnectionViewModel : IDragable
    {
        public Guid ID;
        public SoftwareCell Parent { get; set; }
        public string Datanames { get; set; }
        public string Actionname { get; set; }

        Type IDragable.DataType => typeof (ConnectionViewModel);

        void IDragable.Remove(object i)
        {
            Interactions.RemoveDangelingConnection(Parent.ID, ID);
        }

        public void LoadFromModel(SoftwareCell parent, DataStream dataStream)
        {
            ID = dataStream.ID;
            Parent = parent;
            Datanames = dataStream.DataNames;
            Actionname = dataStream.ActionName;
        }
    }
}