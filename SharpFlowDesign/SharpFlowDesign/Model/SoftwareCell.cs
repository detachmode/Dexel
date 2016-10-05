using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using PropertyChanged;
using SharpFlowDesign.Annotations;

namespace SharpFlowDesign.Model
{
    [ImplementPropertyChanged]
    public class SoftwareCell : INotifyPropertyChanged
    {
        public Guid ID;
        public string Name { get; set; }
        public FlowAttribute Attribute { get; set; } // Pyramide or Barrel
        public SoftwareCell Integration { get; private set; } // ZOOM into this Node. Start node of the integrated flow
        public Point Position { get; set; }

        // Inputs / Outputs
        public  List<DataStream> InputStreams = new List<DataStream>();
        public  List<DataStream> OutputStreams = new List<DataStream>();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}