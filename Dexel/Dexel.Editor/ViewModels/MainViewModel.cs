using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dexel.Editor.ViewModels.DataTypeEditor;
using Dexel.Editor.ViewModels.DrawingBoard;
using Dexel.Editor.Views;
using Dexel.Editor.Views.CustomControls;
using Dexel.Editor.Views.DragAndDrop;
using Dexel.Library;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using PropertyChanged;

namespace Dexel.Editor.ViewModels
{

    [ImplementPropertyChanged]
    public class MainViewModel : IDropable
    {
        private static MainViewModel _self;
        public bool LoadingModelFlag;


        public MainViewModel()
        {
            FunctionUnits = new ObservableCollection<FunctionUnitViewModel>();
            SelectedFunctionUnits = new ObservableCollection<FunctionUnitViewModel>();
            Connections = new ObservableCollection<ConnectionViewModel>();
            IntegrationBorders = new ObservableCollection<FunctionUnitViewModel>();
            DataTypes = new ObservableCollection<DataTypeViewModel>();
            VisibileDataTypes = new ObservableCollection<DataTypeViewModel>();
            FontSizeFunctionUnit = 12;
            VisibilityDatanames = Visibility.Visible;
            VisibilityBlockTextBox = Visibility.Hidden;
            SelectedFunctionUnits.CollectionChanged += (sender, args) => UpdateSelectionState();
        }


        public ObservableCollection<DataTypeViewModel> DataTypes { get; set; }
        public ObservableCollection<FunctionUnitViewModel> IntegrationBorders { get; set; }
        public ObservableCollection<ConnectionViewModel> Connections { get; set; }
        public ObservableCollection<FunctionUnitViewModel> FunctionUnits { get; set; }
        public ObservableCollection<FunctionUnitViewModel> SelectedFunctionUnits { get; set; }
        public ObservableCollection<DataTypeViewModel> VisibileDataTypes { get; set; }
        public ConnectionViewModel TemporaryConnection { get; set; }
        public MainModel Model { get; set; }
        public int FontSizeFunctionUnit { get; set; }
        public Visibility VisibilityDatanames { get; set; }
        public Visibility VisibilityBlockTextBox { get; set; }
        public int MissingDataTypes { get; set; }

        public static MainViewModel Instance() => _self ?? (_self = new MainViewModel());




        #region Modify Selection

        private void UpdateSelectionState()
        {
            FunctionUnits.ForEach(x => x.IsSelected = false);
            SelectedFunctionUnits.ForEach(x => x.IsSelected = true);
        }


        public void SetSelection(FunctionUnitViewModel functionUnitViewModel)
        {
            if (SelectedFunctionUnits.Contains(functionUnitViewModel)) return;

            SelectedFunctionUnits.Clear();
            SelectedFunctionUnits.Add(functionUnitViewModel);
        }


        public void SetSelectionCTRLMod(FunctionUnitViewModel functionUnitViewModel)
        {
            if (SelectedFunctionUnits.Contains(functionUnitViewModel))
                SelectedFunctionUnits.Remove(functionUnitViewModel);
            else
                SelectedFunctionUnits.Add(functionUnitViewModel);
        }


        public void MoveSelectedFunctionUnit(Vector dragDelta)
        {
            SelectedFunctionUnits.ForEach(sc => Interactions.MoveFunctionUnit(sc.Model, dragDelta.X, dragDelta.Y));
        }


        public void DuplicateSelectionAndSelectNew()
        {
            var duplicted = DuplicateSelection();
            Select(duplicted);
        }


        private void Select(List<FunctionUnit> duplicted)
        {
            SelectedFunctionUnits.Clear();
            FunctionUnits.Where(sc => duplicted.Contains(sc.Model)).ForEach(vm => SelectedFunctionUnits.Add(vm));
        }


        public FunctionUnit DuplicateIncludingChildrenAndIntegrated(FunctionUnit functionUnit)
        {
            var list = MainModelManager.GetChildrenAndIntegrated(functionUnit, new List<FunctionUnit>(), Model);
            var copiedlist = MainModelManager.Duplicate(list, Model);
            var first = copiedlist.First(x => x.OriginGuid == functionUnit.ID);
            return first.NewFunctionUnit;
        }


