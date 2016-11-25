using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dexel.Editor.ViewModels;
using Dexel.Editor.ViewModels.DrawingBoard;

namespace Dexel.Editor.Views.DrawingBoard
{

    /// <summary>
    ///     Interaction logic for IOCell.xaml
    /// </summary>
    public partial class IOCell
    {

        public IOCellViewModel ViewModel()
        {
            var cellViewModel = DataContext as IOCellViewModel;
            return cellViewModel;
        }


        public IOCell()
        {
            InitializeComponent();
            LayoutUpdated += IOCell_LayoutUpdated;
        }




        private void IOCell_LayoutUpdated(object sender, EventArgs e)
        {
            if (ViewModel() != null)
            {
                ViewModel().CellWidth = Fu.ActualWidth;
                ViewModel().CellHeight = Fu.ActualHeight;
            }

            UpdateConnectionViewModels();
            MainViewModel.Instance().UpdateIntegrationBorderPositions();
        }



        private void UpdateConnectionViewModels()
        {
            var vm = ViewModel();
            if (vm == null)
            {
                return;
            }

            var outputPoint = new Point(vm.Model.Position.X + Fu.ActualWidth,
                vm.Model.Position.Y + ActualHeight/2);

            var inputPoint = new Point(vm.Model.Position.X,
                vm.Model.Position.Y + ActualHeight/2);

            vm.UpdateConnectionsPosition(inputPoint, outputPoint);
        }


        #region Context menu click events

        private void NewOutput_click(object sender, RoutedEventArgs e)
        {
            Interactions.AddNewOutput(ViewModel().Model, "params");
        }


        private void NewInput_click(object sender, RoutedEventArgs e)
        {
            Interactions.AddNewInput(ViewModel().Model, "params");
        }


        private void Copy_click(object sender, RoutedEventArgs e)
        {
            Interactions.Copy(GetSelectionOrClickedOn(), MainViewModel.Instance().Model);
        }

        private void Cut_click(object sender, RoutedEventArgs e)
        {
            Interactions.Cut(GetSelectionOrClickedOn(), MainViewModel.Instance().Model);
        }


        private void MakeIntegration_OnClick(object sender, RoutedEventArgs e)
        {
            Interactions.StartPickIntegration(ViewModel().Model);
        }


        private void RemoveFromIntegration_OnClick(object sender, RoutedEventArgs e)
        {
            Interactions.RemoveFromIntegration(ViewModel().Model, MainViewModel.Instance().Model);
        }


        private void Delete_click(object sender, RoutedEventArgs e)
        {
            Interactions.Delete(GetSelectionOrClickedOn(), MainViewModel.Instance().Model);
        }

        #endregion



        private List<Model.DataTypes.SoftwareCell> GetSelectionOrClickedOn()
        {
            var list = new List<Model.DataTypes.SoftwareCell>();
            if (MainViewModel.Instance().SelectedSoftwareCells.Count == 0)
                list.Add(ViewModel().Model);
            else
                list = MainViewModel.Instance().SelectedSoftwareCells.Select(x => x.Model).ToList();

            return list;
        }


       
    }

}