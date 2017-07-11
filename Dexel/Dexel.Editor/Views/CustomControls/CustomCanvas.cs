using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dexel.Editor.Views.CustomControls
{
    class CustomCanvas:Canvas
    {
            AdornerLayer aLayer;
            bool _isDown;
            bool _isDragging;
            bool selected = false;
            UIElement selectedElement = null;
            private ContentPresenter selectedPresenter = null;

            Point _startPoint;
            private double _originalLeft;
            private double _originalTop;

            public CustomCanvas()
            {
                Loaded += CustomCanvas_OnLoaded;
            }

            private void CustomCanvas_OnLoaded(object sender, RoutedEventArgs e)
            {
                this.MouseLeftButtonDown += new MouseButtonEventHandler(CustomCanvas_MouseLeftButtonDown);
                this.MouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
                this.MouseMove += new MouseEventHandler(CustomCanvas_MouseMove);
                this.MouseLeave += new MouseEventHandler(CustomCanvas_MouseLeave);

                PreviewMouseLeftButtonDown += new MouseButtonEventHandler(CustomCanvas_PreviewMouseLeftButtonDown);
                PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DragFinishedMouseHandler);
            }

            // Handler for drag stopping on leaving the window
            void CustomCanvas_MouseLeave(object sender, MouseEventArgs e)
            {
                StopDragging();
                e.Handled = true;
            }

            // Handler for drag stopping on user choise
            void DragFinishedMouseHandler(object sender, MouseButtonEventArgs e)
            {
                StopDragging();
                e.Handled = true;
            }

            // Method for stopping dragging
            private void StopDragging()
            {
                if (_isDown)
                {
                    _isDown = false;
                    _isDragging = false;
                }
            }

            // Hanler for providing drag operation with selected element
            void CustomCanvas_MouseMove(object sender, MouseEventArgs e)
            {
                if (_isDown)
                {
                    if ((_isDragging == false) &&
                        ((Math.Abs(e.GetPosition(this).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                         (Math.Abs(e.GetPosition(this).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                        _isDragging = true;

                    if (_isDragging)
                    {
                        Point position = Mouse.GetPosition(this);
                        Canvas.SetTop(selectedPresenter, position.Y - (_startPoint.Y - _originalTop));
                        Canvas.SetLeft(selectedPresenter, position.X - (_startPoint.X - _originalLeft));
                    }
                }
            }

            // Handler for clearing element selection, adorner removal
            void CustomCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (selected)
                {
                    selected = false;
                    if (selectedElement != null)
                    {
                        aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
                        selectedElement = null;
                    }
                }
            }

            // Handler for element selection on the canvas providing resizing adorner
            void CustomCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                // Remove selection on clicking anywhere the window
                if (selected)
                {
                    selected = false;
                    if (selectedElement != null)
                    {
                        // Remove the adorner from the selected element
                        var test = aLayer.GetAdorners(selectedElement)[0];
                        aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
                        selectedElement = null;
                    }
                }

                // If any element except canvas is clicked, 
                // assign the selected element and add the adorner
                if (e.Source != this)
                {
                    selectedElement = e.Source as UIElement;
                    selectedPresenter = VisualTreeHelper.GetParent(selectedElement) as ContentPresenter;
                    _isDown = true;
                    _startPoint = e.GetPosition(this);

                    selectedElement = e.Source as UIElement;

                    _originalLeft = Canvas.GetLeft(selectedPresenter);
                    _originalTop = Canvas.GetTop(selectedPresenter);

                    aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
                    aLayer.Add(new ResizingAdorner(selectedElement));
                    selected = true;
                    e.Handled = true;
                }
            }
        }
}
