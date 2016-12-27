using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dexel.Editor.ViewModels;

namespace Dexel.Editor.Views.CustomControls
{
    public class ZoomBorder : Border
    {
        private UIElement _child;
        private Point _origin;
        private Point _start;

        public Point BeforeContextMenuPoint;

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && !Equals(value, Child))
                    Initialize(value);
                base.Child = value;
            }
        }



        public void Initialize(UIElement element)
        {
            _child = element;
            if (_child == null) return;
            var group = new TransformGroup();
            var st = new ScaleTransform();
            group.Children.Add(st);
            var tt = new TranslateTransform();
            group.Children.Add(tt);
            _child.RenderTransform = group;
            _child.RenderTransformOrigin = new Point(0.0, 0.0);
            MouseWheel += child_MouseWheel;
            MouseDown += child_MouseLeftButtonDown;
            MouseUp += child_MouseLeftButtonUp;
            MouseMove += child_MouseMove;
            PreviewMouseRightButtonDown += SavePositionBeforeContextMenu;
        }

        private void SavePositionBeforeContextMenu(object sender, MouseButtonEventArgs e)
        {
            BeforeContextMenuPoint = e.GetPosition(this);
        }

        public void Reset()
        {
            if (_child == null) return;
            // reset zoom
            var st = GetScaleTransform(_child);
            st.ScaleX = 1;
            st.ScaleY = 1;

            // reset pan
            var tt = GetTranslateTransform(_child);
            tt.X = 0.0;
            tt.Y = 0.0;

            SetFontDependingOnZoom(st);


        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_child == null) return;
            var st = GetScaleTransform(_child);
            var tt = GetTranslateTransform(_child);


            var zoom = e.Delta > 0 ? .1 : -.1;


            if (!(e.Delta > 0) && (st.ScaleX < .3 || st.ScaleY < .3))
                return;

            var relative = e.GetPosition(_child);

            var abosuluteX = relative.X * st.ScaleX + tt.X;
            var abosuluteY = relative.Y * st.ScaleY + tt.Y;

            st.ScaleX += zoom;
            st.ScaleY += zoom;


            SetFontDependingOnZoom(st);

            tt.X = abosuluteX - relative.X * st.ScaleX;
            tt.Y = abosuluteY - relative.Y * st.ScaleY;
        }


        private static void SetFontDependingOnZoom(ScaleTransform st)
        {
            if (st.ScaleX < 0.21 || st.ScaleY < 0.21)
            {
                MainViewModel.Instance().FontSizeFunctionUnit = 32;
                MainViewModel.Instance().VisibilityBlockTextBox = Visibility.Visible;
                MainViewModel.Instance().VisibilityDatanames = Visibility.Hidden;
            }
            if (st.ScaleX < 0.51 || st.ScaleY < 0.51)
            {
                MainViewModel.Instance().FontSizeFunctionUnit = 22;
                MainViewModel.Instance().VisibilityBlockTextBox = Visibility.Visible;
                MainViewModel.Instance().VisibilityDatanames = Visibility.Hidden;
            }
            else if (st.ScaleX < 0.71 || st.ScaleY < 0.71)
            {
                MainViewModel.Instance().FontSizeFunctionUnit = 16;
                MainViewModel.Instance().VisibilityDatanames = Visibility.Hidden;
                MainViewModel.Instance().VisibilityBlockTextBox = Visibility.Visible;
            }
            else
            {
                MainViewModel.Instance().FontSizeFunctionUnit = 12;
                MainViewModel.Instance().VisibilityBlockTextBox = Visibility.Hidden;
                MainViewModel.Instance().VisibilityDatanames = Visibility.Visible;
            }
        }


        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle) return;
            if (_child == null) return;
            var tt = GetTranslateTransform(_child);
            _start = e.GetPosition(this);
            _origin = new Point(tt.X, tt.Y);
            Cursor = Cursors.Hand;
            _child.CaptureMouse();
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle) return;
            if (_child == null) return;
            _child.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }


        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (_child == null) return;
            if (!_child.IsMouseCaptured) return;
            var tt = GetTranslateTransform(_child);
            var v = _start - e.GetPosition(this);
            tt.X = _origin.X - v.X;
            tt.Y = _origin.Y - v.Y;
        }

        #endregion
    }
}

