using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Dexel.Editor.ViewModels;

namespace Dexel.Editor.Views
{
    /// <summary>
    ///     Interaction logic for IOCell.xaml
    /// </summary>
    public partial class IOCell
    {
        public IOCell()
        {
            InitializeComponent();          
            Loaded += (sender, args) =>
            {
                Fu.FocusTextBox();
               
            };
            LayoutUpdated += IOCell_LayoutUpdated;
           
            
        }

        private void IOCell_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (ViewModel() != null)
            {
                ViewModel().CellWidth = Fu.ActualWidth;
                ViewModel().CellHeight = Fu.ActualHeight;
            }

            UpdateConnectionViewModels();          
            MainViewModel.Instance().UpdateIntegrationBorderPositions();
        }


        private Model.DataTypes.SoftwareCell duplicated = null;

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var iocellViewModel = DataContext as IOCellViewModel;
            if (duplicated == null && Keyboard.IsKeyDown(Key.LeftShift))
            {
                duplicated = Interactions.AddNewIOCell(iocellViewModel.Model.Position, MainViewModel.Instance().Model);
            }

            var modeltoMove = iocellViewModel?.Model;
            if (duplicated != null)
            {
                modeltoMove = duplicated;
            }
            
            Interactions.MoveSoftwareCell(modeltoMove, e.HorizontalChange, e.VerticalChange);
        }


        private IOCellViewModel ViewModel()
        {
            var cellViewModel = DataContext as IOCellViewModel;
            return cellViewModel;
        }




        private void UpdateConnectionViewModels()
        {
            var vm = ViewModel();
            if (vm == null)
            {
                return;
            }

            var outputPoint = new Point(vm.Model.Position.X + (Fu.ActualWidth),
                vm.Model.Position.Y + ActualHeight / 2);

            var inputPoint = new Point(vm.Model.Position.X,
                vm.Model.Position.Y + ActualHeight / 2);

            vm.UpdateConnectionsPosition(inputPoint, outputPoint);

        }


        private void NewOutput_click(object sender, RoutedEventArgs e)
        {
            Interactions.AddNewOutput(ViewModel().Model, "params");
        }

        private void NewInput_click(object sender, RoutedEventArgs e)
        {
            Interactions.AddNewInput(ViewModel().Model, "params");
        }
    }
}