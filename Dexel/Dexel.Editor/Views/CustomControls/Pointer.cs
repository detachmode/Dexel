using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Dexel.Editor.ViewModels.DrawingBoard;
using Dexel.Editor.Views.DragAndDrop;

namespace Dexel.Editor.Views.CustomControls
{

    public class Pointer : Canvas
    {
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof(Point), typeof(Pointer));

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(Pointer));

        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(Point), typeof(Pointer));

        public static readonly DependencyProperty FillColorProperty =
            DependencyProperty.Register("FillColor", typeof(SolidColorBrush), typeof(Pointer));

        public static readonly DependencyProperty OuterFillColorProperty =
            DependencyProperty.Register("OuterFillColor", typeof(SolidColorBrush), typeof(Pointer));

        //private static bool _isDragging;



        private double _connectionExtensionLength = 100;
        private readonly Path _outerPathShape;
        private readonly Path _pathShape;
        //private bool _isMouseClicked;

        public Pointer()
        {
            _pathShape = new Path
            {
                Stroke = FillColor,
                StrokeThickness = Thickness
            };

            _outerPathShape = new Path
            {
                Stroke = FillColor,
                StrokeThickness = 12
            };

          

            //_arrowShape.MouseLeave += (sender, args) =>
            //{
            //    if (!_isMouseClicked) return;

            //    FrameworkElementDragBehavior.DragDropInProgressFlag = true;

            //    try
            //    {
            //        ((ConnectionViewModel) DataContext).End = null;
            //        var data = new DataObject();
            //        data.SetData(typeof(ConnectionViewModel), DataContext);
            //        DragDrop.DoDragDrop((DependencyObject)args.Source, data, DragDropEffects.Move);

            //    }
            //    catch
            //    {
            //        // ignored
            //    }

            //    FrameworkElementDragBehavior.DragDropInProgressFlag = false;
            //    _isDragging = false;
            //    _isMouseClicked = false;
            //};

            //_arrowShape.MouseDown += (sender, args) =>
            //{
            //    _isMouseClicked = true;
            //    args.Handled = true;
            //};

            //_arrowShape.MouseUp += (sender, args) =>
            //{
            //    _isMouseClicked = false;
            //    args.Handled = true;
            //};


            //_arrowShape.MouseEnter += (sender, args) => { _arrowShape.Stroke = Brushes.Red; };

            //_arrowShape.MouseLeave += (sender, args) =>
            //{
            //    if (_isDragging) return;
            //    _arrowShape.Stroke = FillColor;
            //};


            Children.Add(_outerPathShape);
            Children.Add(_pathShape);


            DependencyPropertyDescriptor
                .FromProperty(EndProperty, typeof(Pointer))
                .AddValueChanged(this, (s, e) => Update());

            DependencyPropertyDescriptor
                .FromProperty(StartProperty, typeof(Pointer))
                .AddValueChanged(this, (s, e) => Update());

            DependencyPropertyDescriptor
                .FromProperty(FillColorProperty, typeof(Pointer))
                .AddValueChanged(this, (s, e) => Update());

            DependencyPropertyDescriptor
                .FromProperty(OuterFillColorProperty, typeof(Pointer))
                .AddValueChanged(this, (s, e) => Update());


            DependencyPropertyDescriptor
                .FromProperty(ThicknessProperty, typeof(Pointer))
                .AddValueChanged(this, (s, e) => Update());

            //path.GetPointAtFractionLength(0.5, out centerPoint, out tg);
        }


        public Point End
        {
            get { return (Point) GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }


        public double Thickness
        {
            get { return (double) GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }


        public Point Start
        {
            get { return (Point) GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }



        public SolidColorBrush FillColor
        {
            get { return (SolidColorBrush) GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        public SolidColorBrush OuterFillColor
        {
            get { return (SolidColorBrush) GetValue(OuterFillColorProperty); }
            set { SetValue(OuterFillColorProperty, value); }
        }


        private void Update()
        {

            _pathShape.Stroke = FillColor;
            _pathShape.StrokeThickness = Thickness;
            _outerPathShape.Stroke = OuterFillColor;


            UpdatePath();

            UpdateViewModel();

        }


       


        private void OverrideShapeData(Path shape, PathFigure figure)
        {
            var path = new PathGeometry();
            path.Figures.Add(figure);
            shape.Data = path;
        }


        private void UpdateViewModel()
        {
            var connectionViewModel = DataContext as ConnectionViewModel;
            if (connectionViewModel != null)
                connectionViewModel.Center = GetCenterPoint();
        }


        private void UpdatePath()
        {
            var length = (Start - End).Length;
            _connectionExtensionLength = length/2.0;

            var end = End;
            var start = Start;
            
            var startextend = new Point(start.X + _connectionExtensionLength, start.Y);
            var endextend = new Point(end.X - _connectionExtensionLength -30, end.Y);

            var figure = new PathFigure();
            figure.IsClosed = false;
            figure.StartPoint = start;
            figure.Segments.Add(new BezierSegment(startextend, endextend, end, true));

            OverrideShapeData(_pathShape, figure);
            OverrideShapeData(_outerPathShape, figure);


            var figure2 = new PathFigure();
            figure2.IsClosed = false;
            figure2.StartPoint = end;
            figure2.Segments.Add(new LineSegment(End, true));
            ((PathGeometry) _outerPathShape.Data).Figures.Add(figure2);


        }


        private Point GetCenterPoint()
        {
            Point centerPoint, tg;
            ((PathGeometry) _pathShape.Data).GetPointAtFractionLength(0.5, out centerPoint, out tg);
            return centerPoint;
        }
    }

}