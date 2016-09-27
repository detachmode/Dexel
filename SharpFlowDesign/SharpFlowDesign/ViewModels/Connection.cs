using System;
using System.Windows;

namespace SharpFlowDesign.ViewModels
{

    public class Connection
    {
        private Point _startPoint;


        public Connection(ConnectionCanvas connectionCanvas)
        {

        }


        public event Action<Connection> OnChanged; 

        public Point StartPoint
        {
            get
            {
                return _startPoint;
            }
            set
            {
                this._startPoint = value;
                OnChanged(this);
            }
        }
    }

}