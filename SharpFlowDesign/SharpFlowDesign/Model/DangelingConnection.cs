using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;
using SharpFlowDesign.Annotations;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign.Model
{
    [ImplementPropertyChanged]
    public class DangelingConnection : ConnectionBase, IDragable, INotifyPropertyChanged
    {
        public DangelingConnection(SoftwareCell cell)
        {
            ConnectedToCell = cell;
        }


        public DangelingConnection()
        {
            
        }


        public SoftwareCell ConnectedToCell { get; set; }

        Type IDragable.DataType => typeof (Connection);

        void IDragable.Remove(object i)
        {
            ConnectedToCell.RemoveDangelingConnection(this);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}