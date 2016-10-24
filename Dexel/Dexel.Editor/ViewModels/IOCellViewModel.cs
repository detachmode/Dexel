using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dexel.Editor.Behavior;
using Dexel.Editor.CustomControls;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels
{

    [ImplementPropertyChanged]
    public class IOCellViewModel : IDropable
    {
        public IOCellViewModel()
        {
            DangelingInputs = new ObservableCollection<DangelingConnectionViewModel>();
            DangelingOutputs = new ObservableCollection<DangelingConnectionViewModel>();
            Integration = new ObservableCollection<IOCellViewModel>();
        }


        public SoftwareCell Model { get; set; }
        public ObservableCollection<DangelingConnectionViewModel> DangelingInputs { get; set; }
        public ObservableCollection<DangelingConnectionViewModel> DangelingOutputs { get; set; }
        public bool IsSelected { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public ObservableCollection<IOCellViewModel> Integration { get; set; }


        public List<Type> AllowedDropTypes => new List<Type>
        {
            typeof(DangelingConnectionViewModel),
            typeof(ConnectionViewModel)
        };

        public Point IntegrationStartPosition { get; set; }
        public Point IntegrationEndPosition { get; set; }


        public void Drop(object data)
        {
            data.TryCast<DangelingConnectionViewModel>(
                dangConnVM =>
                    Interactions.ConnectDangelingConnectionAndSoftwareCell(dangConnVM.Model, Model,
                        MainViewModel.Instance().Model));
            data.TryCast<ConnectionViewModel>(
                connVM => Interactions.ChangeConnectionDestination(connVM.Model, Model, MainViewModel.Instance().Model));
        }


        public void UpdateConnectionsPosition(Point inputPoint, Point outputPoint)
        {
            MainViewModel.Instance().UpdateConnectionsPosition(inputPoint, outputPoint, this);
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


        public void LoadDangelingOutputs(SoftwareCell modelSoftwareCell)
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
    }

}