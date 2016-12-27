using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Dexel.Editor.ViewModels;
using Dexel.Editor.ViewModels.DrawingBoard;
using Dexel.Editor.Views.Common;
using Dexel.Editor.Views.CustomControls;
using Dexel.Library;
using Dexel.Model.DataTypes;

namespace Dexel.Editor.Views.UserControls.DrawingBoard
{
    /// <summary>
    /// Interaktionslogik für DrawingBoard.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class DrawingBoard : UserControl
    {
        public DrawingBoard()
        {
            InitializeComponent();



        }

        private MainViewModel ViewModel => (MainViewModel)DataContext;
        private MainModel ViewModelModel() => (DataContext as MainViewModel)?.Model;


        #region FunctionUnitView mouse events

        private void FunctionUnit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseEventMediator.OrigMouseDownPoint = GetAbsoluteMousePosition(e.GetPosition(this));
            MouseEventMediator.MouseDown(sender, e);
        }




        private void FunctionUnit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseEventMediator.MouseUp(sender, e);

        }


        private void FunctionUnit_MouseMove(object sender, MouseEventArgs e)
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
            //var transform = myWindow?.TransformToVisual(TheDrawingBoard.FunctionUnitsList);
            //if (transform == null) return;
            //var myUiElementPosition = transform.Transform(TheZoomBorder.BeforeContextMenuPoint);

            DragSelectionCanvas.Visibility = Visibility.Collapsed;
            var selectionRectangleProjection = TheCanvas.TransformToVisual(GridInsideZoomBorder);

            var rect = new Rect(
                new Point(Canvas.GetLeft(DragSelectionBorder), Canvas.GetTop(DragSelectionBorder)),
                new Size(DragSelectionBorder.Width, DragSelectionBorder.Height));
            var newBounds = selectionRectangleProjection.TransformBounds(rect);
            var dragRect = newBounds;

            //
            // Inflate the drag selection-rectangle by 1/10 of its size to 
            // make sure the intended item is selected.
            //
            dragRect.Inflate(rect.Width / 10, rect.Height / 10);

            MainViewModel.Instance().ClearSelection();


            foreach (var fuViewModel in ViewModel.FunctionUnits)
            {
                var itemRect = new Rect(fuViewModel.Model.Position.X, fuViewModel.Model.Position.Y, fuViewModel.Width, fuViewModel.Height);
                if (dragRect.Contains(itemRect))
                {
                    MainViewModel.Instance().AddToSelection(fuViewModel);
                }
            }
        }


        #region focus methods

        private void FocusDataStream(DataStream dataStream)
        {
            MainViewModel.Instance().SelectedFunctionUnits.Clear();

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
            if (frameworkelement != null)
                frameworkelement.DataNamesControl.TextBox.SelectionStart = frameworkelement.DataNamesControl.TextBox.Text.First()
                    .Equals('(')
                    ? 1
                    : 0;
        }


        private void FocusFunctionUnit(FunctionUnit fuModel)
        {
            MainViewModel.Instance().SelectedFunctionUnits.Clear();

            FunctionUnitView frameworkelement = null;
            FunctionUnitViewModel viewmodel = null;

            GetFunctionUnit(fuModel, ref frameworkelement, ref viewmodel);

            if (viewmodel == null)
                return;

            Action a = () =>
            {
                frameworkelement.Fu.TheTextBox.Focus();
                frameworkelement.Fu.TheTextBox.SelectionStart = frameworkelement.Fu.TheTextBox.Text.Length;
            };
            frameworkelement.Fu.TheTextBox.Dispatcher.BeginInvoke(DispatcherPriority.Background, a);

            viewmodel.IsSelected = true;
        }


        private void FocusDefinition(DataStreamDefinition dsd)
        {
            MainViewModel.Instance().SelectedFunctionUnits.Clear();
            FunctionUnitView frameworkelement = null;
            FunctionUnitViewModel viewmodel = null;

            GetFunctionUnit(dsd.Parent, ref frameworkelement, ref viewmodel);

            if (viewmodel == null)
                return;



            FocusTextboxOfDSD(dsd, frameworkelement.OutputFlow);
            FocusTextboxOfDSD(dsd, frameworkelement.InputFlow);
        }


        private static void FocusTextboxOfDSD(DataStreamDefinition dsd, ItemsControl itemsControl)
        {
            for (var i = 0; i < itemsControl.Items.Count; i++)
            {

                var item = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (item.DataContext.GetType() != typeof(DangelingConnectionViewModel))
                    continue;

                var dsdViewModel = (DangelingConnectionViewModel)item.DataContext;
                if (dsdViewModel.Model != dsd) continue;

                var actionKey = new DataTemplateKey(typeof(DangelingConnectionViewModel));
                var actionTemplate = itemsControl.Resources[actionKey] as DataTemplate;
                var dsdView = actionTemplate?.FindName("DangelingConnectionViews", item) as DangelingConnectionView;
                if (dsdView == null) continue;





                dsdView.TheDataNamesControl.TextBox.Focus();
                dsdView.TheDataNamesControl.TextBox.SelectionStart = dsdView.TheDataNamesControl.TextBox.Text.First()
                    .Equals('(')
                    ? 1
                    : 0;
                break;
            }
        }


        private void GetFunctionUnit(FunctionUnit newFumodel, ref FunctionUnitView frameworkelement,
            ref FunctionUnitViewModel viewmodel)
        {
            for (var i = 0; i < FunctionUnitsList.Items.Count; i++)
            {
                var c =
                    (ContentPresenter)FunctionUnitsList.ItemContainerGenerator.ContainerFromIndex(i);
                c.ApplyTemplate();

                frameworkelement = (FunctionUnitView)c.ContentTemplate.FindName("FunctionUnits", c);

                if (frameworkelement != null)
                {
                    viewmodel = (FunctionUnitViewModel)frameworkelement.DataContext;
                    if (viewmodel.Model == newFumodel)
                    {
                        break;
                    }
                }
            }
        }

        #endregion



        private void AddNewFunctionUnit_Click(object sender, RoutedEventArgs e)
        {
            var positionClicked = GetClickedPosition();
            if (positionClicked == null) return;

            var newFumodel = Interactions.AddNewFunctionUnit(positionClicked.Value, ViewModelModel());
            FocusFunctionUnit(newFumodel);
        }

        private void Paste_click(object sender, RoutedEventArgs e)
        {
            var positionClicked = GetClickedPosition();
            if (positionClicked == null) return;

            Interactions.Paste(positionClicked.Value, MainViewModel.Instance().Model);
        }


        private Point? GetClickedPosition()
        {
            var transform = TransformToVisual(FunctionUnitsList);
            var positionClicked = transform?.Transform(TheZoomBorder.BeforeContextMenuPoint);
            return positionClicked;
        }


        public void AppendNewFunctionUnit()
        {
            Keyboard.FocusedElement.TryGetDataContext<FunctionUnitViewModel>(vm =>
                {
                    var unnconnected = vm.Outputs.First(x => !x.Model.Connected);
                    if (unnconnected != null)
                        AppendNewFunctionUnitBehind(vm, (DangelingConnectionViewModel) unnconnected);
                }
            );

            Keyboard.FocusedElement.TryGetDataContext<DataStreamDefinition>(dsd =>
            {
                var fuVM = MainViewModel.Instance().FunctionUnits.First(fu => fu.Model == dsd.Parent);
                var vm = fuVM.Outputs.First(dsdVM => dsdVM.Model == dsd);
                AppendNewFunctionUnitBehind(fuVM, (DangelingConnectionViewModel)vm);
            });
        }


        private void AppendNewFunctionUnitBehind(FunctionUnitViewModel vm, DangelingConnectionViewModel dangelingConnectionViewModel)
        {
            var width = dangelingConnectionViewModel.Width;
            width += vm.Width / 2 + 100;
            var focusedFu = vm.Model;
            var nextmodel = Interactions.AppendNewFunctionUnit(focusedFu, width, dangelingConnectionViewModel.Model,
                ViewModelModel());

            SetFocusOnObject(nextmodel);
        }


        public void TabStopMove(Func<object, MainModel, object> tabstopFunc)
        {
            var focusedelement = Keyboard.FocusedElement;
            focusedelement.TryGetDataContext<FunctionUnitViewModel>(vm =>
            {
                var focusedFu = vm.Model;
                var nextmodel = tabstopFunc(focusedFu, ViewModelModel());
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


        public void NewOrFirstIntegrated(bool ctrlDown)
        {
            Keyboard.FocusedElement.TryGetDataContext<FunctionUnitViewModel>(vm =>
            {
                FunctionUnit nextmodel =null;
                if (ctrlDown)
                    nextmodel = Interactions.CreateNewOrGetFirstIntegrated(vm.Model, ViewModelModel());
                else
                    nextmodel = Interactions.GetFirstIntegrated(vm.Model, ViewModelModel());

                SetFocusOnObject(nextmodel);
            });

        }


        private void SetFocusOnObject(object model)
        {
            model.TryCast<FunctionUnit>(FocusFunctionUnit);
            model.TryCast<DataStream>(FocusDataStream);
            model.TryCast<DataStreamDefinition>(FocusDefinition);
        }
    }
}
