using System;
using System.Collections.Generic;
using Dexel.Contracts.Model;
using Dexel.Editor.Behavior;
using Dexel.Editor.CustomControls;
using Dexel.Editor.Views;
using PropertyChanged;

namespace Dexel.Editor.ViewModels
{
    [ImplementPropertyChanged]
    public class DangelingConnectionViewModel : IDragable, IDropable
    {
        public DangelingConnectionViewModel()
        {
            DataNames = "param";
        }
        public Guid ID;
        public IDataStreamDefinition Model { get; set; }
        public ISoftwareCell Parent { get; set; }
        public string DataNames { get; set; }
        public string Actionname { get; set; }

        Type IDragable.DataType => typeof (DangelingConnectionViewModel);


        public void LoadFromModel(ISoftwareCell parent, IDataStreamDefinition dataStream)
        {
            ID = dataStream.ID;
            Model = dataStream;
            Parent = parent;
            DataNames = dataStream.DataNames;
            Actionname = dataStream.ActionName;
        }


        public List<Type> AllowedDropTypes => new List<Type> { typeof(DangelingConnectionViewModel)};
        public void Drop(object data)
        {
            data.TryCast<DangelingConnectionViewModel>(dangConnVm => Interactions.ConnectTwoDangelingConnections(dangConnVm.Model, dangConnVm.Parent, this.Parent, MainViewModel.Instance().Model));
        }
    }
}