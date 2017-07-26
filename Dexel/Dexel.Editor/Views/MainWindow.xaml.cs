using System;
using System.Collections.ObjectModel;
using System.IO;
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

        
        public DrawingBoard DrawingBoard { get; set; }
        public MainViewModel CurrentlySelectedMainViewModel { get; set; }

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
                        DrawingBoard.AppendNewFunctionUnit();
                    }
                    else if (shiftDown)
                    {
                        DrawingBoard.TabStopMove(Interactions.TabStopGetPrevious);

                    }
                    else
                    {
                        DrawingBoard.TabStopMove(Interactions.TabStopGetNext);
                    }
                    e.Handled = true;
                    break;
                case Key.Return:
                    if (ctrlDown)
                    {
                        DrawingBoard.EnterShortcut(ctrlDown);
                        e.Handled = true;
                    }

                    break;
            }
        }

        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var addedItem in e.AddedItems)
            {
                CurrentlySelectedMainViewModel = addedItem as MainViewModel;
            }
            e.Handled = true;
        }

        private void TabControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            var test = DataContext;
            TabControl tabControl = sender as TabControl;
            ContentPresenter cp =
                tabControl.Template.FindName("PART_SelectedContentHost", tabControl) as ContentPresenter;

            var db = tabControl.ContentTemplate.FindName("TheDrawingBoard", cp) as DrawingBoard;
            DrawingBoard = db;
            e.Handled = true;
        }

        private void RenameMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var renamePopup = new RenameDiagramWindow();

            if (renamePopup.ShowDialog() == true)
            {
                CurrentlySelectedMainViewModel.Model.Name = renamePopup.NewDiagramName;
            }
        }
    }

}