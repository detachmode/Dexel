using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Dexel.Editor.DebuggingHelper;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views;

namespace Dexel.Editor.DragAndDrop
{

    public static class MouseEventMediator
    {
        private const double DragThreshold = 5;
        private static bool _isLeftMouseButtonDownOnWindow;

        /// <summary>
        ///     Set to 'true' when dragging the 'selection rectangle'.
        ///     Dragging of the selection rectangle only starts when the left mouse-button is held down and the mouse-cursor
        ///     is moved more than a threshold distance.
        /// </summary>
        private static bool _isDraggingSelectionRect;

        public static Point OrigMouseDownPoint;
        public static Point ProjectedMousePosition;
        public static Point ScreenMousePosition;


        private static bool _isLeftMouseDownOnIOCell;
        private static bool _isLeftMouseAndControlDownOnIOCell;
        private static bool _isDraggingIOCell;
        private static bool _isCTRLDraggingIOCell;
        private static IOCell _mouseDownOnIOCell;


        public static void MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("++MouseDOWN " + sender.GetType().Name);

            if (sender is IOCell)
            {
                IOCellMouseDown((IOCell) sender, e);
                e.Handled = true;
            }

            if (sender is DrawingBoard)
            {
                DrawingBoardMouseDown((DrawingBoard) sender, e);
                e.Handled = true;

            }

            Interactions.PickState = false;
            Mouse.OverrideCursor = null;
        }


        private static void DrawingBoardMouseDown(DrawingBoard sender, MouseButtonEventArgs e)
        {
            if (FrameworkElementDragBehavior.DragDropInProgressFlag)
            {
                FrameworkElementDragBehavior.DragDropInProgressFlag = false;
                return;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                if (FrameworkElementDragBehavior.DragDropInProgressFlag)
                {
                    return;
                }

                MainViewModel.Instance().ClearSelection();

                _isLeftMouseButtonDownOnWindow = true;
                sender.CaptureMouse();
            }
        }


        public static void MouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("++MouseUp " + sender.GetType().Name);
            if (sender is IOCell)
            {
                IOCellMouseUp(sender, e);
            }
            if (sender is DrawingBoard)
            {
                DrawingBoardMouseUp((DrawingBoard) sender, e);
            }
        }


        private static void DrawingBoardMouseUp(DrawingBoard sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            if (_isDraggingSelectionRect)
            {
                //
                // Drag selection has ended, apply the 'selection rectangle'.
                //
                _isDraggingSelectionRect = false;
                sender.ApplyDragSelectionRect();
            }
            else
            {
                MainViewModel.Instance().ClearSelection();
                Keyboard.ClearFocus();
                ((MainWindow) Application.Current.MainWindow).TheDrawingBoard.Focus();
            }

            if (_isLeftMouseButtonDownOnWindow)
            {
                _isLeftMouseButtonDownOnWindow = false;
                sender.ReleaseMouseCapture();
            }
        }


        public static void MouseMove(object sender, MouseEventArgs e)
        {
            MyDebug.WriteLineIfDifferent("++MouseMove " + sender.GetType().Name);
            if (sender is IOCell || _isLeftMouseDownOnIOCell)
            {
                IOCellMouseMove(sender as IOCell, e);
                e.Handled = true;
            }
            if (sender is DrawingBoard)
            {
                DrawingBoardMouseMove((DrawingBoard) sender, e);
                e.Handled = true;
            }
        }


        private static void DrawingBoardMouseMove(DrawingBoard drawingboard, MouseEventArgs e)
        {
            if (_isDraggingSelectionRect)
            {
                drawingboard.UpdateDragSelectionRect(OrigMouseDownPoint, ScreenMousePosition);
            }
            else if (_isLeftMouseButtonDownOnWindow)
            {
                var dragDelta = ScreenMousePosition - OrigMouseDownPoint;
                var dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > DragThreshold)
                {
                    // When the mouse has been dragged more than the threshold value drag selection will show up.
                    _isDraggingSelectionRect = true;
                    drawingboard.InitDragSelectionRect(OrigMouseDownPoint, ScreenMousePosition);
                }
            }
        }


        private static void IOCellMouseDown(IOCell iocell, MouseButtonEventArgs e)
        {
            _mouseDownOnIOCell = iocell;
            if (FrameworkElementDragBehavior.DragDropInProgressFlag)
            {
                return;
            }
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }


            var ioCellViewModel = (IOCellViewModel)iocell.DataContext;


            if (Interactions.PickState)
            {
                Interactions.SetPickedIntegration(ioCellViewModel.Model, MainViewModel.Instance().Model);
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
                MainViewModel.Instance().SetSelection(ioCellViewModel);
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
                    //
                    // Execute mouse up selection logic only if there was no drag operation.
                    //
                    if (_isLeftMouseAndControlDownOnIOCell)
                    {
                        MainViewModel.Instance().SetSelectionCTRLMod(ioCellViewModel);
                    }
                    else
                    {
                        MainViewModel.Instance().SetSelection(ioCellViewModel);
                    }
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
            bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (_isCTRLDraggingIOCell)
            {          
                var dragDelta = ProjectedMousePosition - OrigMouseDownPoint;
                OrigMouseDownPoint = ProjectedMousePosition;

               Interactions.MoveIOCellIncludingChildrenAndIntegrated((_mouseDownOnIOCell.DataContext as IOCellViewModel).Model,dragDelta, MainViewModel.Instance().Model);
            }

            if (_isDraggingIOCell)
            {
                var dragDelta = ProjectedMousePosition - OrigMouseDownPoint;
                OrigMouseDownPoint = ProjectedMousePosition;


                MainViewModel.Instance().MoveSelectedIOCells(dragDelta);
            }
            else if (_isLeftMouseDownOnIOCell)
            {
                // The user is left-dragging the IOCell,
                // but don't initiate the drag operation until
                // the mouse cursor has moved more than the threshold value.

                var dragDelta = ProjectedMousePosition - OrigMouseDownPoint;
                var dragDistance = Math.Abs(dragDelta.Length);

                if (ctrlDown && shiftDown)
                {
                    _isCTRLDraggingIOCell = true;
                    var softwarecell  = MainViewModel.Instance().DuplicateIncludingChildrenAndIntegrated((sender.DataContext as IOCellViewModel).Model);

                }

                else if (shiftDown)
                {
                    _isDraggingIOCell = true;
                    MainViewModel.Instance().DuplicateSelectionAndSelectNew();
                }

                else if (ctrlDown)
                    _isCTRLDraggingIOCell = true;


                if (dragDistance > DragThreshold)
                    // When the mouse has been dragged more than the threshold value commence dragging the rectangle.
                    _isDraggingIOCell = true;

                e.Handled = true;
            }
        }
    }

}