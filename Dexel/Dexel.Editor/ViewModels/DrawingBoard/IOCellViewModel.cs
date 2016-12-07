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
            Inputs = new ObservableCollection<IInputOutputViewModel>();
            Outputs = new ObservableCollection<IInputOutputViewModel>();
            Integration = new ObservableCollection<IOCellViewModel>();
        }


        public SoftwareCell Model { get; set; }
        public ObservableCollection<IInputOutputViewModel> Inputs { get; set; }
        public ObservableCollection<IInputOutputViewModel> Outputs { get; set; }
        public double CellWidth { get; set; }
        public double CellHeight { get; set; }
        public ObservableCollection<IOCellViewModel> Integration { get; set; }

        public Point IntegrationStartPosition { get; set; }
        public Point IntegrationEndPosition { get; set; }


        public List<Type> AllowedDropTypes => new List<Type>
        {
            typeof(IInputOutputViewModel),
            typeof(ConnectionViewModel)
        };


        public void Drop(object data)
        {
            data.TryCast<DangelingConnectionViewModel>(
                dangConnVM =>
                    Interactions.ConnectDangelingConnectionAndSoftwareCell(dangConnVM.Model, Model,
                        MainViewModel.Instance().Model));
            data.TryCast<ConnectionViewModel>(
                connVM => Interactions.ChangeConnectionDestination(connVM.Model, Model, MainViewModel.Instance().Model));
        }


        public bool IsSelected { get; set; }


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
            RemoveDeleted(modelSoftwareCell.InputStreams, Inputs);
            UpdateOrAdd(modelSoftwareCell, modelSoftwareCell.InputStreams, Inputs);

        }


        private void UpdateOrAdd(SoftwareCell modelSoftwareCell,
            List<DataStreamDefinition> streamDefinitions,
            ObservableCollection<IInputOutputViewModel> viewmodels)
        {
            var lookup = viewmodels.ToLookup(x => x.Model.ID, x => x);
            streamDefinitions.ForEach(dataStreamDef => FindDangelingConnectionViewModel(lookup, dataStreamDef,
                onFound: viewModel => UpdateExisting(modelSoftwareCell, viewmodels, dataStreamDef, viewModel),
                onNotFound: () => NewViewModel(modelSoftwareCell, dataStreamDef, viewmodels.Add)));
        }


        private static void UpdateExisting(SoftwareCell modelSoftwareCell,
            ObservableCollection<IInputOutputViewModel> viewmodels,
            DataStreamDefinition dataStreamDef, IInputOutputViewModel oldViewModel)
        {
            CheckExistingViewmodel(dataStreamDef, oldViewModel,
                correctViewModelConnected: () => ConnectionAdapterViewModel.LoadFromModel((ConnectionAdapterViewModel)oldViewModel, modelSoftwareCell, dataStreamDef),
                correctViewModelUnconnected: () => DangelingConnectionViewModel.LoadFromModel((DangelingConnectionViewModel)oldViewModel, modelSoftwareCell, dataStreamDef),
                wrongViewModel: () =>
                {
                    var index = viewmodels.IndexOf(oldViewModel);
                    viewmodels.Remove(oldViewModel);
                    NewViewModel(modelSoftwareCell, dataStreamDef, newVm => viewmodels.Insert(index, newVm));
                }
                );
        }


        private static void CheckExistingViewmodel(DataStreamDefinition dataStreamDef,
            IInputOutputViewModel viewModel, Action correctViewModelConnected, Action correctViewModelUnconnected, Action wrongViewModel)
        {
            if (dataStreamDef.Connected)
            {
                if (viewModel is ConnectionAdapterViewModel)
                    correctViewModelConnected();
                else
                {
                    wrongViewModel();
                }
            }

            if (!dataStreamDef.Connected)
            {
                if (viewModel is DangelingConnectionViewModel)
                    correctViewModelUnconnected();
                else
                {
                    wrongViewModel();
                }
            }
        }


        private static void NewViewModel(SoftwareCell modelSoftwareCell, DataStreamDefinition dataStreamDef, Action<IInputOutputViewModel> onNewViewModel)
        {
            if (dataStreamDef.Connected)
            {
                var vm = new ConnectionAdapterViewModel();
                vm.LoadFromModel(modelSoftwareCell, dataStreamDef);
                onNewViewModel(vm);

            }
            else
            {
                var vm = new DangelingConnectionViewModel();
                vm.LoadFromModel(modelSoftwareCell, dataStreamDef);
                onNewViewModel(vm);
            }

        }


        private void FindDangelingConnectionViewModel(ILookup<Guid, IInputOutputViewModel> lookup, DataStreamDefinition model,
            Action<IInputOutputViewModel> onFound, Action onNotFound)
        {
            var found = lookup[model.ID].ToList();
            if (found.Any())
                onFound(found.First());
            else
                onNotFound();
        }


        private void RemoveDeleted(List<DataStreamDefinition> dsdToLoad,
            ObservableCollection<IInputOutputViewModel> observableCollection)
        {
            var todelte = observableCollection.Where(vm => dsdToLoad.All(dsd => dsd.ID != vm.Model.ID)).ToList();
            todelte.ForEach(vm => observableCollection.Remove(vm));
        }


        public void LoadDangelingOutputs(SoftwareCell modelSoftwareCell)
        {
            RemoveDeleted(modelSoftwareCell.OutputStreams, Outputs);
            CheckOrderCorrect(modelSoftwareCell.OutputStreams, Outputs, 
                onCorrectOrder: () => UpdateOrAdd(modelSoftwareCell, modelSoftwareCell.OutputStreams, Outputs),
                onWrongOrder: () =>
                {
                    Outputs.Clear();
                    modelSoftwareCell.OutputStreams.ForEach(dsd => NewViewModel(dsd.Parent, dsd, Outputs.Add));
                });
        }





        private void CheckOrderCorrect(List<DataStreamDefinition> modelDataStreamDefinitions, ObservableCollection<IInputOutputViewModel> viewmodels, Action onCorrectOrder, Action onWrongOrder)
        {

            int idx = -1;
            var correct = viewmodels.All(vm =>
            {
                ++idx;
                return vm.Model == modelDataStreamDefinitions[idx];
            });

            if (correct)
                onCorrectOrder();
            else
                onWrongOrder();
        }

        #endregion
    }

}