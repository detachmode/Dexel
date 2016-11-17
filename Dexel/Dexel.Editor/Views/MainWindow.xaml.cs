using System;
using System.CodeDom;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Dexel.Editor.CustomControls;
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
        private static MainWindow instance = null;
        public static MainWindow Get()
        {
            return instance;
        }

        public MainWindow(MainViewModel vm)
        {
            instance = this;
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

        private void FocusDataStream(DataStream dataStream)
        {
            MainViewModel.Instance().SelectedSoftwareCells.Clear();

            ConnectionsView frameworkelement = null;
            ConnectionViewModel viewmodel = null;
            for (int i = 0; i < TheDrawingBoard.ConnectionsList.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)TheDrawingBoard.ConnectionsList.ItemContainerGenerator.ContainerFromIndex(i);
                c.ApplyTemplate();

                frameworkelement = (ConnectionsView)c.ContentTemplate.FindName("TheConnectionsView", c);

                if (frameworkelement != null)
                {
                    viewmodel = (ConnectionViewModel)frameworkelement.DataContext;
                    if (viewmodel.Model == dataStream)
                    {
                        break;
                    }
                }
            } 

            if (viewmodel == null)
                return;

            frameworkelement.DataNamesControl.TextBox.Focus();

        }

        private void FocusCell(Model.DataTypes.SoftwareCell cellModel)
        {
            MainViewModel.Instance().SelectedSoftwareCells.Clear();

            IOCell frameworkelement = null;
            IOCellViewModel viewmodel = null;

            GetCell(cellModel, ref frameworkelement, ref viewmodel);

            if (viewmodel == null)
                return;

            frameworkelement.Fu.theTextBox.Focus();
            viewmodel.IsSelected = true;
        }


        private void FocusDefinition(Model.DataTypes.DataStreamDefinition dsd)
        {
            MainViewModel.Instance().SelectedSoftwareCells.Clear();
            IOCell frameworkelement = null;
            IOCellViewModel viewmodel = null;

            GetCell(dsd.Parent, ref frameworkelement, ref viewmodel);

            if (viewmodel == null)
                return;

            for (int i = 0; i < frameworkelement.OutputFlow.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter) frameworkelement.OutputFlow.ItemContainerGenerator.ContainerFromIndex(i);
                c.ApplyTemplate();

                var dsdView = (DangelingConnectionView) c.ContentTemplate.FindName("DangelingConnectionView", c);
                if (dsdView != null)
                {
                    var dsdViewModel = (DangelingConnectionViewModel)dsdView.DataContext;
                    if (dsdViewModel.Model == dsd)
                    {
                        dsdView.TheDataNamesControl.TextBox.Focus();
                        break;
                       
                    }
                }
            }
        }


        private void GetCell(Model.DataTypes.SoftwareCell newcellmodel, ref IOCell frameworkelement, ref IOCellViewModel viewmodel)
        {
            for (int i = 0; i < TheDrawingBoard.SoftwareCellsList.Items.Count; i++)
            {
                ContentPresenter c =
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


        public void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Delete)
            {
                Interactions.Delete(MainViewModel.Instance().SelectedSoftwareCells.Select(x => x.Model), MainViewModel.Instance().Model);
            }

            //if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            //{
            //    return;
            //}
            switch (e.Key)
            {
                case Key.Tab:

                    var focusedElement = Keyboard.FocusedElement;
                    TryGetDataContext<IOCellViewModel> (focusedElement, vm =>
                    {
                        var focusedcell = vm.Model;
                        var nextmodel = Interactions.TabStopGetNext(focusedcell, ViewModel());
                        SetFocusOnObject(nextmodel);
                    });

                    TryGetDataContext<DataStream>(focusedElement, focusedDataStream =>
                    {
                        var nextmodel =  Interactions.TabStopGetNext(focusedDataStream, ViewModel());
                        SetFocusOnObject(nextmodel);
                    });

                    TryGetDataContext<DataStreamDefinition>(focusedElement, vm =>
                    {
                        var nextmodel = Interactions.TabStopGetNext(vm, ViewModel());
                        SetFocusOnObject(nextmodel);
                    });


                    e.Handled = true;
                    break;
            }
        }


        private void SetFocusOnObject(object nextmodel)
        {
            nextmodel.TryCast<Model.DataTypes.SoftwareCell>(FocusCell);
            nextmodel.TryCast<Model.DataTypes.DataStream>(FocusDataStream);
            nextmodel.TryCast<Model.DataTypes.DataStreamDefinition>(FocusDefinition);
        }


        private static void TryGetDataContext<T>(object element, Action<T> doAction)
        {
            try
            {
                var frameworkelement = (FrameworkElement)element;
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
    }



}

