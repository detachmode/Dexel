using System.Windows;
using System.Windows.Controls.Primitives;
using SharpFlowDesign.Model;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign.Views
{
    /// <summary>
    ///     Interaction logic for IOCell.xaml
    /// </summary>
    public partial class IOCell
    {
        public IOCell()
        {
            InitializeComponent();
            Loaded += (sender, args) => Fu.FocusTextBox();
            LayoutUpdated += IOCell_LayoutUpdated;
        }

        private void IOCell_LayoutUpdated(object sender, System.EventArgs e)
        {
            UpdateConnectionViewModels();
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var iocellViewModel = DataContext as IOCellViewModel;
            Interactions.MoveSoftwareCell(iocellViewModel?.Model, e.HorizontalChange, e.VerticalChange);
        }



        private IOCellViewModel GetDataContext()
        {
            var cellViewModel = DataContext as IOCellViewModel;
            return cellViewModel;
        }




        private void UpdateConnectionViewModels()
        {
            var vm = GetDataContext();
            if (vm == null)
            {
                return;
            }

            var outputPoint = new Point(vm.Model.Position.X + (ActualWidth - OutputFlow.ActualWidth),
                vm.Model.Position.Y + ActualHeight / 2);

            var inputPoint = new Point(vm.Model.Position.X + InputFlow.ActualWidth,
                vm.Model.Position.Y + ActualHeight / 2);

            vm.UpdateConnectionsPosition(inputPoint, outputPoint);

        }


        private void NewOutput_click(object sender, RoutedEventArgs e)
        {
            Interactions.AddNewOutput(GetDataContext().Model.ID, "params", MainModel.Get());
        }
    }
}