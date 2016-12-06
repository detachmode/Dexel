using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dexel.Editor.CustomControls;
using Dexel.Editor.DragAndDrop;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels.DrawingBoard
{

    [ImplementPropertyChanged]
    public class IOCellViewModel : IDropable, ISelectable
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
        public double CellWidth { get; set; }
        public double CellHeight { get; set; }
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
        public static void LoadFromModel(IOCellViewModel vm, SoftwareCell modelSoftwareCell)
        {
            vm.LoadFromModel(modelSoftwareCell);
        }

        private void LoadDangelingInputs(SoftwareCell modelSoftwareCell)
        {
            RemoveDeleted(modelSoftwareCell.InputStreams, DangelingInputs);
            UpdateOrAdd(modelSoftwareCell, modelSoftwareCell.InputStreams, DangelingInputs);
        }


        private void UpdateOrAdd(SoftwareCell modelSoftwareCell,
            List<DataStreamDefinition> streamDefinitions, 
            ObservableCollection<DangelingConnectionViewModel> dangelingConnectionViewModels)
        {
            var lookup = dangelingConnectionViewModels.ToLookup(x => x.Model.ID, x => x);
            streamDefinitions.ForEach(dataStreamDef => FindViewModel(lookup, dataStreamDef,
                onFound: viewModel =>
                {
                    if (dataStreamDef.Connected)
                        dangelingConnectionViewModels.Remove(viewModel);
                    else
                        DangelingConnectionViewModel.LoadFromModel(viewModel, modelSoftwareCell, dataStreamDef);
                },
                onNotFound: () =>
                {
                    if (dataStreamDef.Connected) return;
                    var vm = new DangelingConnectionViewModel();
                    vm.LoadFromModel(modelSoftwareCell, dataStreamDef);
                    dangelingConnectionViewModels.Add(vm);
                }));
        }


        private void FindViewModel(ILookup<Guid, DangelingConnectionViewModel> lookup, DataStreamDefinition model, Action<DangelingConnectionViewModel> onFound, Action onNotFound)
        {
            var found = lookup[model.ID].ToList();
            if (found.Any())
            {
                onFound(found.First());
            }
            else
            {
                onNotFound();
            }
        }


        private void RemoveDeleted(List<DataStreamDefinition> dsdToLoad, ObservableCollection<DangelingConnectionViewModel> observableCollection)
        {
            var todelte = observableCollection.Where(vm => dsdToLoad.All(dsd => dsd.ID != vm.Model.ID)).ToList();
            todelte.ForEach(vm => observableCollection.Remove(vm));
        }

        public void LoadDangelingOutputs(SoftwareCell modelSoftwareCell)
        {
            RemoveDeleted(modelSoftwareCell.OutputStreams, DangelingOutputs);
            UpdateOrAdd(modelSoftwareCell, modelSoftwareCell.OutputStreams, DangelingOutputs);          
        }

        #endregion
    }

}