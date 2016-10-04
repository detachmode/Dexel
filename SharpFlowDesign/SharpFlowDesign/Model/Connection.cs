using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign.Model
{
    [ImplementPropertyChanged]
    public class Connection : ConnectionBase, INotifyPropertyChanged, IDragable
    {
        public Connection(IOCellViewModel start, IOCellViewModel end)
        {
            Start = start;
            End = end;
        }


        public Connection()
        {
            
        }


        public bool IsDragging { get; set; }
        public IOCellViewModel Start { get; set; }
        public IOCellViewModel End { get; set; }
        



        Type IDragable.DataType => typeof (Connection);

        void IDragable.Remove(object i)
        {
            MainViewModel.Instance().RemoveConnection(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}