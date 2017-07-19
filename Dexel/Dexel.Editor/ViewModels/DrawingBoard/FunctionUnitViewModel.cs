﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dexel.Editor.Views;
using Dexel.Editor.Views.CustomControls;
using Dexel.Editor.Views.DragAndDrop;
using Dexel.Library;
using Dexel.Model.DataTypes;
using PropertyChanged;
using Roslyn.Validator;

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
        public MainViewModel MainViewModel { get; set; }
        public ObservableCollection<IInputOutputViewModel> Inputs { get; set; }
        public ObservableCollection<IInputOutputViewModel> Outputs { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public ObservableCollection<FunctionUnitViewModel> Integration { get; set; }
        public string ValidationErrorMessage { get; set; }
        public Point IntegrationStartPosition { get; set; }
        public Point IntegrationEndPosition { get; set; }


        public List<Type> AllowedDropTypes => new List<Type>
        {
            typeof(IInputOutputViewModel),
            typeof(ConnectionViewModel),
             typeof(DangelingConnectionViewModel)
        };


        public void Drop(object data)
        {
            data.TryCast<DangelingConnectionViewModel>(
                dangConnVM =>
                    Interactions.ConnectDangelingConnectionAndFunctionUnit(MainViewModel, dangConnVM.Model, Model));
            data.TryCast<ConnectionViewModel>(
                connVM => Interactions.ChangeConnectionDestination(MainViewModel, connVM.Model, Model));
        }


        public bool IsSelected { get; set; }
        public bool IsInvalid { get; set; }
        public bool LoadingModelFlag => MainViewModel.Model.Runtime.IsLoading;

        public void UpdateConnectionsPosition(Point inputPoint, Point outputPoint)
        {
            MainViewModel.UpdateConnectionsPosition(MainViewModel.Connections, inputPoint, outputPoint, this);
        }

        #region Load Model

        public void LoadFromModel(MainViewModel mainModel, FunctionUnit modelFunctionUnit)
        {
            Model = modelFunctionUnit;
            MainViewModel = mainModel;
            LoadDangelingInputs(mainModel, modelFunctionUnit);
            LoadDangelingOutputs(mainModel, modelFunctionUnit);
        }


        public static void LoadFromModel(MainViewModel mainModel, FunctionUnitViewModel vm, FunctionUnit modelFunctionUnit)
        {
            vm.LoadFromModel(mainModel, modelFunctionUnit);
        }


        private void LoadDangelingInputs(MainViewModel model, FunctionUnit modelFunctionUnit)
        {
            RemoveDeleted(modelFunctionUnit.InputStreams, Inputs);
            UpdateOrAdd(model, modelFunctionUnit, modelFunctionUnit.InputStreams, Inputs);

        }


        private void UpdateOrAdd(MainViewModel model, FunctionUnit modelFunctionUnit,
            List<DataStreamDefinition> streamDefinitions,
            ObservableCollection<IInputOutputViewModel> viewmodels)
        {
            var lookup = viewmodels.ToLookup(x => x.Model.ID, x => x);
            streamDefinitions.ForEach(dataStreamDef => FindDangelingConnectionViewModel(lookup, dataStreamDef,
                onFound: viewModel => UpdateExisting(model, modelFunctionUnit, viewmodels, dataStreamDef, viewModel),
                onNotFound: () => NewViewModel(model, modelFunctionUnit, dataStreamDef, viewmodels.Add)));
        }


        private static void UpdateExisting(MainViewModel mainModel, FunctionUnit modelFunctionUnit,
            ObservableCollection<IInputOutputViewModel> viewmodels,
            DataStreamDefinition dataStreamDef, IInputOutputViewModel oldViewModel)
        {
            CheckExistingViewmodel(dataStreamDef, oldViewModel,
                correctViewModelConnected: () =>
                {
                    ((ConnectionAdapterViewModel)oldViewModel).LoadFromModel(mainModel, modelFunctionUnit, dataStreamDef);
                },
                correctViewModelUnconnected: () =>
                {
                    ((DangelingConnectionViewModel)oldViewModel).LoadFromModel(mainModel, modelFunctionUnit, dataStreamDef);
                },
                wrongViewModel: () =>
                {
                    var index = viewmodels.IndexOf(oldViewModel);
                    viewmodels.Remove(oldViewModel);
                    NewViewModel(mainModel, modelFunctionUnit, dataStreamDef, newVm => viewmodels.Insert(index, newVm));
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


        private static void NewViewModel(MainViewModel model, FunctionUnit modelFunctionUnit, DataStreamDefinition dataStreamDef, Action<IInputOutputViewModel> onNewViewModel)
        {
            if (dataStreamDef.Connected)
            {
                var vm = new ConnectionAdapterViewModel();
                vm.LoadFromModel(model, modelFunctionUnit, dataStreamDef);
                onNewViewModel(vm);

            }
            else
            {
                var vm = new DangelingConnectionViewModel();
                vm.LoadFromModel(model, modelFunctionUnit, dataStreamDef);
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


        public void LoadDangelingOutputs(MainViewModel mainViewModel, FunctionUnit modelFunctionUnit)
        {
            RemoveDeleted(modelFunctionUnit.OutputStreams, Outputs);
            CheckOrderCorrect(modelFunctionUnit.OutputStreams, Outputs, 
                onCorrectOrder: () => UpdateOrAdd(mainViewModel, modelFunctionUnit, modelFunctionUnit.OutputStreams, Outputs),
                onWrongOrder: () =>
                {
                    Outputs.Clear();
                    modelFunctionUnit.OutputStreams.ForEach(dsd => NewViewModel(mainViewModel, dsd.Parent, dsd, Outputs.Add));
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

        public void ResetToValidIncludingOutputs()
        {
            IsInvalid = false;
            ValidationErrorMessage = "";
            Outputs.ForEach(dsdVM =>
            {
                dsdVM.ValidationFlag = ValidationFlag.Valid;
                dsdVM.ValidationErrorMessage = "";
            });

            Inputs.ForEach(dsdVM =>
            {
                dsdVM.ValidationFlag = ValidationFlag.Valid;
                dsdVM.ValidationErrorMessage = "";
            });
        }


        public void SetValidationError(ValidationError error, string msg)
        {
            IsInvalid = true;
            ValidationErrorMessage = msg;
        }
    }

}