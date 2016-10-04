using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign.CustomControls
{
    public class Pointer : Canvas
    {
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof (Point), typeof (Pointer));

        public static readonly DependencyProperty ArrowSizeProperty =
            DependencyProperty.Register("ArrowSize", typeof (Point), typeof (Pointer));

        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(Point), typeof(Pointer));

        public static readonly DependencyProperty FillColorProperty =
         DependencyProperty.Register("FillColor", typeof(SolidColorBrush), typeof(Pointer));

        public static readonly DependencyProperty TextProperty =
 DependencyProperty.Register("Text", typeof(string), typeof(Pointer));

        //    public static readonly DependencyProperty TextBoxProperty =
        //DependencyProperty.Register("TextBox", typeof(ContentPresenter), typeof(Pointer));

        private readonly Path arrowShape;
        private readonly Path pathShape;
        private readonly TextBox txtBox;

        private readonly double connectionExtensionLength = 100;
        private static bool IsDragging;

        public Pointer()
        {

            pathShape = new Path
            {
                Stroke = FillColor,
                StrokeThickness = 3
            };

            arrowShape = new Path
            {
                Stroke = FillColor,
                Fill = FillColor,               
                StrokeThickness = 0

            };
            
            arrowShape.MouseDown += (sender, args) =>
            {
                arrowShape.IsHitTestVisible = false;
                pathShape.IsHitTestVisible = false;
                IsDragging = true;
                (DataContext as ConnectionViewModel).IsDragging = true;
                try
                {
                    (DataContext as ConnectionViewModel).End = null;
                    DragDrop.DoDragDrop((DependencyObject) args.Source, this, DragDropEffects.Move);
                }
                catch
                {
                    // ignored
                }
                (DataContext as ConnectionViewModel).IsDragging = false;
                IsDragging = false;
                arrowShape.IsHitTestVisible = true;
                pathShape.IsHitTestVisible = true;

            };

           

            arrowShape.MouseEnter += (sender, args) =>
            {
                arrowShape.Fill = Brushes.Red;
            };

            arrowShape.MouseLeave += (sender, args) =>
            {
                if (IsDragging) return;
                arrowShape.Fill = FillColor;
            };





            txtBox = new TextBox();

            Children.Add(pathShape);
            Children.Add(arrowShape);
            Children.Add(txtBox);

            DependencyPropertyDescriptor
                .FromProperty(EndProperty, typeof (Pointer))
                .AddValueChanged(this, (s, e) => Update());

            DependencyPropertyDescriptor
                .FromProperty(StartProperty, typeof(Pointer))
                .AddValueChanged(this, (s, e) => Update());


            DependencyPropertyDescriptor
                .FromProperty(ArrowSizeProperty, typeof(Pointer))
                .AddValueChanged(this, (s, e) => Update());

            DependencyPropertyDescriptor
                .FromProperty(FillColorProperty, typeof(Pointer))
                .AddValueChanged(this, (s, e) => Update());

            DependencyPropertyDescriptor
                .FromProperty(TextProperty, typeof(Pointer))
                .AddValueChanged(this, (s, e) => Update());

            //path.GetPointAtFractionLength(0.5, out centerPoint, out tg);
        }

        public Point End
        {
            get { return (Point) GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }


        public Point Start
        {
            get { return (Point) GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }

        public Point ArrowSize
        {
            get { return (Point) GetValue(ArrowSizeProperty); }
            set { SetValue(ArrowSizeProperty, value); }
        }

        public SolidColorBrush FillColor
        {
            get { return (SolidColorBrush)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }



        private void Update()
        {
            arrowShape.Stroke = FillColor;
            arrowShape.Fill = FillColor;
            pathShape.Stroke = FillColor;

            var end = End;
            var start = Start;

            txtBox.Text = Text;

            end.X -= ArrowSize.X;
            var figure = new PathFigure
            {
                StartPoint = start,
                IsClosed = false
            };
            var startextend = new Point(start.X + connectionExtensionLength, start.Y);
            var endextend = new Point(end.X - connectionExtensionLength, end.Y);
            figure.Segments.Add(new BezierSegment(startextend, endextend, end, true));

           

            //Point tg;
            var path = new PathGeometry();
            path.Figures.Add(figure);

            Point centerPoint;
            Point tg;
            path.GetPointAtFractionLength(0.5, out centerPoint, out tg);
            Canvas.SetLeft(txtBox, centerPoint.X);
            Canvas.SetTop(txtBox, centerPoint.Y);

            pathShape.Data = path;

            var position = End;

            position.X -= ArrowSize.X;
            figure = new PathFigure
            {
                StartPoint = position,
                IsClosed = true
            };
            var pts = new List<Point>
            {
                new Point(position.X, position.Y - ArrowSize.Y/2),
                new Point(position.X + ArrowSize.X, position.Y),
                new Point(position.X, position.Y + ArrowSize.Y/2)
            };
            figure.Segments.Add(new PolyLineSegment(pts, true));
            path = new PathGeometry();
            path.Figures.Add(figure);
            arrowShape.Data = path;
        }
    }
}