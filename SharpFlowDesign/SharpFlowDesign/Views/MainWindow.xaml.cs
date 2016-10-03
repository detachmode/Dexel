using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Petzold.Media2D;
using SharpFlowDesign.Model;
using SharpFlowDesign.ViewModels;
using SharpFlowDesign.Views;

namespace SharpFlowDesign
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = MainViewModel.Instance();
            //Interactions.SetViewModel((MainViewModel)DataContext);

        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var myWindow = GetWindow(this);
            var transform = myWindow?.TransformToVisual(TheDrawingBoard.ItemContainer);
            if (transform == null) return;
            var myUiElementPosition = transform.Transform(TheZoomBorder.BeforeContextMenuPoint);

            Interactions.AddNewIOCell(myUiElementPosition);
        }
    }



}

