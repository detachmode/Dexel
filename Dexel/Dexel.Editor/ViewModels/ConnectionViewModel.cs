using System;
using System.Windows;

using Dexel.Editor.Behavior;
using Dexel.Model;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels
{
    [ImplementPropertyChanged]
    public class ConnectionViewModel : IDragable
    {

        public ConnectionViewModel()
        {
            // Just For Designer purspose
            End = new Point(100,100);
            Model = DataStreamManager.NewDataStream("string");

        }

        public Guid ID { get; set; }
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