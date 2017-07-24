using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views.AdditionalWindows;
using Dexel.Editor.Views.Common;
using Dexel.Editor.Views.UserControls.DrawingBoard;
using Dexel.Model.DataTypes;
using Dexel.Model.Mockdata;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;

namespace Dexel.Editor.Views
{

    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static MainWindow _instance;

        public static string SyntaxColortheme = @"Views/Themes/FlowDesignColorDark.xshd";
        public static XshdSyntaxDefinition Xshd;
        public DrawingBoard CurrentlySelectedDrawingBoard { get; set; }
        private MainViewModel CurrentlySelectedMainViewModel { get; set; }

        public MainWindow()
        {
            _instance = this;
            InitializeComponent();
            Interactions.StartAutoSave();
        }


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

        public static MainWindow Get() => _instance;

        public void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            var ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (e.Key == Key.Delete)
            {
                var viewModel = (MainViewModel)DataContext;
                Interactions.Delete(viewModel, viewModel.SelectedFunctionUnits.Select(x => x.Model));
            }

            switch (e.Key)
            {
                case Key.Tab:
                    if (ctrlDown)
                    {
                        CurrentlySelectedDrawingBoard.AppendNewFunctionUnit();
                    }
                    else if (shiftDown)
                    {
                        CurrentlySelectedDrawingBoard.TabStopMove(Interactions.TabStopGetPrevious);

                    }
                    else
                    {
                         CurrentlySelectedDrawingBoard.TabStopMove(Interactions.TabStopGetNext);
                    }
                    e.Handled = true;
                    break;
                case Key.Return:
                    if (ctrlDown)
                    {
                        CurrentlySelectedDrawingBoard.EnterShortcut(ctrlDown);
                        e.Handled = true;
                    }

                    break;
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
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
                Interactions.SaveToFile(saveFileDialog.FileName, CurrentlySelectedMainViewModel.Model);
        }


        private void MenuItem_Load(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Interactions.LoadFromFile(CurrentlySelectedMainViewModel, openFileDialog.FileName);
        }

        private void MenuItem_LoadFromCSharp(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "C# (*.cs)|*.cs|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Interactions.LoadFromCSharp((MainViewModel)DataContext, openFileDialog.FileName);
        }


        private void MenuItem_Merge(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*";
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
            CurrentlySelectedDrawingBoard.ResetView();
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
        
        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            if (TabControl.SelectedContent == null) return;
            foreach (var addedItem in e.AddedItems)
            {
                    CurrentlySelectedMainViewModel = addedItem as MainViewModel;
            }
            e.Handled = true;
        }

        private void TabControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            ContentPresenter cp =
                tabControl.Template.FindName("PART_SelectedContentHost", tabControl) as ContentPresenter;

            var db = tabControl.ContentTemplate.FindName("TheDrawingBoard", cp) as DrawingBoard;
            CurrentlySelectedDrawingBoard = db;
        }
    }

}