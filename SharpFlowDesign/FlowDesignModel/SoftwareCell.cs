using System;
using System.Collections.Generic;
using PropertyChanged;

namespace FlowDesignModel
{
    [ImplementPropertyChanged]
    public class SoftwareCell
    {
        public Guid ID;
        public string Name { get; set; }
        public FlowAttribute Attribute { get; set; } // Pyramide or Barrel
        public SoftwareCell Integration { get; private set; } // ZOOM into this Node. Start node of the integrated flow
        public System.Windows.Point Position { get; set; }

        // Inputs / Outputs
        public  List<DataStreamDefinition> InputStreams = new List<DataStreamDefinition>();
        public  List<DataStreamDefinition> OutputStreams = new List<DataStreamDefinition>();

    }

}