using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Petzold.Media2D;
using PropertyChanged;

namespace SharpFlowDesign.ViewModels
{

    [ImplementPropertyChanged]
    public class ConnectionCanvas : Canvas
    {
        public IOCellViewModel Start { get; set; }
        public IOCellViewModel End { get; set; }


        private Point _endPoint;
        public Point EndPoint
        {
            get
            {
                return _endPoint;
            }
            set
            {
                this._endPoint = value;
                //Update();
            }
        }

        public List<Connection> connections;


        private ArrowLine _arrow = new ArrowLine();
        private readonly Connection _connection;


        public ConnectionCanvas()
        {
            _arrow.Stroke = new SolidColorBrush(Colors.Red);
            _arrow.StrokeThickness = 3;
            this.Children.Add(_arrow);
            _connection = new Connection(this);
        }



    }
}
