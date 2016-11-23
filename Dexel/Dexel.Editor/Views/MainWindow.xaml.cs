using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Dexel.Editor.CustomControls;
using Dexel.Editor.ViewModels;
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
                        AppendNewCell();
                    else if (shiftDown)
                        TabStopMove(Interactions.TabStopGetPrevious);
                    else
                        TabStopMove(Interactions.TabStopGetNext);
                    e.Handled = true;
                    break;
                case Key.Return:
                    NewOrFirstIntegrated();
                    e.Handled = true;
                    break;
            }
        }


        private void NewOrFirstIntegrated()
        {
            TryGetDataContext<IOCellViewModel>(Keyboard.FocusedElement, vm =>
            {
                var nextmodel = Interactions.NewOrFirstIntegrated(vm.Model, ViewModel());
                SetFocusOnObject(nextmodel);

            });
           
        }

        #region focus methods

        private void FocusDataStream(DataStream dataStream)
        {
            MainViewModel.Instance().SelectedSoftwareCells.Clear();

            ConnectionsView frameworkelement = null;
            ConnectionViewModel viewmodel = null;
            for (var i = 0; i < TheDrawingBoard.ConnectionsList.Items.Count; i++)
            {
                var c = (ContentPresenter) TheDrawingBoard.ConnectionsList.ItemContainerGenerator.ContainerFromIndex(i);
                c.ApplyTemplate();

                frameworkelement = (ConnectionsView) c.ContentTemplate.FindName("TheConnectionsView", c);

                if (frameworkelement == null) continue;

                viewmodel = (ConnectionViewModel) frameworkelement.DataContext;

                if (viewmodel.Model == dataStream)
                    break;
            }

            if (viewmodel == null)
                return;

            frameworkelement?.DataNamesControl.TextBox.Focus();
        }


        private void FocusCell(Model.DataTypes.SoftwareCell cellModel)
        {
            MainViewModel.Instance().SelectedSoftwareCells.Clear();

            IOCell frameworkelement = null;
            IOCellViewModel viewmodel = null;

            GetCell(cellModel, ref frameworkelement, ref viewmodel);

            if (viewmodel == null)
                return;

            Action a = () =>
            {
                frameworkelement.Fu.theTextBox.Focus();
                frameworkelement.Fu.theTextBox.SelectionStart = frameworkelement.Fu.theTextBox.Text.Length;
            };
            frameworkelement.Fu.theTextBox.Dispatcher.BeginInvoke(DispatcherPriority.Background, a);

            viewmodel.IsSelected = true;
        }


        private void FocusDefinition(DataStreamDefinition dsd)
        {
            MainViewModel.Instance().SelectedSoftwareCells.Clear();
            IOCell frameworkelement = null;
            IOCellViewModel viewmodel = null;

            GetCell(dsd.Parent, ref frameworkelement, ref viewmodel);

            if (viewmodel == null)
                return;

            FocusTextbox(dsd, frameworkelement.OutputFlow);
            FocusTextbox(dsd, frameworkelement.InputFlow);
        }


        private static void FocusTextbox(DataStreamDefinition dsd, ItemsControl itemsControl)
        {
            for (var i = 0; i < itemsControl.Items.Count; i++)
            {
                var c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                c.ApplyTemplate();

                var dsdView = (DangelingConnectionView) c.ContentTemplate.FindName("DangelingConnectionView", c);
                if (dsdView == null) continue;
                var dsdViewModel = (DangelingConnectionViewModel) dsdView.DataContext;
                if (dsdViewModel.Model != dsd) continue;

                dsdView.TheDataNamesControl.TextBox.Focus();
                dsdView.TheDataNamesControl.TextBox.SelectionStart = dsdView.TheDataNamesControl.TextBox.Text.First()
                    .Equals('(')
                    ? 1
                    : 0;
                break;
            }
        }


        private void GetCell(Model.DataTypes.SoftwareCell newcellmodel, ref IOCell frameworkelement,
            ref IOCellViewModel viewmodel)
        {
            for (var i = 0; i < TheDrawingBoard.SoftwareCellsList.Items.Count; i++)
            {
                var c =
                    (ContentPresenter) TheDrawingBoard.SoftwareCellsList.ItemContainerGenerator.ContainerFromIndex(i);
                c.ApplyTemplate();

                frameworkelement = (IOCell) c.ContentTemplate.FindName("IoCell", c);

                if (frameworkelement != null)
                {
                    viewmodel = (IOCellViewModel) frameworkelement.DataContext;
                    if (viewmodel.Model == newcellmodel)
                    {
                        break;
                    }
                }
            }
        }

        #endregion



        private void MenuItem_GenerateCode(object sender, RoutedEventArgs e)
        {
            Interactions.ConsolePrintGeneratedCode(ViewModel());
        }


        private void MenuItem_DebugPrint(object sender, RoutedEventArgs e)
        {
            Interactions.DebugPrint(ViewModel());
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



        private void AppendNewCell()
        {
            TryGetDataContext<IOCellViewModel>(Keyboard.FocusedElement, vm
                => AppendNewCellBehind(vm, vm.DangelingOutputs.First()));

            TryGetDataContext<DataStreamDefinition>(Keyboard.FocusedElement, dsd
                =>
            {
                var cellVM = MainViewModel.Instance().SoftwareCells.First(iocell => iocell.Model == dsd.Parent);
                var vm = cellVM.DangelingOutputs.First(dsdVM => dsdVM.Model == dsd);
                AppendNewCellBehind(cellVM, vm);
            });
        }


        private void AppendNewCellBehind(IOCellViewModel vm, DangelingConnectionViewModel dangelingConnectionViewModel)
        {
            var width = dangelingConnectionViewModel.Width;
            width += vm.CellWidth/2 + 100;
            var focusedcell = vm.Model;
            var nextmodel = Interactions.AppendNewCell(focusedcell, width, dangelingConnectionViewModel.Model,
                ViewModel());

            SetFocusOnObject(nextmodel);
        }


        private void TabStopMove(Func<object, MainModel, object> tabstopFunc)
        {
            var focusedElement = Keyboard.FocusedElement;
            TryGetDataContext<IOCellViewModel>(focusedElement, vm =>
            {
                var focusedcell = vm.Model;
                var nextmodel = tabstopFunc(focusedcell, ViewModel());
                SetFocusOnObject(nextmodel);
            });

            TryGetDataContext<DataStream>(focusedElement, focusedDataStream =>
            {
                var nextmodel = tabstopFunc(focusedDataStream, ViewModel());
                SetFocusOnObject(nextmodel);
            });

            TryGetDataContext<DataStreamDefinition>(focusedElement, vm =>
            {
                var nextmodel = tabstopFunc(vm, ViewModel());
                SetFocusOnObject(nextmodel);
            });
        }


        private void SetFocusOnObject(object model)
        {
            model.TryCast<Model.DataTypes.SoftwareCell>(FocusCell);
            model.TryCast<DataStream>(FocusDataStream);
            model.TryCast<DataStreamDefinition>(FocusDefinition);
        }


        private static void TryGetDataContext<T>(object element, Action<T> doAction)
        {
            try
            {
                var frameworkelement = (FrameworkElement) element;
                var vm = (T) frameworkelement.DataContext;
                doAction(vm);
            }
            catch
            {
                // ignored
            }
        }


        private void Paste_click(object sender, RoutedEventArgs e)
        {
            var positionClicked = GetClickedPosition();
            if (positionClicked == null) return;

            Interactions.Paste(positionClicked.Value, MainViewModel.Instance().Model);
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
    }

}