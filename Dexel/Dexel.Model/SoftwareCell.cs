using System;
using System.Collections.Generic;
using System.Windows;
using Dexel.Contracts.Model;
using PropertyChanged;

namespace Dexel.Model
{

    [ImplementPropertyChanged]
    public class SoftwareCell : ISoftwareCell
    {
        public SoftwareCell()
        {
            // Inputs / Outputs
            InputStreams = new List<IDataStreamDefinition>();
            OutputStreams = new List<IDataStreamDefinition>();
            Integration = new List<ISoftwareCell>();
        }


        public string Name { get; set; }
        public Guid ID { get; set; }
        public IFlowAttribute Attribute { get; set; } // Pyramide or Barrel
        public List<ISoftwareCell> Integration { get; private set; } // ZOOM into this Node. Start node of the integrated flow
        public Point Position { get; set; }
        public List<IDataStreamDefinition> InputStreams { get; set; }
        public List<IDataStreamDefinition> OutputStreams { get; set; }
    }

}