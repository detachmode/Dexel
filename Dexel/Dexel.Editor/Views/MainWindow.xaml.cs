using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views.AdditionalWindows;
using Dexel.Model.DataTypes;
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

        public MainWindow(MainViewModel vm)
        {
            _instance = this;
            InitializeComponent();
            DataContext = vm;
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
                Interactions.LoadFromFile(files[0], MainViewModel.Instance().Model);
            }
        }

        public static MainWindow Get() => _instance;


        private MainModel MainModel() => (DataContext as MainViewModel)?.Model;


        public void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            var ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (e.Key == Key.Delete)
            {
                Interactions.Delete(MainViewModel.Instance().SelectedFunctionUnits.Select(x => x.Model),
                    MainViewModel.Instance().Model);
            }

            switch (e.Key)
            {
                case Key.Tab:
                    if (ctrlDown)
                        TheDrawingBoard.AppendNewFunctionUnit();
                    else if (shiftDown)
                        TheDrawingBoard.TabStopMove(Interactions.TabStopGetPrevious);
                    else
                        TheDrawingBoard.TabStopMove(Interactions.TabStopGetNext);
                    e.Handled = true;
                    break;
                case Key.Return:
                    if (ctrlDown)
                    {
                        TheDrawingBoard.EnterShortcut(ctrlDown);
                        e.Handled = true;
                    }
                        
                    break;
            }
        }

        private void MenuItem_GenerateCodeToDesktop(object sender, RoutedEventArgs e)
        {
            Interactions.GeneratedCodeToDesktop(sleepBeforeErrorPrint:true, mainModel:MainModel());
        }


        private void MenuItem_DebugPrint(object sender, RoutedEventArgs e)
        {
            Interactions.DebugPrint(MainModel());
        }



        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrint(MainModel(), Interactions.DebugPrint);
        }


        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoOutputTimerDispose();
        }


        private void AutoGenerate_Checked(object sender, RoutedEventArgs e)
        {
            Interactions.AutoPrint(MainModel(), mainModel => 
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
                Interactions.SaveToFile(saveFileDialog.FileName, MainModel());
        }


        private void MenuItem_Load(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Interactions.LoadFromFile(openFileDialog.FileName, MainModel());
        }

        private void MenuItem_LoadFromCSharp(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "C# (*.cs)|*.cs|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Interactions.LoadFromCSharp(openFileDialog.FileName);
        }


        private void MenuItem_Merge(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Interactions.MergeFromFile(openFileDialog.FileName, MainModel());
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

        private void MenuItem_GenerateCodeToClipboard(object sender, RoutedEventArgs e)
        {
            Interactions.GenerateCodeToClipboard(MainViewModel.Instance().Model);
        }


        private void MenuItem_ResetView(object sender, RoutedEventArgs e)
        {
            TheDrawingBoard.ResetView();
        }


        private void MenuItem_GenerateCodeToConsole(object sender, RoutedEventArgs e)
        {
            Interactions.GenerateCodeToConsole(MainModel());
        }
        private void MenuItem_DarkTheme(object sender, RoutedEventArgs e)
        {
            Interactions.ChangeToDarkTheme();
        }

        private void MenuItem_PrintTheme(object sender, RoutedEventArgs e)
        {
            Interactions.ChangeToPrintTheme();
          
        }

        private void uiSketch_OnClicked(object sender, RoutedEventArgs e)
        {
            var uisketch= new UI_Sketches.TemporaryTestWindow();
            uisketch.Show();
        }
    }

}