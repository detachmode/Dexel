using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpFlowDesign.CustomControls;
using SharpFlowDesign.Model;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign.Views
{
    /// <summary>
    /// Interaktionslogik für DrawingBoard.xaml
    /// </summary>
    public partial class DrawingBoard : UserControl
    {
        public DrawingBoard()
        {
            InitializeComponent();
            
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("UIElement_OnMouseUp");
            Keyboard.ClearFocus();

            //Interactions.DeselectAll();

        }

        private void MainWindow_OnDragOver(object sender, DragEventArgs e)
        {
            SetPointerToMousePosition(e);
        }

       

        private void SetPointerToMousePosition(DragEventArgs e)
        {
            Point p2 = e.GetPosition(this);
            var obj = e.Data.GetData(e.Data.GetFormats()[0]);
            if (!(obj is Pointer)) return;

            var pointer = (obj as Pointer);
            var datacontext = (Connection)pointer.DataContext;

            pointer.End = p2;
        }

    }
}
