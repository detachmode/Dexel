using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views.AdditionalWindows;
using Dexel.Editor.Views.UserControls.DrawingBoard;
using Dexel.Model.DataTypes;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace Dexel.Editor.Views
{
    /// <summary>
    /// Interaktionslogik für DexelWindow.xaml
    /// </summary>
    public partial class DexelWindow : Window
    {
        public DexelWindow()
        {
            InitializeComponent();
        }

        public DrawingBoard DrawingBoard { get; set; }
        private MainViewModel CurrentlySelectedMainViewModel { get; set; }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                var viewModel = (MainViewModel)DataContext;
                Interactions.LoadFromFile(viewModel, files[0]);
            }
        }
        

        private void MenuItem_GenerateCodeToDesktop(object sender, RoutedEventArgs e)
        {
            Interactions.GeneratedCodeToDesktop(sleepBeforeErrorPrint: true, mainModel: CurrentlySelectedMainViewModel.Model);
        }


        private void MenuItem_DebugPrint(object sender, RoutedEventArgs e)
        {
            Interactions.DebugPrint(CurrentlySelectedMainViewModel.Model);
        }



        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrint(CurrentlySelectedMainViewModel.Model, Interactions.DebugPrint);
        }


        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoOutputTimerDispose();
        }


        private void AutoGenerate_Checked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrint(CurrentlySelectedMainViewModel.Model, mainModel =>
                Interactions.GeneratedCodeToDesktop(sleepBeforeErrorPrint: false, mainModel: mainModel));
        }


        private void AutoGenerate_UnChecked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoOutputTimerDispose();
        }


        private void MenuItem_Save(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                CurrentlySelectedMainViewModel.Model.Name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                Interactions.SaveToFile(saveFileDialog.FileName, CurrentlySelectedMainViewModel.Model);
            }
        }


        private void MenuItem_Load(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
                Interactions.LoadFromFile(CurrentlySelectedMainViewModel, openFileDialog.FileName);
        }

        private void MenuItem_LoadFromCSharp(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "C# (*.cs)|*.cs|All Files (*.*)|*.*" };
            if (openFileDialog.ShowDialog() == true)
                Interactions.LoadFromCSharp((MainViewModel)DataContext, openFileDialog.FileName);
        }


        private void MenuItem_Merge(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
                Interactions.MergeFromFile((MainViewModel)DataContext, openFileDialog.FileName, CurrentlySelectedMainViewModel.Model);
        }

        private void help_OnClicked(object sender, RoutedEventArgs e)
        {
            var helpdia = new HelpWindow();
            helpdia.ShowDialog();
        }

        private void MenuItem_New(object sender, RoutedEventArgs e)
        {
            var mainViewModel = (MainViewModel)DataContext;
            mainViewModel.LoadFromModel(new MainModel());
        }

        private void MenuItem_GenerateCodeToClipboard(object sender, RoutedEventArgs e)
        {
            var mainViewModel = (MainViewModel)DataContext;
            Interactions.GenerateCodeToClipboard(mainViewModel.Model);
        }


        private void MenuItem_ResetView(object sender, RoutedEventArgs e)
        {
            MainWindow.DrawingBoard.ResetView();
        }


        private void MenuItem_GenerateCodeToConsole(object sender, RoutedEventArgs e)
        {
            Interactions.GenerateCodeToConsole(CurrentlySelectedMainViewModel.Model);
        }
        private void MenuItem_DarkTheme(object sender, RoutedEventArgs e)
        {
            var mainViewModel = (MainViewModel)DataContext;
            Interactions.ChangeToDarkTheme(mainViewModel);
        }

        private void MenuItem_PrintTheme(object sender, RoutedEventArgs e)
        {
            var mainViewModel = (MainViewModel)DataContext;
            Interactions.ChangeToPrintTheme(mainViewModel);

        }

        private void uiSketch_OnClicked(object sender, RoutedEventArgs e)
        {
            var uisketch = new UI_Sketches.TemporaryTestWindow();
            uisketch.Show();
        }
    }
}

