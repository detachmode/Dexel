using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using PropertyChanged;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.Model;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class ConnectionViewModel : IDragable
    {

        public ConnectionViewModel()
        {
            // Just For Designer purspose
            End = new Point(100,100);
            Model = new DataStream();
            Model.DataNames = "string";

        }
        public Guid ID;

        public DataStream Model { get; set; }
        public bool IsDragging { get; set; }
        public Point Start { get; set; }
        public Point? End { get; set; }
        public Point Center { get; set; }


        Type IDragable.DataType => typeof (ConnectionViewModel);



        public void LoadFromModel(DataStream modelDataStream)
        {
            Model = modelDataStream;
            ID = modelDataStream.ID;
        }



    }
}