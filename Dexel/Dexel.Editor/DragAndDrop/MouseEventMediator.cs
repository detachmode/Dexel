using System;
using System.Windows;
using System.Windows.Input;
using Dexel.Editor.CustomControls;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views;
using SoftwareCell = Dexel.Model.DataTypes.SoftwareCell;

namespace Dexel.Editor.DragAndDrop
{

    public static class MouseEventMediator
    {
        private const double DragThreshold = 5;
        private static bool _isLeftMouseButtonDownOnWindow;
        private static bool _isDraggingSelectionRect;
        public static Point OrigMouseDownPoint;
        public static Point ProjectedMousePosition;
        public static Point ScreenMousePosition;
        private static bool _isLeftMouseDownOnIOCell;
        private static bool _isLeftMouseAndControlDownOnIOCell;
        private static bool _isDraggingIOCell;
        private static bool _isCTRLDraggingIOCell;
        private static SoftwareCell _mouseDownOnSoftwareCell;


        public static void MouseDown(object sender, MouseButtonEventArgs e)
        {
            sender.TryCast<IOCell>(cell => IOCellMouseDown(cell, e));
            sender.TryCast<DrawingBoard>(board => DrawingBoardMouseDown(board, e));

            // reset picking in any case
            Interactions.PickState = false;
            Mouse.OverrideCursor = null;
        }


        private static void DrawingBoardMouseDown(DrawingBoard sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (FrameworkElementDragBehavior.DragDropInProgressFlag)
            {
                FrameworkElementDragBehavior.DragDropInProgressFlag = false;
                return;
            }

            if (e.ChangedButton != MouseButton.Left) return;
            if (FrameworkElementDragBehavior.DragDropInProgressFlag) return;
            _isLeftMouseButtonDownOnWindow = true;

            MainViewModel.Instance().ClearSelection();
            sender.CaptureMouse();
        }


        public static void MouseUp(object sender, MouseButtonEventArgs e)
        {
            sender.TryCast<IOCell>(cell => IOCellMouseUp(cell, e));
            sender.TryCast<DrawingBoard>(board => DrawingBoardMouseUp(board, e));
        }


        private static void DrawingBoardMouseUp(DrawingBoard sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            if (_isDraggingSelectionRect)
            {
                _isDraggingSelectionRect = false;
                sender.ApplyDragSelectionRect();
            }
            else
            {
                MainViewModel.Instance().ClearSelection();
                Keyboard.ClearFocus();
                ((MainWindow) Application.Current.MainWindow).TheDrawingBoard.Focus();
            }

            if (!_isLeftMouseButtonDownOnWindow) return;
            _isLeftMouseButtonDownOnWindow = false;
            sender.ReleaseMouseCapture();
        }


        public static void MouseMove(object sender, MouseEventArgs e)
        {
            IOCellMouseMove(sender, e);
            sender.TryCast<DrawingBoard>(board => DrawingBoardMouseMove(board, e));
        }


        private static void DrawingBoardMouseMove(DrawingBoard drawingboard, MouseEventArgs e)
        {
            e.Handled = true;
            if (_isDraggingSelectionRect)
            {
                drawingboard.UpdateDragSelectionRect(OrigMouseDownPoint, ScreenMousePosition);
            }
            else if (_isLeftMouseButtonDownOnWindow)
            {
                var dragDelta = ScreenMousePosition - OrigMouseDownPoint;
                var dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance < DragThreshold) return;
                _isDraggingSelectionRect = true;
                drawingboard.InitDragSelectionRect(OrigMouseDownPoint, ScreenMousePosition);
            }
        }


        private static void IOCellMouseDown(IOCell iocell, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _mouseDownOnSoftwareCell = iocell.ViewModel().Model;

            if (FrameworkElementDragBehavior.DragDropInProgressFlag)
                return;
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (Interactions.PickState)
            {
                Interactions.SetPickedIntegration(iocell.ViewModel().Model, MainViewModel.Instance().Model);
                return;
            }

            _isLeftMouseDownOnIOCell = true;

            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                _isLeftMouseAndControlDownOnIOCell = true;
            }
            else
            {
                _isLeftMouseAndControlDownOnIOCell = false;
                MainViewModel.Instance().SetSelection(iocell.ViewModel());
            }

