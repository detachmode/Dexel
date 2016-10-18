using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dexel.Editor.CustomControls;
using Dexel.Editor.ViewModels;

namespace Dexel.Editor.Views
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
            var datacontext = (ConnectionViewModel)pointer.DataContext;

            pointer.End = p2;
        }

    }
}
