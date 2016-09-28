using System.Diagnostics;
using System.Windows;
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
        }


        // Event hanlder for dragging functionality support same to all thumbs
        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            Debug.WriteLine("onDragDelta");

            _isDraggingMode = true;

            var vm = (IOCellViewModel) DataContext;
            Interactions.OnItemDragged(vm, e);
        }


        private new void PreviewMouseUp(object sender, MouseButtonEventArgs args)
        {
            Debug.WriteLine("PreviewMouseUp");
            if (!_isDraggingMode)
            {
                Interactions.ToggleSelection(GetDataContext());
                if (args.Source.GetType() == typeof(FunctionUnit))
                    Fu.FocusTextBox();
                if (args.Source.GetType() == typeof(Flow))
                {
                    var flow = (Flow) args.Source;
                    flow.FocusTextBox();
                }
                args.Handled = true;
            }
        }


        private void Thumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            Debug.WriteLine("Thumb_OnDragStarted");
            Interactions.DecideDragMode(GetDataContext());
        }


        private void Thumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            Debug.WriteLine("Thumb_OnDragCompleted");
            _isDraggingMode = false;
            e.Handled = true;
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
            ((IOCellViewModel) DataContext).InputPoint = 
                new Point(InputFlow.ActualWidth,ActualHeight/2);
            ((IOCellViewModel) DataContext).OutputPoint =
                new Point(ActualWidth - OutputFlow.ActualWidth, ActualHeight / 2);
        }
    }

}