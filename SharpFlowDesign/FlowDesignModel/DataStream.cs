using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using PropertyChanged;

namespace FlowDesignModel
{
    [ImplementPropertyChanged]
    public class DataStream : INotifyPropertyChanged
    {
        public DataStream()
        {
            Sources = new List<DataStreamDefinition>();
            Destinations = new List<DataStreamDefinition>();
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            args.WhenProperty("DataNames", UpdateDataStreamDefinitions);
        }

        private void UpdateDataStreamDefinitions()
        {
           Sources.ForEach( x => x.DataNames = this.DataNames);
           Destinations.ForEach(x => x.DataNames = this.DataNames);
        }

        public Guid ID;
        public string ActionName { get; set; }
        public string DataNames { get; set; }
        public List<DataStreamDefinition> Sources { get; }
        public List<DataStreamDefinition> Destinations { get; }


        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}