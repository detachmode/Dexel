using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dexel.Editor.CustomControls;
using Dexel.Editor.DragAndDrop;
using Dexel.Editor.Views;
using Dexel.Library;
using Dexel.Model;
using Dexel.Model.DataTypes;
using PropertyChanged;
using SoftwareCell = Dexel.Model.DataTypes.SoftwareCell;

namespace Dexel.Editor.ViewModels
{

    [ImplementPropertyChanged]
    public class MainViewModel : IDropable
    {
        private static MainViewModel self;


        public MainViewModel()
        {
            SoftwareCells = new ObservableCollection<IOCellViewModel>();
            SelectedSoftwareCells = new ObservableCollection<IOCellViewModel>();
            Connections = new ObservableCollection<ConnectionViewModel>();
            IntegrationBorders = new ObservableCollection<IOCellViewModel>();
            FontSizeCell = 12;
            VisibilityDatanames = Visibility.Visible;
            VisibilityBlockTextBox = Visibility.Hidden;
            
            SelectedSoftwareCells.CollectionChanged += (sender, args) => Update();
        }


        public ObservableCollection<IOCellViewModel> IntegrationBorders { get; set; }
        public ObservableCollection<ConnectionViewModel> Connections { get; set; }
        public ObservableCollection<IOCellViewModel> SoftwareCells { get; set; }
        public ObservableCollection<IOCellViewModel> SelectedSoftwareCells { get; set; }
        public ConnectionViewModel TemporaryConnection { get; set; }

        public MainModel Model { get; set; }

        public int FontSizeCell { get; set; }
        public Visibility VisibilityDatanames { get; set; }
        public Visibility VisibilityBlockTextBox { get; set; }
        


        private void Update()
        {
           
            SoftwareCells.ForEach(x => x.IsSelected = false);
            SelectedSoftwareCells.ForEach(x => x.IsSelected = true);
        }


        public static MainViewModel Instance()
        {
            return self ?? (self = new MainViewModel());
        }


        public void Reload()
        {
            if (Model != null)
            {
                LoadFromModel(Model);
            }
        }


        public void SetSelection(IOCellViewModel ioCellViewModel)
        {
            if (SelectedSoftwareCells.Contains(ioCellViewModel)) return;

            SelectedSoftwareCells.Clear();
            SelectedSoftwareCells.Add(ioCellViewModel);
        }


        public void SetSelectionCTRLMod(IOCellViewModel ioCellViewModel)
        {
            //
            // Control key was held down.
            // Toggle the selection.
            //
            if (SelectedSoftwareCells.Contains(ioCellViewModel))
            {
                //
                // Item was already selected, control-click removes it from the selection.
                //
                SelectedSoftwareCells.Remove(ioCellViewModel);
            }
            else
            {
                // 
                // Item was not already selected, control-click adds it to the selection.
                //
                SelectedSoftwareCells.Add(ioCellViewModel);
            }
        }


        public void MoveSelectedIOCells(Vector dragDelta)
        {
            foreach (var iocell in SelectedSoftwareCells)
            {
                var pt = iocell.Model.Position;
                pt.X += dragDelta.X;
                pt.Y += dragDelta.Y;
                iocell.Model.Position = pt;
            }
        }


        public void DuplicateSelectionAndSelectNew()
        {
            var duplicted = Duplicate();


            // select the duplicated
            SelectedSoftwareCells.Clear();
            SoftwareCells.Where(sc => duplicted.Contains(sc.Model)).ForEach(vm => SelectedSoftwareCells.Add(vm));
        }

        public SoftwareCell DuplicateIncludingChildrenAndIntegrated(SoftwareCell softwareCell)
        {
            var list = MainModelManager.GetChildrenAndIntegrated(softwareCell, new List<SoftwareCell>(), Model);
            var copiedlist = MainModelManager.Duplicate(list, Model);
            var first = copiedlist.First(x => x.OriginGuid == softwareCell.ID);
            return first.NewCell;
        }


        private List<SoftwareCell> Duplicate()
        {
            var copiedList = MainModelManager.Duplicate(SelectedSoftwareCells.Select(vm => vm.Model).ToList(), Model);

            Reload();
            return copiedList.Select(x => x.NewCell).ToList();
        }


        public void ClearSelection()
        {
            SelectedSoftwareCells.Clear();
        }


        public void AddToSelection(IOCellViewModel ioCellViewModel)
        {
            SelectedSoftwareCells.Add(ioCellViewModel);
        }


       

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

        public void UpdateConnectionsPosition(Point inputPoint, Point outputPoint, IOCellViewModel ioCellViewModel)
        {
            var allOutputs = Connections.Where(conn => conn.Model.Sources.Any(x => x.Parent == ioCellViewModel.Model));
            var allInputs =
                Connections.Where(conn => conn.Model.Destinations.Any(x => x.Parent == ioCellViewModel.Model));

            allInputs.ToList().ForEach(x => x.End = inputPoint);
            allOutputs.ToList().ForEach(x => x.Start = outputPoint);
        }


        public void UpdateIntegrationBorderPositions()
        {
            IntegrationBorders.ForEach(iocellvm =>
            {
                if (iocellvm.Integration.Count == 0)
                {
                    return;
                }
                var tempIntegrations =
                    iocellvm.Integration.OrderBy(cellvm1 => cellvm1.Model.Position.X + cellvm1.CellWidth);
                var min = tempIntegrations.First();
                var max = tempIntegrations.Last();

                tempIntegrations = iocellvm.Integration.OrderBy(cellvm1 => cellvm1.Model.Position.Y + cellvm1.CellHeight);
                var miny = tempIntegrations.First();
                iocellvm.IntegrationStartPosition = new Point(min.Model.Position.X - 60, miny.Model.Position.Y);
                iocellvm.IntegrationEndPosition = new Point(max.Model.Position.X + max.CellWidth + 60,
                    miny.Model.Position.Y);
            });
        }

        #endregion

        #region Load Model

        public void LoadFromModel(MainModel mainModel)
        {
            Model = mainModel;
            LoadConnection(mainModel.Connections);
            LoadSoftwareCells(mainModel.SoftwareCells);
            LoadIntegrations();
        }


        private void LoadIntegrations()
        {
            IntegrationBorders.Clear();
            SoftwareCells.Where(x => x.Model.Integration.Count != 0).ToList().ForEach(hasIntegration =>
            {
                var integratedVMs =
                    SoftwareCells.Where(otherVM => hasIntegration.Model.Integration.Contains(otherVM.Model));
                integratedVMs.ToList().ForEach(hasIntegration.Integration.Add);
                IntegrationBorders.Add(hasIntegration);
            });
            UpdateIntegrationBorderPositions();
        }


        private void LoadSoftwareCells(List<SoftwareCell> softwareCells)
        {
            SoftwareCells.Clear();
            softwareCells.ForEach(modelSoftwareCell =>
            {
                var vm = new IOCellViewModel();
                vm.LoadFromModel(modelSoftwareCell);
                SoftwareCells.Add(vm);
            });
        }


        private void LoadConnection(List<DataStream> dataStreams)
        {
            Connections.Clear();
            dataStreams.Where(x => x.Sources.Any() && x.Destinations.Any()).ToList().ForEach(modelConnection =>
            {
                var vm = new ConnectionViewModel();
                vm.LoadFromModel(modelConnection);
                Connections.Add(vm);
            });
        }

        #endregion


    }

}