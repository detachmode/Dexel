using System.IO;
using System.Windows;
using Dexel.Editor.ViewModels;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Microsoft.Win32;

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
            var transform = myWindow?.TransformToVisual(TheDrawingBoard.SoftwareCellsList);
            if (transform == null) return;
           var myUiElementPosition = transform.Transform(TheDrawingBoard.TheZoomBorder.BeforeContextMenuPoint);

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


        private MainModel getModelFromDataContext()
        {
            return (DataContext as MainViewModel).Model;
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrint(getModelFromDataContext(), Interactions.DebugPrint);
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoOutputTimerDispose();
        }

        private void AutoGenerate_Checked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrint(getModelFromDataContext(), Interactions.ConsolePrintGeneratedCode);
        }

        private void AutoGenerate_UnChecked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoOutputTimerDispose();
        }


        private void MenuItem_Save(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
                Interactions.SaveToFile(saveFileDialog.FileName, getModelFromDataContext());
        }


        private void MenuItem_Load(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Interactions.LoadFromFile(openFileDialog.FileName, getModelFromDataContext());
        }

        private void MenuItem_Merge(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Interactions.MergeFromFile(openFileDialog.FileName, getModelFromDataContext());
        }
    }



}

