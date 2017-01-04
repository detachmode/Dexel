using System;
using System.Collections.Generic;
using System.Windows;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [Serializable]
    [ImplementPropertyChanged]
    public class FunctionUnit
    {
        public Guid ID { get; set; }
        public FlowAttribute Attribute { get; set; }
        public List<FunctionUnit> IsIntegrating { get; set; }
        public string Name { get; set; }
        public Point Position { get; set; }
        public List<DataStreamDefinition> InputStreams { get; set; }
        public List<DataStreamDefinition> OutputStreams { get; set; }

        public FunctionUnit()
        {
            // Inputs / Outputs
            InputStreams = new List<DataStreamDefinition>();
            OutputStreams = new List<DataStreamDefinition>();
            IsIntegrating = new List<FunctionUnit>();
        }
    }


    public enum FlowAttribute
    {
        State,
        Provider
    }

}