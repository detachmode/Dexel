using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;
using SharpFlowDesign.Properties;

namespace SharpFlowDesign.Model
{
    [ImplementPropertyChanged]
    public class DataStream : INotifyPropertyChanged
    {
        public DataStream()
        {
            Sources = new List<SoftwareCell>();
            Destinations = new List<SoftwareCell>();
        }
        public Guid ID;
        public string ActionName { get; set; }
        public string DataNames { get; set; }
        public bool Optional { get; set; }
        public List<SoftwareCell> Sources { get; }
        public List<SoftwareCell> Destinations { get; }


        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}