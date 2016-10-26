using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dexel.Editor.Behavior;
using Dexel.Editor.CustomControls;
using Dexel.Editor.ViewModels;

namespace Dexel.Editor.Views
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

        #region private fields

        /// <summary>
        /// Set to 'true' when the left mouse-button is down.
        /// </summary>
        private bool isLeftMouseButtonDownOnWindow = false;

        /// <summary>
        /// Set to 'true' when dragging the 'selection rectangle'.
        /// Dragging of the selection rectangle only starts when the left mouse-button is held down and the mouse-cursor
        /// is moved more than a threshold distance.
        /// </summary>
        private bool isDraggingSelectionRect = false;

        /// <summary>
        /// Records the location of the mouse (relative to the window) when the left-mouse button has pressed down.
        /// </summary>
        private Point origMouseDownPoint;

        /// <summary>
        /// The threshold distance the mouse-cursor must move before drag-selection begins.
        /// </summary>
        private static readonly double DragThreshold = 5;

        /// <summary>
        /// Set to 'true' when the left mouse-button is held down on a rectangle.
        /// </summary>
        private bool isLeftMouseDownOnRectangle = false;

        /// <summary>
        /// Set to 'true' when the left mouse-button and control are held down on a rectangle.
        /// </summary>
        private bool isLeftMouseAndControlDownOnRectangle = false;

        /// <summary>
        /// Set to 'true' when dragging a rectangle.
        /// </summary>
        private bool isDraggingRectangle = false;

        #endregion



        private MainViewModel ViewModel => (MainViewModel)DataContext;



        #region IOCell mouse events

        private void IOCell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("- " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            var rectangle = (FrameworkElement)sender;
            var IOCellViewModel = (IOCellViewModel)rectangle.DataContext;

            isLeftMouseDownOnRectangle = true;

            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {

                isLeftMouseAndControlDownOnRectangle = true;
            }
            else
            {

                isLeftMouseAndControlDownOnRectangle = false;

                if (this.SoftwareCellsList.SelectedItems.Count == 0)
                {
                    //
                    // Nothing already selected, select the item.
                    //
                    this.SoftwareCellsList.SelectedItems.Add(IOCellViewModel);
                }
                else if (this.SoftwareCellsList.SelectedItems.Contains(IOCellViewModel))
                {
                    // 
                    // Item is already selected, do nothing.
                    // We will act on this in the MouseUp if there was no drag operation.
                    //
                }
                else
                {
                    //
                    // Item is not selected.
                    // Deselect all, and select the item.
                    //
                    this.SoftwareCellsList.SelectedItems.Clear();
                    this.SoftwareCellsList.SelectedItems.Add(IOCellViewModel);
                }
            }

            rectangle.CaptureMouse();
            origMouseDownPoint = GetAbsoluteMousePosition(e.GetPosition(this));

            e.Handled = true;
        }


        public bool IgnoreCurrentMouseEvents = false;


        private void IOCell_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("- " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            if (isLeftMouseDownOnRectangle)
            {
                var rectangle = (FrameworkElement)sender;
                var IOCellViewModel = (IOCellViewModel)rectangle.DataContext;

                if (!isDraggingRectangle)
                {
                    //
                    // Execute mouse up selection logic only if there was no drag operation.
                    //
                    if (isLeftMouseAndControlDownOnRectangle)
                    {
                        //
                        // Control key was held down.
                        // Toggle the selection.
                        //
                        if (this.SoftwareCellsList.SelectedItems.Contains(IOCellViewModel))
                        {
                            //
                            // Item was already selected, control-click removes it from the selection.
                            //
                            this.SoftwareCellsList.SelectedItems.Remove(IOCellViewModel);
                        }
                        else
                        {
                            // 
                            // Item was not already selected, control-click adds it to the selection.
                            //
                            this.SoftwareCellsList.SelectedItems.Add(IOCellViewModel);
                        }
                    }
                    else
                    {
                        //
                        // Control key was not held down.
                        //
                        if (this.SoftwareCellsList.SelectedItems.Count == 1 &&
                            this.SoftwareCellsList.SelectedItem == IOCellViewModel)
                        {
                            //
                            // The item that was clicked is already the only selected item.
                            // Don't need to do anything.
                            //
                        }
                        else
                        {
                            //
                            // Clear the selection and select the clicked item as the only selected item.
                            //
                            this.SoftwareCellsList.SelectedItems.Clear();
                            this.SoftwareCellsList.SelectedItems.Add(IOCellViewModel);
                        }
                    }
                }

                rectangle.ReleaseMouseCapture();
                isLeftMouseDownOnRectangle = false;
                isLeftMouseAndControlDownOnRectangle = false;

                e.Handled = true;
            }

            isDraggingRectangle = false;
        }


        private void IOCell_MouseMove(object sender, MouseEventArgs e)
        {
            
            if (isDraggingRectangle)
            {
                //
                // Drag-move selected rectangles.
                //
                Point curMouseDownPoint = GetAbsoluteMousePosition(e.GetPosition(this));
                var dragDelta = curMouseDownPoint - origMouseDownPoint;

                origMouseDownPoint = curMouseDownPoint;

                foreach (IOCellViewModel iocell in this.SoftwareCellsList.SelectedItems)
                {


                    var pt = iocell.Model.Position;
                    pt.X += dragDelta.X;
                    pt.Y += dragDelta.Y;
                    iocell.Model.Position = pt;
                }
            }
            else if (isLeftMouseDownOnRectangle)
            {
                //
                // The user is left-dragging the rectangle,
                // but don't initiate the drag operation until
                // the mouse cursor has moved more than the threshold value.
                //
                Point curMouseDownPoint = GetAbsoluteMousePosition(e.GetPosition(this));
                var dragDelta = curMouseDownPoint - origMouseDownPoint;
                double dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > DragThreshold)
                {
                    //
                    // When the mouse has been dragged more than the threshold value commence dragging the rectangle.
                    //
                    isDraggingRectangle = true;
                }

                e.Handled = true;
            }
        }

        #endregion

        #region window mouse events

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("- " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            if (e.ChangedButton == MouseButton.Left)
            {
                if (FrameworkElementDragBehavior.DragDropInProgressFlag)
                {
                    IgnoreCurrentMouseEvents = true;
                    return;
                }
                //
                //  Clear selection immediately when starting drag selection.
                //
                SoftwareCellsList.SelectedItems.Clear();

                isLeftMouseButtonDownOnWindow = true;

                origMouseDownPoint = e.GetPosition(this);

                this.CaptureMouse();

                e.Handled = true;
            }
        }


        private Point GetAbsoluteMousePosition(Point pt)
        {          
            var selectionRectangleProjection = TheCanvas.TransformToVisual(GridInsideZoomBorder);
            return selectionRectangleProjection.Transform(pt);
           
        }


        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("- " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            if (e.ChangedButton == MouseButton.Left)
            {
                if (IgnoreCurrentMouseEvents)
                {
                    IgnoreCurrentMouseEvents = false;
                    return;
                }
                bool wasDragSelectionApplied = false;

                if (isDraggingSelectionRect)
                {
                    //
                    // Drag selection has ended, apply the 'selection rectangle'.
                    //

                    isDraggingSelectionRect = false;
                    ApplyDragSelectionRect();

                    e.Handled = true;
                    wasDragSelectionApplied = true;
                }

                if (isLeftMouseButtonDownOnWindow)
                {
                    isLeftMouseButtonDownOnWindow = false;
                    this.ReleaseMouseCapture();

                    e.Handled = true;
                }

                if (!wasDragSelectionApplied)
                {
                    //
                    // A click and release in empty space clears the selection.
                    //
                    SoftwareCellsList.SelectedItems.Clear();
                }
            }
        }


        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
          
            if (isDraggingSelectionRect)
            {

                Point curMouseDownPoint = e.GetPosition(this);
                UpdateDragSelectionRect(origMouseDownPoint, curMouseDownPoint);

                e.Handled = true;
            }
            else if (isLeftMouseButtonDownOnWindow)
            {

                Point curMouseDownPoint = e.GetPosition(this);
                var dragDelta = curMouseDownPoint - origMouseDownPoint;
                double dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > DragThreshold)
                {
                    //
                    // When the mouse has been dragged more than the threshold value drag selection will show up.
                    //
                    isDraggingSelectionRect = true;
                    InitDragSelectionRect(origMouseDownPoint, curMouseDownPoint);
                }

                e.Handled = true;
            }
        }

        #endregion

        private void InitDragSelectionRect(Point pt1, Point pt2)
        {
            UpdateDragSelectionRect(pt1, pt2);
            DragSelectionCanvas.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Update the position and size of the rectangle used for drag selection.
        /// </summary>
        private void UpdateDragSelectionRect(Point pt1, Point pt2)
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
        private void ApplyDragSelectionRect()
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

            SoftwareCellsList.SelectedItems.Clear();

            foreach (IOCellViewModel IOCellViewModel in this.ViewModel.SoftwareCells)
            {
                Rect itemRect = new Rect(IOCellViewModel.Model.Position.X, IOCellViewModel.Model.Position.Y, IOCellViewModel.CellWidth, IOCellViewModel.CellHeight);
                if (dragRect.Contains(itemRect))
                {
                    SoftwareCellsList.SelectedItems.Add(IOCellViewModel);
                }
            }
        }

        //private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    Debug.WriteLine("UIElement_OnMouseUp");
        //    Keyboard.ClearFocus();

        //    //Interactions.DeselectAll();

        //}

        //private void MainWindow_OnDragOver(object sender, DragEventArgs e)
        //{
        //    SetPointerToMousePosition(e);
        //}



        //private void SetPointerToMousePosition(DragEventArgs e)
        //{
        //    Point p2 = e.GetPosition(this);
        //    var obj = e.Data.GetData(e.Data.GetFormats()[0]);
        //    if (!(obj is Pointer)) return;

        //    var pointer = (obj as Pointer);
        //    var datacontext = (ConnectionViewModel)pointer.DataContext;

        //    pointer.End = p2;
        //}

    }
}