        private List<FunctionUnit> DuplicateSelection()
        {
            var copiedList = MainModelManager.Duplicate(SelectedFunctionUnits.Select(vm => vm.Model).ToList(), Model);

            Reload();
            return copiedList.Select(x => x.NewFunctionUnit).ToList();
        }


        public void ClearSelection()
        {
            SelectedFunctionUnits.Clear();
        }


        public void AddToSelection(FunctionUnitViewModel functionUnitViewModel)
        {
            SelectedFunctionUnits.Add(functionUnitViewModel);
        }

        #endregion

        #region Drop

        public List<Type> AllowedDropTypes => new List<Type>
        {
            typeof(ConnectionViewModel)
        };


        public void Drop(object data)
        {
            data.TryCast<ConnectionViewModel>(
                connectionVM => Interactions.DeConnect(connectionVM.Model, Model));
        }

        #endregion

        #region Update Positions

        public void UpdateConnectionsPosition(Point inputPoint, Point outputPoint, FunctionUnitViewModel functionUnitViewModel)
        {


            var allOutputs = Connections.Where(conn => conn.Model.Sources.Any(x => x.Parent == functionUnitViewModel.Model));
            var allInputs = Connections.Where(conn => conn.Model.Destinations.Any(x => x.Parent == functionUnitViewModel.Model));

            allInputs.ToList().ForEach(connVm =>
            {
                SetInputPosition(inputPoint, connVm, functionUnitViewModel);
            });

            allOutputs.ToList().ForEach(connVm =>
            {
                SetOutputPosition(outputPoint, connVm, functionUnitViewModel);
            });
        }


        private void SetInputPosition(Point point, ConnectionViewModel connVm, FunctionUnitViewModel functionUnitViewModel)
        {
            var inputVm = (ConnectionAdapterViewModel)functionUnitViewModel.Inputs.First(ioVm => ioVm.Model == connVm.Model.Destinations.First());
            var index = functionUnitViewModel.Inputs.IndexOf(inputVm);
            var count = functionUnitViewModel.Inputs.Count;

            var pt = OffsetPos(point, count, index, inputVm, isOutput:false);

            connVm.End = pt;
        }


        private void SetOutputPosition(Point point, ConnectionViewModel connVm, FunctionUnitViewModel functionUnitViewModel)
        {
            var outputVm = (ConnectionAdapterViewModel)functionUnitViewModel.Outputs.First(ioVm => ioVm.Model == connVm.Model.Sources.First());
            var index = functionUnitViewModel.Outputs.IndexOf(outputVm);
            var count = functionUnitViewModel.Outputs.Count;

            var pt = OffsetPos(point, count, index, outputVm);

            connVm.Start = pt;
        }


        private static Point OffsetPos(Point point, int count, int index, ConnectionAdapterViewModel adapterViewModel, bool isOutput = true)
        {
            const int dsdHeight = 42;

            var pt = new Point
            {
                X = point.X,
                Y = point.Y
            };

            pt.Y -= (count - 1)*(dsdHeight/2);
            pt.Y += index*dsdHeight + 2;

            if (isOutput)
                pt.X += adapterViewModel.Width - 1;


            return pt;
        }


        public void UpdateIntegrationBorderPositions(ObservableCollection<FunctionUnitViewModel> integrationsBorders)
        {
            integrationsBorders.ForEach(UpdateIntegrationBorderPosition);
        }


        public void UpdateIntegrationBorderPosition(FunctionUnitViewModel fuVm)
        {
            if (fuVm.Integration.Count == 0)
                return;
            var tempIntegrations =
                fuVm.Integration.OrderBy(vm => vm.Model.Position.X + vm.Width);
            var min = tempIntegrations.First();
            var max = tempIntegrations.Last();

            tempIntegrations = fuVm.Integration.OrderBy(vm => vm.Model.Position.Y + vm.Height);
            var miny = tempIntegrations.First();
            fuVm.IntegrationStartPosition = new Point(min.Model.Position.X - 60, miny.Model.Position.Y);
            fuVm.IntegrationEndPosition = new Point(max.Model.Position.X + max.Width + 60,
                miny.Model.Position.Y);
        }

        #endregion

        #region Load Model

        public void Reload()
        {
            if (Model != null)
                LoadFromModel(Model);
        }


