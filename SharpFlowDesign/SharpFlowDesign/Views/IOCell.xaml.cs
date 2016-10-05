using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using SharpFlowDesign.Model;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign.Views
{
    /// <summary>
    ///     Interaction logic for IOCell.xaml
    /// </summary>
    public partial class IOCell
    {
        private static bool _isDraggingMode;


        public IOCell()
        {
            InitializeComponent();
            Loaded += (sender, args) => Fu.FocusTextBox();
            LayoutUpdated += IOCell_LayoutUpdated;
            
        }

        private void IOCell_LayoutUpdated(object sender, System.EventArgs e)
        {
            updateConnectionViewModels();
        }





        // Event hanlder for dragging functionality support same to all thumbs
        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var node = ((SoftwareCell)sender).DataContext as IOCellViewModel;
            var pos = node.Model.Position;
            pos.X += e.HorizontalChange;
            pos.Y += e.VerticalChange;
            node.Model.Position = pos;

//            Debug.WriteLine("onDragDelta");
//
//            _isDraggingMode = true;
//
//            var vm = (IOCellViewModel) DataContext;
//            Interactions.OnItemDragged(vm, e);
        }


        private new void PreviewMouseUp(object sender, MouseButtonEventArgs args)
        {
//            Debug.WriteLine("PreviewMouseUp");
//            if (!_isDraggingMode)
//            {
//                Interactions.ToggleSelection(GetDataContext());
//                if (args.Source.GetType() == typeof (SoftwareCell))
//                    Fu.FocusTextBox();
//                if (args.Source.GetType() == typeof (Stream))
//                {
//                    var flow = (Stream) args.Source;
//                    flow.FocusTextBox();
//                }
//                args.Handled = true;
//            }
        }


        private void Thumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
//            Debug.WriteLine("Thumb_OnDragStarted");
//            Interactions.DecideDragMode(GetDataContext());
        }


        private void Thumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
//            Debug.WriteLine("Thumb_OnDragCompleted");
//            _isDraggingMode = false;
//            e.Handled = true;
        }


        private IOCellViewModel GetDataContext()
        {
            var cellViewModel = DataContext as IOCellViewModel;
            return cellViewModel ?? null;
        }


        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }


        private new void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }


        private void updateConnectionViewModels()
        {
            var vm = GetDataContext();
            if (vm == null)
            {
                return;
            }
            var outputIDs =  vm.Model.OutputStreams.Select(x => x.ID).ToList();
            var outputs = MainViewModel.Instance().Connections.Where(x => outputIDs.Contains(x.ID)).ToList();

            outputs.ForEach(x =>
            {
                x.Start = new Point(vm.Model.Position.X + (ActualWidth - OutputFlow.ActualWidth), vm.Model.Position.Y + (ActualHeight / 2));
               
            });

            var inputIDs = vm.Model.InputStreams.Select(x => x.ID).ToList();
            var inputs = MainViewModel.Instance().Connections.Where(x => inputIDs.Contains(x.ID)).ToList();

            inputs.ForEach(x =>
            {
                x.End = new Point(vm.Model.Position.X + InputFlow.ActualWidth, vm.Model.Position.Y + ActualHeight / 2);
            });


        }

        private void Output_DragEnter(object sender, DragEventArgs e)
        {
            var vm =  GetDataContext();
            Interactions.AddNewConnectionNoDestination(vm);
            

        }

        private void NewOutput_click(object sender, RoutedEventArgs e)
        {
            Interactions.AddNewOutput(GetDataContext().Model.ID, "params", MainModel.Get());
            MainViewModel.Instance().LoadFromModel(MainModel.Get());
        }
    }
}