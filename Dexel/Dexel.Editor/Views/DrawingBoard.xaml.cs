using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dexel.Editor.CustomControls;
using Dexel.Editor.DragAndDrop;
using Dexel.Editor.ViewModels;
using Dexel.Library;

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

        private MainViewModel ViewModel => (MainViewModel)DataContext;



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
            MouseEventMediator.ProjectedMousePosition =  GetAbsoluteMousePosition(e.GetPosition(this));
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
