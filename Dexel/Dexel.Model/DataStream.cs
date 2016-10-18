using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dexel.Contracts.Model;
using PropertyChanged;

namespace Dexel.Model
{


    [ImplementPropertyChanged]
    public class DataStream : INotifyPropertyChanged, IDataStream
    {

        public DataStream()
        {
            Sources = new List<IDataStreamDefinition>();
            Destinations = new List<IDataStreamDefinition>();
            PropertyChanged += OnPropertyChanged;
        }


        public Guid ID { get; set; }
        public string ActionName { get; set; }
        public string DataNames { get; set; }
        public List<IDataStreamDefinition> Sources { get; }
        public List<IDataStreamDefinition> Destinations { get; }


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