using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Petzold.Media2D;
using PropertyChanged;
using SharpFlowDesign.Views;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class IOCellViewModel
    {
        public IOCellViewModel()
        {

            ArrowLinesStart = new List<ConnectionArrow>();
            ArrowLinesEnd = new List<ConnectionArrow>();
        }

        public string Name { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public Point Position { get; set; }
        public bool IsSelected { get; set; }
        public List<ConnectionArrow> ArrowLinesStart { get; set; }
        public List<ConnectionArrow> ArrowLinesEnd { get; set; }
        public double ActualWidth { get; set; }
        public double ActualHeight { get; set; }
        public Point InputPoint { get; set; }
        public Point OutputPoint { get; set; }


        public void Move(double x, double y)
        {
            var pos = this.Position;
            pos.X += x;
            pos.Y += y;
            this.Position = pos;
            foreach (var arrowLine in ArrowLinesStart)
            {
                arrowLine.SetValue(Canvas.LeftProperty, pos.X);
                arrowLine.SetValue(Canvas.TopProperty, pos.Y);
                arrowLine.Arrow.X1 = OutputPoint.X;
                arrowLine.Arrow.Y1 = OutputPoint.Y;
            }

            foreach (var arrowLine in ArrowLinesEnd)
            {
                //var width = (pos.X + InputPoint.X) - (double)arrowLine.GetValue(Canvas.LeftProperty);
                //var height = (pos.Y + InputPoint.Y) - (double)arrowLine.GetValue(Canvas.TopProperty);
                //arrowLine.Width = width;
                //arrowLine.Height = height;
                arrowLine.Arrow.X2 = pos.X+ InputPoint.X -(double)arrowLine.GetValue(Canvas.LeftProperty);
                arrowLine.Arrow.Y2 = pos.Y+InputPoint.Y - (double)arrowLine.GetValue(Canvas.TopProperty); ;
            }
        }



        public void Deselect()
        {
            this.IsSelected = false;
        }


        public void Select()
        {
            this.IsSelected = true;
        }
    }
}