            iocell.CaptureMouse();
        }


        private static void IOCellMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLeftMouseDownOnIOCell)
            {
                var iocell = (FrameworkElement) sender;
                var ioCellViewModel = iocell.DataContext as IOCellViewModel;
                if (ioCellViewModel == null)
                    return;

                if (!_isDraggingIOCell && !_isCTRLDraggingIOCell)
                {
                    if (_isLeftMouseAndControlDownOnIOCell)
                        MainViewModel.Instance().SetSelectionCTRLMod(ioCellViewModel);
                    else
                        MainViewModel.Instance().SetSelection(ioCellViewModel);
                }

                iocell.ReleaseMouseCapture();
                _isLeftMouseDownOnIOCell = false;
                _isLeftMouseAndControlDownOnIOCell = false;

                e.Handled = true;
            }

            _isDraggingIOCell = false;
            _isCTRLDraggingIOCell = false;
        }


        private static void IOCellMouseMove(IOCell sender, MouseEventArgs e)
        {
            if (!_isLeftMouseDownOnIOCell) return;
            e.Handled = true;


            //if (!DragThresholdReached()) return;

            ModifiersKeysState(
                ctrlAndShift: DoCtrlShiftDraggingIOCells,
                onlyShift: DoShiftDraggingIOCells,
                onlyCtrl: () => _isCTRLDraggingIOCell = true
                );

            _isDraggingIOCell = true;

            if (_isCTRLDraggingIOCell)
                DoCtrlDraggingIOCells();

            if (_isDraggingIOCell)
                DraggingSelectedIOCells();
        }


        private static void DoCtrlShiftDraggingIOCells()
        {
            if (_isDraggingIOCell)
                return;

            _isCTRLDraggingIOCell = true;


            _mouseDownOnSoftwareCell =
                Interactions.DuplicateIOCellIncludingChildrenAndIntegrated(_mouseDownOnSoftwareCell,
                    MainViewModel.Instance().Model);
        }


        private static void ModifiersKeysState(Action ctrlAndShift = null, Action onlyCtrl = null,
            Action onlyShift = null)
        {
            if (IsCtrlDown() && IsShiftDown())
                ctrlAndShift?.Invoke();

            if (IsShiftDown() && !IsCtrlDown())
                onlyShift?.Invoke();

            if (IsCtrlDown() && !IsShiftDown())
                onlyCtrl?.Invoke();
        }


        private static bool IsShiftDown()
        {
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        }


        private static bool IsCtrlDown()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }


        private static void DoShiftDraggingIOCells()
        {
            if (_isDraggingIOCell)
                return;
           

            MainViewModel.Instance().DuplicateSelectionAndSelectNew();
        }


        private static bool DragThresholdReached()
        {
            var dragDelta = ProjectedMousePosition - OrigMouseDownPoint;
            var dragDistance = Math.Abs(dragDelta.Length);
            return dragDistance > DragThreshold;
        }


        private static void DraggingSelectedIOCells()
        {
            var dragDelta = ProjectedMousePosition - OrigMouseDownPoint;
            OrigMouseDownPoint = ProjectedMousePosition;

            MainViewModel.Instance().MoveSelectedIOCells(dragDelta);
        }


        private static void DoCtrlDraggingIOCells()
        {
            var dragDelta = ProjectedMousePosition - OrigMouseDownPoint;
            OrigMouseDownPoint = ProjectedMousePosition;


            if (_mouseDownOnSoftwareCell == null) return;

            Interactions.MoveIOCellIncludingChildrenAndIntegrated(
                _mouseDownOnSoftwareCell, dragDelta, MainViewModel.Instance().Model);
        }
    }

}