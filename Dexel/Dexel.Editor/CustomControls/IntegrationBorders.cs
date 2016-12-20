using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;
using Dexel.Editor.Views;

namespace Dexel.Editor.CustomControls
{
    public class IntegrationBorders : Canvas
    {
        public static readonly DependencyProperty TopPointProperty =
           DependencyProperty.Register("TopPoint", typeof(Point), typeof(IntegrationBorders));

        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof(Point), typeof(IntegrationBorders));

        public static readonly DependencyProperty TopWidthProperty =
           DependencyProperty.Register("TopWidth", typeof(double), typeof(IntegrationBorders));
        
        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(IntegrationBorders));

        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(Point), typeof(IntegrationBorders));

        public static readonly DependencyProperty FillColorProperty =
            DependencyProperty.Register("FillColor", typeof(SolidColorBrush), typeof(IntegrationBorders));




        private readonly Path pathShape;
        public IntegrationBorders()
        {
            pathShape = new Path
            {
                Stroke = FillColor,
                StrokeThickness = Thickness
            };
        
            Children.Add(pathShape);


            DependencyPropertyDescriptor
               .FromProperty(TopPointProperty, typeof(IntegrationBorders))
               .AddValueChanged(this, (s, e) => Update());

            DependencyPropertyDescriptor
               .FromProperty(TopWidthProperty, typeof(IntegrationBorders))
               .AddValueChanged(this, (s, e) => Update());

            DependencyPropertyDescriptor
                .FromProperty(EndProperty, typeof(IntegrationBorders))
                .AddValueChanged(this, (s, e) => Update());

            DependencyPropertyDescriptor
                .FromProperty(StartProperty, typeof(IntegrationBorders))
                .AddValueChanged(this, (s, e) => Update());

            DependencyPropertyDescriptor
                .FromProperty(FillColorProperty, typeof(IntegrationBorders))
                .AddValueChanged(this, (s, e) => Update());

            DependencyPropertyDescriptor
                .FromProperty(ThicknessProperty, typeof(IntegrationBorders))
                .AddValueChanged(this, (s, e) => Update());

        }

        private void Update()
        {
            pathShape.Stroke = FillColor;
            pathShape.StrokeThickness = Thickness;
           
            UpdatePath();
        }


        private void UpdatePath()
        {

            var figure1 = new PathFigure { IsClosed = false };
            var start1 = TopPoint;
            start1.X = start1.X - TopWidth / 2;
            figure1.StartPoint = start1;
            figure1.Segments.Add(new LineSegment(Start, true));

            var path = new PathGeometry();
            path.Figures.Add(figure1);
            pathShape.Data = path;
        


            var figure2 = new PathFigure {IsClosed = false};
            var start = TopPoint;
            start.X = start.X + TopWidth/2;
            figure2.StartPoint = start;
            figure2.Segments.Add(new LineSegment(End, true));
            ((PathGeometry)pathShape.Data).Figures.Add(figure2);
        }


        public SolidColorBrush FillColor
        {
            get { return (SolidColorBrush)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        public Point End
        {
            get { return (Point)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        public Point TopPoint
        {
            get { return (Point)GetValue(TopPointProperty); }
            set { SetValue(TopPointProperty, value); }
        }

        public double TopWidth
        {
            get { return (double)GetValue(TopWidthProperty); }
            set { SetValue(TopWidthProperty, value); }
        }

        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        public Point Start
        {
            get { return (Point)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }
    };


}

