using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using PropertyChanged;
using SharpFlowDesign.Annotations;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.Common;
using SharpFlowDesign.Model;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class IOCellViewModel : INotifyPropertyChanged, IDropable
    {
        public IOCellViewModel()
        {
            DangelingInputs = new ObservableCollection<DangelingConnection>();
            DangelingOutputs = new ObservableCollection<DangelingConnection>();

        }

        public static IOCellViewModel Create(SoftwareCell cell)
        {

            var newIOCellViewModel = new IOCellViewModel();
            newIOCellViewModel.LoadModel(cell);
            return newIOCellViewModel;
        }




        public SoftwareCell ModelSoftwareCell { get; set; }
        public ObservableCollection<DangelingConnection> DangelingInputs { get; set; }
        public ObservableCollection<DangelingConnection> DangelingOutputs { get; set; }

        public Point Position { get; set; }
        public bool IsSelected { get; set; }
        public double ActualWidth { get; set; }
        public double ActualHeight { get; set; }
        public Point InputPoint { get; set; }
        public Point OutputPoint { get; set; }


        private void LoadModel(SoftwareCell model)
        {
            ModelSoftwareCell = model;

            model.InputStreams.ToList().ForEach(stream =>
            {
                if (stream.Sources.Count != 0) return;
                DangelingInputs.Add(new DangelingConnection(this));

            });

            model.OutputStreams.ToList().ForEach(stream =>
            {
                if (stream.Destinations.Count != 0) return;
                DangelingOutputs.Add(new DangelingConnection(this));
            });
        }



        public Type DataType => typeof(Connection);

        public void Drop(object data, int index = -1)
        {
            var dangelingConnection = data as DangelingConnection;

            if (dangelingConnection == null) return;
            MainViewModel.Instance().Connections.Add(
                new Connection(dangelingConnection.IOCellViewModel, this));
            //           this.Children = this.GetChildren();  //refresh view
        }


        public void Move(double x, double y)
        {
            var pos = Position;
            pos.X += x;
            pos.Y += y;
            Position = pos;
        }


        public void Deselect()
        {
            IsSelected = false;
        }


        public void Select()
        {
            IsSelected = true;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RemoveDangelingConnection(DangelingConnection dangelingConnection)
        {
            DangelingInputs.Remove(dangelingConnection);
            DangelingOutputs.Remove(dangelingConnection);
        }

    }
}