using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using PropertyChanged;
using SharpFlowDesign.Properties;

namespace SharpFlowDesign.Model
{
    [ImplementPropertyChanged]
    public class SoftwareCell
    {
        public Guid ID;
        public string Name { get; set; }
        public FlowAttribute Attribute { get; set; } // Pyramide or Barrel
        public SoftwareCell Integration { get; private set; } // ZOOM into this Node. Start node of the integrated flow
        public Point Position { get; set; }

        // Inputs / Outputs
        public  List<DataStreamDefinition> InputStreams = new List<DataStreamDefinition>();
        public  List<DataStreamDefinition> OutputStreams = new List<DataStreamDefinition>();

    }

}