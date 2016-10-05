using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using PropertyChanged;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.Model;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class IOCellViewModel : IDropable
    {
        public IOCellViewModel()
        {
            DangelingInputs = new ObservableCollection<DangelingConnectionViewModel>();
            DangelingOutputs = new ObservableCollection<DangelingConnectionViewModel>();
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
        }
        public SoftwareCell Model { get; set; }
        public ObservableCollection<DangelingConnectionViewModel> DangelingInputs { get; set; }
        public ObservableCollection<DangelingConnectionViewModel> DangelingOutputs { get; set; }
        public bool IsSelected { get; set; }
        public double ActualWidth { get; set; }
        public double ActualHeight { get; set; }
        public Point InputPoint { get; set; }
        public Point OutputPoint { get; set; }


        public Type DataType => typeof (DangelingConnectionViewModel);

        public void Drop(object data, int index = -1)
        {
            var dangelingConnection = data as DangelingConnectionViewModel;

            //Interactions.ConnectDangelingConnection();
            
          
            if (dangelingConnection != null)
            {
                MainViewModel.Instance().Connections.Add(
                    new ConnectionViewModel()
                    {
                        Model = new DataStream(),
                       
                    });
            }
            //           this.Children = this.GetChildren();  //refresh view
        }




        public void LoadFromModel(SoftwareCell modelSoftwareCell)
        {
            Model = modelSoftwareCell;
            Model.PropertyChanged += Model_PropertyChanged;

            //this.Name = modelSoftwareCell.Name;
            LoadDangelingInputs(modelSoftwareCell);
            LoadDangelingOutputs(modelSoftwareCell);
        }

        private void LoadDangelingInputs(SoftwareCell modelSoftwareCell)
        {
            DangelingInputs.Clear();
            modelSoftwareCell.InputStreams.ToList().ForEach(dataStream =>
            {
                if (dataStream.Sources.Count != 0) return;
                var vm = new DangelingConnectionViewModel();
                vm.LoadFromModel(modelSoftwareCell,dataStream);
                DangelingInputs.Add(vm);
            });
        }

        private void LoadDangelingOutputs(SoftwareCell modelSoftwareCell)
        {
            DangelingOutputs.Clear();
            modelSoftwareCell.OutputStreams.ToList().ForEach(dataStream =>
            {
                if (dataStream.Destinations.Count != 0) return;
                var vm = new DangelingConnectionViewModel();
                vm.LoadFromModel(modelSoftwareCell, dataStream);
                DangelingOutputs.Add(vm);
            });
        }
    }
}