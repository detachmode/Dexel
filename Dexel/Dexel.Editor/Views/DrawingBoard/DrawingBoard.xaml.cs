using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Dexel.Editor.CustomControls;
using Dexel.Editor.DragAndDrop;
using Dexel.Editor.ViewModels;
using Dexel.Editor.ViewModels.DrawingBoard;
using Dexel.Model.DataTypes;

namespace Dexel.Editor.Views.DrawingBoard
{
    /// <summary>
    /// Interaktionslogik für DrawingBoard.xaml
    /// </summary>
    public partial class DrawingBoard : UserControl
    {
        public DrawingBoard()
        {
            InitializeComponent();



        }

        private MainViewModel ViewModel => (MainViewModel)DataContext;
        private MainModel ViewModelModel() => (DataContext as MainViewModel)?.Model;


        #region IOCell mouse events

        private void IOCell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseEventMediator.OrigMouseDownPoint = GetAbsoluteMousePosition(e.GetPosition(this));
            MouseEventMediator.MouseDown(sender, e);
        }




        private void IOCell_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseEventMediator.MouseUp(sender, e);

        }


        private void IOCell_MouseMove(object sender, MouseEventArgs e)
        {
            MouseEventMediator.ProjectedMousePosition = GetAbsoluteMousePosition(e.GetPosition(this));
            MouseEventMediator.ScreenMousePosition = e.GetPosition(this);
            MouseEventMediator.MouseMove(sender, e);
        }

        #endregion

        #region window mouse events

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseEventMediator.OrigMouseDownPoint = e.GetPosition(this);
            MouseEventMediator.MouseDown(sender, e);

        }


        public Point GetAbsoluteMousePosition(Point pt)
        {
            var selectionRectangleProjection = TheCanvas.TransformToVisual(GridInsideZoomBorder);
            return selectionRectangleProjection.Transform(pt);

        }


        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {

            MouseEventMediator.MouseUp(sender, e);
        }


        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            MouseEventMediator.ProjectedMousePosition = GetAbsoluteMousePosition(e.GetPosition(this));
            MouseEventMediator.ScreenMousePosition = e.GetPosition(this);
            MouseEventMediator.MouseMove(sender, e);
        }

        #endregion

        public void InitDragSelectionRect(Point pt1, Point pt2)
        {
            UpdateDragSelectionRect(pt1, pt2);
            DragSelectionCanvas.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Update the position and size of the rectangle used for drag selection.
        /// </summary>
        public void UpdateDragSelectionRect(Point pt1, Point pt2)
        {
            double x, y, width, height;
            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            Canvas.SetLeft(DragSelectionBorder, x);
            Canvas.SetTop(DragSelectionBorder, y);
            DragSelectionBorder.Width = width;
            DragSelectionBorder.Height = height;
        }

        /// <summary>
        /// Select all nodes that are in the drag selection rectangle.
        /// </summary>
        public void ApplyDragSelectionRect()
        {
            //var transform = myWindow?.TransformToVisual(TheDrawingBoard.SoftwareCellsList);
            //if (transform == null) return;
            //var myUiElementPosition = transform.Transform(TheZoomBorder.BeforeContextMenuPoint);

            DragSelectionCanvas.Visibility = Visibility.Collapsed;
            var selectionRectangleProjection = TheCanvas.TransformToVisual(GridInsideZoomBorder);

            var rect = new Rect(
                new Point(Canvas.GetLeft(DragSelectionBorder), Canvas.GetTop(DragSelectionBorder)),
                new Size(DragSelectionBorder.Width, DragSelectionBorder.Height));
            var newBounds = selectionRectangleProjection.TransformBounds(rect);
            Rect dragRect = newBounds;

            //
            // Inflate the drag selection-rectangle by 1/10 of its size to 
            // make sure the intended item is selected.
            //
            dragRect.Inflate(rect.Width / 10, rect.Height / 10);

            MainViewModel.Instance().ClearSelection();


            foreach (IOCellViewModel IOCellViewModel in this.ViewModel.SoftwareCells)
            {
                Rect itemRect = new Rect(IOCellViewModel.Model.Position.X, IOCellViewModel.Model.Position.Y, IOCellViewModel.CellWidth, IOCellViewModel.CellHeight);
                if (dragRect.Contains(itemRect))
                {
                    MainViewModel.Instance().AddToSelection(IOCellViewModel);
                }
            }
        }


        #region focus methods

        private void FocusDataStream(DataStream dataStream)
        {
            MainViewModel.Instance().SelectedSoftwareCells.Clear();

            ConnectionsView frameworkelement = null;
            ConnectionViewModel viewmodel = null;

            for (var i = 0; i < ConnectionsList.Items.Count; i++)
            {
                var c = (ContentPresenter)ConnectionsList.ItemContainerGenerator.ContainerFromIndex(i);
                c.ApplyTemplate();

                frameworkelement = (ConnectionsView)c.ContentTemplate.FindName("TheConnectionsView", c);
                if (frameworkelement == null) continue;
                viewmodel = (ConnectionViewModel)frameworkelement.DataContext;
                if (viewmodel.Model == dataStream)
                    break;
            }

            if (viewmodel == null)
                return;


            frameworkelement?.DataNamesControl.TextBox.Focus();
            frameworkelement.DataNamesControl.TextBox.SelectionStart = frameworkelement.DataNamesControl.TextBox.Text.First()
                    .Equals('(')
                    ? 1
                    : 0;
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

                var dsdView = (DangelingConnectionView)c.ContentTemplate.FindName("DangelingConnectionView", c);
                if (dsdView == null) continue;
                var dsdViewModel = (DangelingConnectionViewModel)dsdView.DataContext;
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
            for (var i = 0; i < SoftwareCellsList.Items.Count; i++)
            {
                var c =
                    (ContentPresenter)SoftwareCellsList.ItemContainerGenerator.ContainerFromIndex(i);
                c.ApplyTemplate();

                frameworkelement = (IOCell)c.ContentTemplate.FindName("IoCell", c);

                if (frameworkelement != null)
                {
                    viewmodel = (IOCellViewModel)frameworkelement.DataContext;
                    if (viewmodel.Model == newcellmodel)
                    {
                        break;
                    }
                }
            }
        }

        #endregion



        private void AddNewCell_Click(object sender, RoutedEventArgs e)
        {
            var positionClicked = GetClickedPosition();
            if (positionClicked == null) return;

            var newcellmodel = Interactions.AddNewIOCell(positionClicked.Value, ViewModelModel());
            FocusCell(newcellmodel);
        }

        private void Paste_click(object sender, RoutedEventArgs e)
        {
            var positionClicked = GetClickedPosition();
            if (positionClicked == null) return;

            Interactions.Paste(positionClicked.Value, MainViewModel.Instance().Model);
        }


        private Point? GetClickedPosition()
        {
            var transform = this.TransformToVisual(SoftwareCellsList);
            var positionClicked = transform?.Transform(TheZoomBorder.BeforeContextMenuPoint);
            return positionClicked;
        }


        public void AppendNewCell()
        {
            Keyboard.FocusedElement.TryGetDataContext<IOCellViewModel>(vm =>
                {
                    var unnconnected = vm.Outputs.First(x => !x.Model.Connected);
                    if (unnconnected != null)
                        AppendNewCellBehind(vm, (DangelingConnectionViewModel) unnconnected);
                }
            );

            Keyboard.FocusedElement.TryGetDataContext<DataStreamDefinition>(dsd =>
            {
                var cellVM = MainViewModel.Instance().SoftwareCells.First(iocell => iocell.Model == dsd.Parent);
                var vm = cellVM.Outputs.First(dsdVM => dsdVM.Model == dsd);
                AppendNewCellBehind(cellVM, (DangelingConnectionViewModel)vm);
            });
        }


        private void AppendNewCellBehind(IOCellViewModel vm, DangelingConnectionViewModel dangelingConnectionViewModel)
        {
            var width = dangelingConnectionViewModel.Width;
            width += vm.CellWidth / 2 + 100;
            var focusedcell = vm.Model;
            var nextmodel = Interactions.AppendNewCell(focusedcell, width, dangelingConnectionViewModel.Model,
                ViewModelModel());

            SetFocusOnObject(nextmodel);
        }


        public void TabStopMove(Func<object, MainModel, object> tabstopFunc)
        {
            var focusedelement = Keyboard.FocusedElement;
            focusedelement.TryGetDataContext<IOCellViewModel>(vm =>
            {
                var focusedcell = vm.Model;
                var nextmodel = tabstopFunc(focusedcell, ViewModelModel());
                SetFocusOnObject(nextmodel);
            });

            focusedelement.TryGetDataContext<DataStream>(focusedDataStream =>
            {
                var nextmodel = tabstopFunc(focusedDataStream, ViewModelModel());
                SetFocusOnObject(nextmodel);
            });

            focusedelement.TryGetDataContext<DataStreamDefinition>(vm =>
            {
                var nextmodel = tabstopFunc(vm, ViewModelModel());
                SetFocusOnObject(nextmodel);
            });
        }


        public void NewOrFirstIntegrated()
        {
            Keyboard.FocusedElement.TryGetDataContext<IOCellViewModel>(vm =>
            {
                var nextmodel = Interactions.NewOrFirstIntegrated(vm.Model, ViewModelModel());
                SetFocusOnObject(nextmodel);

            });

        }


        private void SetFocusOnObject(object model)
        {
            model.TryCast<Model.DataTypes.SoftwareCell>(FocusCell);
            model.TryCast<DataStream>(FocusDataStream);
            model.TryCast<DataStreamDefinition>(FocusDefinition);
        }
    }
}
