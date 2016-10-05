using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using PropertyChanged;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.Model;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class ConnectionViewModel : INotifyPropertyChanged, IDragable
    {

        public ConnectionViewModel()
        {
            End = new Point(100,100);
        }
        public Guid ID;

        public DataStream Model { get; set; }
        public bool IsDragging { get; set; }
        public Point Start { get; set; }
        public Point? End { get; set; }
        public Point Center { get; set; }


        Type IDragable.DataType => typeof (ConnectionViewModel);

        void IDragable.Remove(object i)
        {
            Interactions.RemoveConnection(ID);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadFromModel(DataStream modelDataStream)
        {
            Model = modelDataStream;
            Model.PropertyChanged += ModelOnPropertyChanged;
            ID = modelDataStream.ID;
            //Start = modelDataStream.Sources.First().;
            //End = modelDataStream.Destinations.First();
        }


        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
          //  throw new NotImplementedException();
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}