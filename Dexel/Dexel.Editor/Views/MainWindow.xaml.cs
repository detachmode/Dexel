using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Dexel.Editor.CustomControls;
using Dexel.Editor.ViewModels;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Microsoft.Win32;

namespace Dexel.Editor.Views
{

    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static MainWindow _instance;


        public MainWindow(MainViewModel vm)
        {
            _instance = this;
            InitializeComponent();
            DataContext = vm;
        }

        public static MainWindow Get() => _instance;


        private MainModel ViewModel() => (DataContext as MainViewModel)?.Model;


        public void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            var ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (e.Key == Key.Delete)
            {
                Interactions.Delete(MainViewModel.Instance().SelectedSoftwareCells.Select(x => x.Model),
                    MainViewModel.Instance().Model);
            }

            switch (e.Key)
            {
                case Key.Tab:
                    if (ctrlDown)
                        TheDrawingBoard.AppendNewCell();
                    else if (shiftDown)
                        TheDrawingBoard.TabStopMove(Interactions.TabStopGetPrevious);
                    else
                        TheDrawingBoard.TabStopMove(Interactions.TabStopGetNext);
                    e.Handled = true;
                    break;
                case Key.Return:
                    TheDrawingBoard.NewOrFirstIntegrated();
                    e.Handled = true;
                    break;
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



       





        private void help_OnClicked(object sender, RoutedEventArgs e)
        {
            var helpdia = new HelpWindow();
            helpdia.ShowDialog();
        }

        private void MenuItem_New(object sender, RoutedEventArgs e)
        {         
             MainViewModel.Instance().LoadFromModel(new MainModel());
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Interactions.AddMissingDataTypes(MainViewModel.Instance().Model);

        }
    }

}