        public void LoadFromModel(MainModel mainModel)
        {
            LoadingModelFlag = true;
            try
            {
                Model = mainModel;
                LoadFunctionUnits(mainModel.FunctionUnits);
                LoadConnection(mainModel.Connections);               
                LoadIntegrations();
                LoadDataTypes(mainModel.DataTypes);
            }
            finally
            {
                LoadingModelFlag = false;
            }
        }


        private void LoadDataTypes(List<CustomDataType> dataTypes)
        {
            DataTypes.Clear();
            dataTypes.ForEach(dataType =>
            {
                var vm = new DataTypeViewModel();
                vm.Model = dataType;

                if ((dataType.SubDataTypes == null) || !dataType.SubDataTypes.Any())
                    vm.Definitions = "";
                else
                    vm.Definitions = dataType.SubDataTypes
                        .Select(x => string.IsNullOrEmpty(x.Name) ? x.Type : $"{x.Name}:{x.Type}")
                        .Aggregate((str, type) => str + "\n" + type);

                DataTypes.Add(vm);
            });
        }


        private void LoadIntegrations()
        {
            var newcollection = new ObservableCollection<FunctionUnitViewModel>();
            FunctionUnits.Where(x => x.Model.IsIntegrating.Count != 0).ToList().ForEach(hasIntegration =>
            {
                var integratedVMs =
                    FunctionUnits.Where(otherVM => hasIntegration.Model.IsIntegrating.Contains(otherVM.Model));
                var list = new ObservableCollection<FunctionUnitViewModel>();
                integratedVMs.ToList().ForEach(list.Add);
                hasIntegration.Integration = list;
                newcollection.Add(hasIntegration);
            });

            UpdateIntegrationBorderPositions(newcollection);
            IntegrationBorders = newcollection;
        }


        private void LoadFunctionUnits(List<FunctionUnit> functionUnitsToLoad)
        {
            RemoveDeletedFunctionUnits(functionUnitsToLoad);

            var lookup = FunctionUnits.ToLookup(x => x.Model.ID, x => x);
            functionUnitsToLoad.ForEach(model => FindFunctionUnitViewModel(lookup, model,
                onFound: viewModel => FunctionUnitViewModel.LoadFromModel(viewModel, model),
                onNotFound: () => AddNewFunctionUnit(model)));
        }


        private void AddNewFunctionUnit(FunctionUnit model)
        {
            var vm = new FunctionUnitViewModel();
            vm.LoadFromModel(model);
            FunctionUnits.Add(vm);
        }


        private void FindFunctionUnitViewModel(ILookup<Guid, FunctionUnitViewModel> lookup, FunctionUnit model,
            Action<FunctionUnitViewModel> onFound, Action onNotFound)
        {
            var found = lookup[model.ID].ToList();
            if (found.Any())
                onFound(found.First());
            else
                onNotFound();
        }


        private void FindConnectionViewModel(ILookup<Guid, ConnectionViewModel> lookup, DataStream model,
            Action<ConnectionViewModel> onFound, Action onNotFound)
        {
            var found = lookup[model.ID].ToList();
            if (found.Any())
                onFound(found.First());
            else
                onNotFound();
        }


        private void RemoveDeletedFunctionUnits(List<FunctionUnit> functionUnitsToLoad)
        {
            var todelte = FunctionUnits.Where(vm => functionUnitsToLoad.All(fu => fu.ID != vm.Model.ID)).ToList();
            todelte.ForEach(vm => FunctionUnits.Remove(vm));
        }


        private void LoadConnection(List<DataStream> datastreamsToLoad)
        {
            RemoveDeletedConnections(datastreamsToLoad);

            var lookup = Connections.ToLookup(x => x.Model.ID, x => x);
            datastreamsToLoad.ForEach(model => FindConnectionViewModel(lookup, model,
                onFound: viewModel => ConnectionViewModel.LoadFromModel(viewModel, model),
                onNotFound: () => AddNewConnection(model)));
        }


        private void AddNewConnection(DataStream model)
        {
            var vm = new ConnectionViewModel();
            vm.LoadFromModel(model);
            Connections.Add(vm);
        }


        private void RemoveDeletedConnections(List<DataStream> datastreamsToLoad)
        {
            var todelte = Connections.Where(vm => datastreamsToLoad.All(fu => fu.ID != vm.Model.ID)).ToList();
            todelte.ForEach(vm => Connections.Remove(vm));
        }

        #endregion
    }

}