using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dexel.Editor.ViewModels;
using Dexel.Editor.ViewModels.DrawingBoard;

namespace Dexel.Editor.Views.DrawingBoard
{

    /// <summary>
    ///     Interaction logic for FunctionUnitView.xaml
    /// </summary>
    public partial class FunctionUnitView
    {

        public FunctionUnitViewModel ViewModel()
        {
            var cellViewModel = DataContext as FunctionUnitViewModel;
            return cellViewModel;
        }


        public FunctionUnitView()
        {
            InitializeComponent();
            LayoutUpdated += FunctionUnit_LayoutUpdated;
        }




        private void FunctionUnit_LayoutUpdated(object sender, EventArgs e)
        {
            if (MainViewModel.Instance().LoadingModelFlag)
                return;
            if (ViewModel() == null) return;

            ViewModel().Width = Fu.ActualWidth;
            ViewModel().Height = Fu.ActualHeight;

            MainViewModel.Instance().UpdateIntegrationBorderPosition(ViewModel());
            UpdateConnectionViewModels();
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



        private List<Model.DataTypes.FunctionUnit> GetSelectionOrClickedOn()
        {
            var list = new List<Model.DataTypes.FunctionUnit>();
            if (MainViewModel.Instance().SelectedFunctionUnits.Count == 0)
                list.Add(ViewModel().Model);
            else
                list = MainViewModel.Instance().SelectedFunctionUnits.Select(x => x.Model).ToList();

            return list;
        }


        private void DeleteDataStreamDefinition(object sender, RoutedEventArgs e)
        {
            var vm = (DangelingConnectionViewModel)((FrameworkElement)sender).DataContext;
            Interactions.DeleteDatastreamDefiniton(vm.Model, vm.Parent);
        }
    }

}