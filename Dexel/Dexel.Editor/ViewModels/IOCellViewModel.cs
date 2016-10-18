using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dexel.Contracts.Model;
using Dexel.Editor.Behavior;
using Dexel.Editor.CustomControls;
using Dexel.Editor.Views;
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
        }


        public ISoftwareCell Model { get; set; }
        public ObservableCollection<DangelingConnectionViewModel> DangelingInputs { get; set; }
        public ObservableCollection<DangelingConnectionViewModel> DangelingOutputs { get; set; }
        public bool IsSelected { get; set; }


        public List<Type> AllowedDropTypes => new List<Type>
        {
            typeof(DangelingConnectionViewModel),
            typeof(ConnectionViewModel)
        };

        public Action<object> Dropped;


        public void Drop(object data)
        {
            Dropped?.Invoke(data);
        }

        #region Load Model

        public void LoadFromModel(ISoftwareCell modelSoftwareCell)
        {
            Model = modelSoftwareCell;
            LoadDangelingInputs(modelSoftwareCell);
            LoadDangelingOutputs(modelSoftwareCell);
        }


        private void LoadDangelingInputs(ISoftwareCell modelSoftwareCell)
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


        private void LoadDangelingOutputs(ISoftwareCell modelSoftwareCell)
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