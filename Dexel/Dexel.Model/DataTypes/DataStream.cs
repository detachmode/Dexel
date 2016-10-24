using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [ImplementPropertyChanged]
    public class DataStream : INotifyPropertyChanged
    {
        public Guid ID { get; internal set; }
        public string ActionName { get;  set; }
        public string DataNames { get;  set; }
        public List<DataStreamDefinition> Sources { get; internal set; }
        public List<DataStreamDefinition> Destinations { get; internal set; }

        internal DataStream()
        {
            Sources = new List<DataStreamDefinition>();
            Destinations = new List<DataStreamDefinition>();
            PropertyChanged += OnPropertyChanged;
        }


        public event PropertyChangedEventHandler PropertyChanged;


        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            args.WhenProperty("DataNames", UpdateDataStreamDefinitions);
        }


        private void UpdateDataStreamDefinitions()
        {
            Sources.ForEach(x => x.DataNames = DataNames);
            Destinations.ForEach(x => x.DataNames = DataNames);
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}