using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign.UserControls
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




        private void PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("PreviewMouseUp");
            if (!IOCell.IsDraggingMode)
            {              
                Interactions.ToggleSelection(GetDataContext());
                FU.FocusTextBox();

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
    }
}
