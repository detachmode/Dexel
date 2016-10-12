using System.Windows;
using FlowDesignModel;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign.Views
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

        private void MenuItem_GenerateCode(object sender, RoutedEventArgs e)
        {
            Interactions.ConsolePrintGeneratedCode(MainModel.Get());
        }
    }



}

