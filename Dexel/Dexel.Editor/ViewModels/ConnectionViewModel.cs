using System;
using System.Windows;
using Dexel.Contracts.Model;
using Dexel.Editor.Behavior;
using PropertyChanged;

namespace Dexel.Editor.ViewModels
{
    [ImplementPropertyChanged]
    public class ConnectionViewModel : IDragable
    {

        public ConnectionViewModel()
        {
            // Just For Designer purspose
            //End = new Point(100,100);
            //Model = new DataStream();
            //Model.DataNames = "string";

        }

        public Guid ID { get; set; }
        public IDataStream Model { get; set; }
        public bool IsDragging { get; set; }
        public Point Start { get; set; }
        public Point? End { get; set; }
        public Point Center { get; set; }


        Type IDragable.DataType => typeof (ConnectionViewModel);



        public void LoadFromModel(IDataStream modelDataStream)
        {
            Model = modelDataStream;
            ID = modelDataStream.ID;
        }



    }
}