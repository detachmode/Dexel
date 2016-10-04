using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
            updateViewModel();
        }





        // Event hanlder for dragging functionality support same to all thumbs
        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var node = ((SoftwareCell)sender).DataContext as IOCellViewModel;
            var pos = node.Position;
            pos.X += e.HorizontalChange;
            pos.Y += e.VerticalChange;
            node.Position = pos;

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
            var dc = (IOCellViewModel) DataContext;
            return dc;
        }


        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }


        private new void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }


        private void updateViewModel()
        {
            var vm = GetDataContext();
            vm.InputPoint = new Point(vm.Position.X + InputFlow.ActualWidth, vm.Position.Y + ActualHeight/2);
            vm.OutputPoint = new Point(vm.Position.X + (ActualWidth - OutputFlow.ActualWidth), vm.Position.Y + (ActualHeight /2));
        }

        private void Output_DragEnter(object sender, DragEventArgs e)
        {
            var vm =  GetDataContext();
            Interactions.AddNewConnectionNoDestination(vm);
            

        }

        private void NewOutput_click(object sender, RoutedEventArgs e)
        {
            GetDataContext().DangelingOutputs.Add(new DangelingConnectionViewModel(GetDataContext()));

        }
    }
}