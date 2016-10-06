using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using PropertyChanged;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.CustomControls;
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


        public SoftwareCell Model { get; set; }
        public ObservableCollection<DangelingConnectionViewModel> DangelingInputs { get; set; }
        public ObservableCollection<DangelingConnectionViewModel> DangelingOutputs { get; set; }
        public bool IsSelected { get; set; }


        public List<Type> AllowedDropTypes => new List<Type>
        {
            typeof(DangelingConnectionViewModel),
            typeof(ConnectionViewModel)
        };


        public void Drop(object data, int index = -1)
        {
            data.TryCast<DangelingConnectionViewModel>(
                dangConnVM => Interactions.ConnectDangelingConnection(dangConnVM.Model, Model, MainModel.Get()));
            data.TryCast<ConnectionViewModel>(
                connVM => Interactions.ChangeConnectionDestination(connVM.Model, Model, MainModel.Get()));
        }

        #region Load Model

        public void LoadFromModel(SoftwareCell modelSoftwareCell)
        {
            Model = modelSoftwareCell;
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
                vm.LoadFromModel(modelSoftwareCell, dataStream);
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

        #endregion

        #region update Connection Position when layout was updated ( size changed, position)

        public void UpdateConnectionsPosition(Point inputPoint, Point outputPoint)
        {
            UpdateInputConnections(inputPoint);
            UpdateOutputConnections(outputPoint);
        }


        private void UpdateInputConnections(Point inputPoint)
        {
            var inputIDs = Model.InputStreams.Select(x => x?.ID).ToList();
            var inputs = MainViewModel.Instance().Connections.Where(x => inputIDs.Contains(x.ID)).ToList();

            inputs.ForEach(x => { x.End = inputPoint; });
        }


        private void UpdateOutputConnections(Point outputPoint)
        {
            var outputIDs = Model.OutputStreams.Select(x => x.ID).ToList();
            var outputs = MainViewModel.Instance().Connections.Where(x => outputIDs.Contains(x.ID)).ToList();

            outputs.ForEach(x => { x.Start = outputPoint; });
        }

        #endregion
    }

}