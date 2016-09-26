using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign.Views
{
    /// <summary>
    /// Interaction logic for IOCell.xaml
    /// </summary>
    public partial class IOCell : UserControl
    {
        private static bool IsDraggingMode = false;

        public IOCell()
        {
            InitializeComponent();
;
        }



        // Event hanlder for dragging functionality support same to all thumbs
        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            Debug.WriteLine("onDragDelta");

            IOCell.IsDraggingMode = true;

            var vm = (IOCellViewModel)DataContext;
            Interactions.OnItemDragged(vm, e);

        }




        private void PreviewMouseUp(object sender, MouseButtonEventArgs args)
        {
            Debug.WriteLine("PreviewMouseUp");
            if (!IOCell.IsDraggingMode)
            {              
               Interactions.ToggleSelection(GetDataContext());
               if (args.Source.GetType() == typeof(FunctionUnit))
                    FU.FocusTextBox();
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
            IOCell.IsDraggingMode = false;
            e.Handled = true;
        }




        private IOCellViewModel GetDataContext()
        {

            var dc = (IOCellViewModel)this.DataContext;
            return dc;
        }


        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }
    }
}
