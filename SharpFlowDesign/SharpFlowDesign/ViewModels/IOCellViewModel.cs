using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using FlowDesignModel;
using PropertyChanged;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.CustomControls;

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
                dangConnVM => Interactions.ConnectDangelingConnectionAndSoftwareCell(dangConnVM.Model, dangConnVM.Parent, Model, MainModel.Get()));
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
            modelSoftwareCell.InputStreams.ToList().ForEach(dataStreamDef =>
            {
                if (dataStreamDef.Connected) return;
                var vm = new DangelingConnectionViewModel();
                vm.LoadFromModel(modelSoftwareCell, dataStreamDef);
                DangelingInputs.Add(vm);
            });
        }


        private void LoadDangelingOutputs(SoftwareCell modelSoftwareCell)
        {
            DangelingOutputs.Clear();
            modelSoftwareCell.OutputStreams.ToList().ForEach(dataStreamDef =>
            {
                if (dataStreamDef.Connected) return;
                var vm = new DangelingConnectionViewModel();
                vm.LoadFromModel(modelSoftwareCell, dataStreamDef);
                DangelingOutputs.Add(vm);
            });
        }

        #endregion



        public void UpdateConnectionsPosition(Point inputPoint, Point outputPoint)
        {
            MainViewModel.Instance().UpdateConnectionsPosition(inputPoint, outputPoint, this);
        }


    }

}