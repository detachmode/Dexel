using System;
using System.Collections.Generic;
using PropertyChanged;

namespace SharpFlowDesign.Model
{
    [ImplementPropertyChanged]
    public class DataStream
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
    }
}