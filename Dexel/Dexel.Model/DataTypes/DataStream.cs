using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [ImplementPropertyChanged]
    internal class DataStream : INotifyPropertyChanged
    {
        internal Guid ID { get; set; }
        internal string ActionName { get; set; }
        internal string DataNames { get; set; }
        internal List<DataStreamDefinition> Sources { get; set; }
        internal List<DataStreamDefinition> Destinations { get; set; }

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