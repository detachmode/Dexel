using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
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


        private void AddNewCell_Click(object sender, RoutedEventArgs e)
        {
            var positionClicked = GetClickedPosition();
            if (positionClicked == null) return;

            var newcellmodel = Interactions.AddNewIOCell(positionClicked.Value, ViewModel());
            FocusCell(newcellmodel);
        }


        private Point? GetClickedPosition()
        {
            var myWindow = GetWindow(this);
            var transform = myWindow?.TransformToVisual(TheDrawingBoard.SoftwareCellsList);
            var positionClicked = transform?.Transform(TheDrawingBoard.TheZoomBorder.BeforeContextMenuPoint);
            return positionClicked;
        }


        private void FocusCell(Model.DataTypes.SoftwareCell newcellmodel)
        {
            //MainViewModel.Instance().ClearSelection();
            //Keyboard.ClearFocus();
            //var t2 =  ((MainWindow)Application.Current.MainWindow).TheDrawingBoard.Focus();

            for (int i = 0; i < TheDrawingBoard.SoftwareCellsList.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)TheDrawingBoard.SoftwareCellsList.ItemContainerGenerator.ContainerFromIndex(i);
                c.ApplyTemplate();

                var item = c.ContentTemplate.FindName("IoCell", c) as IOCell;

                var vm = (item.DataContext as IOCellViewModel);

                if (vm.Model != newcellmodel)
                    continue;



                var t =  item.Fu.theTextBox.Focus();
                //Keyboard.Focus(item.Fu.theTextBox);

                vm.IsSelected = true;
            }
        }


        private void MenuItem_GenerateCode(object sender, RoutedEventArgs e)
        {
            Interactions.ConsolePrintGeneratedCode(ViewModel());
        }

        private void MenuItem_DebugPrint(object sender, RoutedEventArgs e)
        {
            Interactions.DebugPrint(ViewModel());
        }


        private MainModel ViewModel()
        {
            return (DataContext as MainViewModel).Model;
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrint(ViewModel(), Interactions.DebugPrint);
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoOutputTimerDispose();
        }

        private void AutoGenerate_Checked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrint(ViewModel(), Interactions.ConsolePrintGeneratedCode);
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
                Interactions.SaveToFile(saveFileDialog.FileName, ViewModel());
        }


        private void MenuItem_Load(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Interactions.LoadFromFile(openFileDialog.FileName, ViewModel());
        }

        private void MenuItem_Merge(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Interactions.MergeFromFile(openFileDialog.FileName, ViewModel());
        }


        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Interactions.Delete(MainViewModel.Instance().SelectedSoftwareCells.Select(x => x.Model), MainViewModel.Instance().Model);
            }
        }

        private void Paste_click(object sender, RoutedEventArgs e)
        {
            var positionClicked = GetClickedPosition();
            if (positionClicked == null) return;

            Interactions.Paste(positionClicked.Value, MainViewModel.Instance().Model);
        }
    }



}

