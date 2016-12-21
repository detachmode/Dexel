using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dexel.Editor.Views;
using Dexel.Editor.Views.CustomControls;
using Dexel.Editor.Views.DragAndDrop;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels.DrawingBoard
{

    [ImplementPropertyChanged]
    public class FunctionUnitViewModel : IDropable, ISelectable
    {
        public FunctionUnitViewModel()
        {
            Inputs = new ObservableCollection<IInputOutputViewModel>();
            Outputs = new ObservableCollection<IInputOutputViewModel>();
            Integration = new ObservableCollection<FunctionUnitViewModel>();
        }


        public FunctionUnit Model { get; set; }
        public ObservableCollection<IInputOutputViewModel> Inputs { get; set; }
        public ObservableCollection<IInputOutputViewModel> Outputs { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public ObservableCollection<FunctionUnitViewModel> Integration { get; set; }

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
                    Interactions.ConnectDangelingConnectionAndFunctionUnit(dangConnVM.Model, Model,
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

        public void LoadFromModel(FunctionUnit modelFunctionUnit)
        {
            Model = modelFunctionUnit;
            LoadDangelingInputs(modelFunctionUnit);
            LoadDangelingOutputs(modelFunctionUnit);
        }


        public static void LoadFromModel(FunctionUnitViewModel vm, FunctionUnit modelFunctionUnit)
        {
            vm.LoadFromModel(modelFunctionUnit);
        }


        private void LoadDangelingInputs(FunctionUnit modelFunctionUnit)
        {
            RemoveDeleted(modelFunctionUnit.InputStreams, Inputs);
            UpdateOrAdd(modelFunctionUnit, modelFunctionUnit.InputStreams, Inputs);

        }


        private void UpdateOrAdd(FunctionUnit modelFunctionUnit,
            List<DataStreamDefinition> streamDefinitions,
            ObservableCollection<IInputOutputViewModel> viewmodels)
        {
            var lookup = viewmodels.ToLookup(x => x.Model.ID, x => x);
            streamDefinitions.ForEach(dataStreamDef => FindDangelingConnectionViewModel(lookup, dataStreamDef,
                onFound: viewModel => UpdateExisting(modelFunctionUnit, viewmodels, dataStreamDef, viewModel),
                onNotFound: () => NewViewModel(modelFunctionUnit, dataStreamDef, viewmodels.Add)));
        }


        private static void UpdateExisting(FunctionUnit modelFunctionUnit,
            ObservableCollection<IInputOutputViewModel> viewmodels,
            DataStreamDefinition dataStreamDef, IInputOutputViewModel oldViewModel)
        {
            CheckExistingViewmodel(dataStreamDef, oldViewModel,
                correctViewModelConnected: () => ConnectionAdapterViewModel.LoadFromModel((ConnectionAdapterViewModel)oldViewModel, modelFunctionUnit, dataStreamDef),
                correctViewModelUnconnected: () => DangelingConnectionViewModel.LoadFromModel((DangelingConnectionViewModel)oldViewModel, modelFunctionUnit, dataStreamDef),
                wrongViewModel: () =>
                {
                    var index = viewmodels.IndexOf(oldViewModel);
                    viewmodels.Remove(oldViewModel);
                    NewViewModel(modelFunctionUnit, dataStreamDef, newVm => viewmodels.Insert(index, newVm));
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


        private static void NewViewModel(FunctionUnit modelFunctionUnit, DataStreamDefinition dataStreamDef, Action<IInputOutputViewModel> onNewViewModel)
        {
            if (dataStreamDef.Connected)
            {
                var vm = new ConnectionAdapterViewModel();
                vm.LoadFromModel(modelFunctionUnit, dataStreamDef);
                onNewViewModel(vm);

            }
            else
            {
                var vm = new DangelingConnectionViewModel();
                vm.LoadFromModel(modelFunctionUnit, dataStreamDef);
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


        public void LoadDangelingOutputs(FunctionUnit modelFunctionUnit)
        {
            RemoveDeleted(modelFunctionUnit.OutputStreams, Outputs);
            CheckOrderCorrect(modelFunctionUnit.OutputStreams, Outputs, 
                onCorrectOrder: () => UpdateOrAdd(modelFunctionUnit, modelFunctionUnit.OutputStreams, Outputs),
                onWrongOrder: () =>
                {
                    Outputs.Clear();
                    modelFunctionUnit.OutputStreams.ForEach(dsd => NewViewModel(dsd.Parent, dsd, Outputs.Add));
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