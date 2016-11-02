using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Dexel.Editor.DebuggingHelper;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views;

namespace Dexel.Editor.DragAndDrop
{

    public static class DragAndDropMediator
    {
        private static bool isLeftMouseButtonDownOnWindow;

        /// <summary>
        ///     Set to 'true' when dragging the 'selection rectangle'.
        ///     Dragging of the selection rectangle only starts when the left mouse-button is held down and the mouse-cursor
        ///     is moved more than a threshold distance.
        /// </summary>
        private static bool isDraggingSelectionRect;
        private static readonly double DragThreshold = 5;

        public static Point OrigMouseDownPoint;
        public static Point ProjectedMousePosition;
        public static Point ScreenMousePosition;



        private static bool isLeftMouseDownOnIOCell;
        private static bool isLeftMouseAndControlDownOnIOCell;
        private static bool isDraggingIOCell;


        public static event Action<IOCellViewModel> MouseDownOnIOCell;


        public static void MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("++MouseDOWN " + sender.GetType().Name);


            if (sender is IOCell)
            {
                IOCellMouseDown(sender, e);
                e.Handled = true;
            }

            if (sender is DrawingBoard)
            {
                DrawingBoardMouseDown(sender, e);
                e.Handled = true;
            }
        }


        private static void DrawingBoardMouseDown(object sender, MouseButtonEventArgs e)
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

                isLeftMouseButtonDownOnWindow = true;
                (sender as DrawingBoard).CaptureMouse();
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
                DrawingBoardMouseUp(sender, e);
            }
        }


        private static void DrawingBoardMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            if (isDraggingSelectionRect)
            {
                //
                // Drag selection has ended, apply the 'selection rectangle'.
                //
                isDraggingSelectionRect = false;
                (sender as DrawingBoard).ApplyDragSelectionRect();
            }
            else
            {
                MainViewModel.Instance().ClearSelection();
            }

            if (isLeftMouseButtonDownOnWindow)
            {
                isLeftMouseButtonDownOnWindow = false;
                (sender as DrawingBoard).ReleaseMouseCapture();
            }
        }


        public static void MouseMove(object sender, MouseEventArgs e)
        {
            MyDebug.WriteLineIfDifferent("++MouseMove " + sender.GetType().Name);
            if (sender is IOCell || isLeftMouseDownOnIOCell)
            {
                IOCellMouseMove(sender, e);
                e.Handled = true;
            }
            if (sender is DrawingBoard)
            {
                DrawingBoardMouseMove(sender, e);
                e.Handled = true;
            }
        }


        private static void DrawingBoardMouseMove(object sender, MouseEventArgs e)
        {
            var drawingboard = sender as DrawingBoard;

            if (isDraggingSelectionRect)
            {
                drawingboard.UpdateDragSelectionRect(OrigMouseDownPoint, ScreenMousePosition);
            }
            else if (isLeftMouseButtonDownOnWindow)
            {
                var dragDelta = ScreenMousePosition - OrigMouseDownPoint;
                var dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > DragThreshold)
                {
                    //
                    // When the mouse has been dragged more than the threshold value drag selection will show up.
                    //
                    isDraggingSelectionRect = true;
                    drawingboard.InitDragSelectionRect(OrigMouseDownPoint, ScreenMousePosition);
                }
            }
        }


        private static void IOCellMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (FrameworkElementDragBehavior.DragDropInProgressFlag)
            {
                return;
            }
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            var iocell = (FrameworkElement) sender;
            var IOCellViewModel = (IOCellViewModel) iocell.DataContext;

            isLeftMouseDownOnIOCell = true;

            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                isLeftMouseAndControlDownOnIOCell = true;
            }
            else
            {
                isLeftMouseAndControlDownOnIOCell = false;
                MainViewModel.Instance().SetSelection(IOCellViewModel);
            }

            iocell.CaptureMouse();
        }


        private static void IOCellMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isLeftMouseDownOnIOCell)
            {
                var iocell = (FrameworkElement) sender;
                var IOCellViewModel = (IOCellViewModel) iocell.DataContext;

                if (!isDraggingIOCell)
                {
                    //
                    // Execute mouse up selection logic only if there was no drag operation.
                    //
                    if (isLeftMouseAndControlDownOnIOCell)
                    {
                        MainViewModel.Instance().SetSelectionCTRLMod(IOCellViewModel);
                    }
                    else
                    {
                        MainViewModel.Instance().SetSelection(IOCellViewModel);
                    }
                }

                iocell.ReleaseMouseCapture();
                isLeftMouseDownOnIOCell = false;
                isLeftMouseAndControlDownOnIOCell = false;

                e.Handled = true;
            }

            isDraggingIOCell = false;
        }


        private static void IOCellMouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingIOCell)
            {
                //
                // Drag-move selected IOCell.
                //

                var dragDelta = ProjectedMousePosition - OrigMouseDownPoint;
                OrigMouseDownPoint = ProjectedMousePosition;
                MainViewModel.Instance().MoveSelectedIOCells(dragDelta);
            }
            else if (isLeftMouseDownOnIOCell)
            {
                //
                // The user is left-dragging the IOCell,
                // but don't initiate the drag operation until
                // the mouse cursor has moved more than the threshold value.
                //
                var dragDelta = ProjectedMousePosition - OrigMouseDownPoint;
                var dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > DragThreshold)
                {
                    //
                    // When the mouse has been dragged more than the threshold value commence dragging the rectangle.
                    //
                    isDraggingIOCell = true;

                    if (Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        MainViewModel.Instance().ShiftMoveSelection();
                    }
                }

                e.Handled = true;
            }
        }
    }

}