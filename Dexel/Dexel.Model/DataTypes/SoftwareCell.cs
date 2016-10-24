using System;
using System.Collections.Generic;
using System.Windows;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{

    [ImplementPropertyChanged]
    public class SoftwareCell
    {
        public Guid ID { get; set; }
        public FlowAttribute Attribute { get; set; }
        public List<SoftwareCell> Integration { get; set; }
        public string Name { get; set; }
        public Point Position { get; set; }
        public List<DataStreamDefinition> InputStreams { get; set; }
        public List<DataStreamDefinition> OutputStreams { get; set; }

        internal SoftwareCell()
        {
            // Inputs / Outputs
            InputStreams = new List<DataStreamDefinition>();
            OutputStreams = new List<DataStreamDefinition>();
            Integration = new List<SoftwareCell>();
        }
    }


    public enum FlowAttribute
    {
        State,
        Provider
    }

}