using System.Windows;
using Dexel.Contracts.Model;
using Dexel.Editor.ViewModels;
using Dexel.Model;

namespace Dexel.Editor.Views
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var myWindow = GetWindow(this);
            var transform = myWindow?.TransformToVisual(TheDrawingBoard.ItemContainer);
            if (transform == null) return;
            var myUiElementPosition = transform.Transform(TheZoomBorder.BeforeContextMenuPoint);

            Interactions.AddNewIOCell(myUiElementPosition, getModelFromDataContext());
        }

        private void MenuItem_GenerateCode(object sender, RoutedEventArgs e)
        {
            Interactions.ConsolePrintGeneratedCode(getModelFromDataContext());
        }

        private void MenuItem_DebugPrint(object sender, RoutedEventArgs e)
        {
            Interactions.DebugPrint(getModelFromDataContext());
        }


        private IMainModel getModelFromDataContext()
        {
            return (DataContext as MainViewModel).Model;
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrint(getModelFromDataContext(), Interactions.DebugPrint);
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrintOff();
        }

        private void AutoGenerate_Checked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrint(getModelFromDataContext(), Interactions.ConsolePrintGeneratedCode);
        }

        private void AutoGenerate_UnChecked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrintOff();
        }
    }



}

