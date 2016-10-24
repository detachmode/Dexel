using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using PropertyChanged;

namespace Dexel.Contracts.Model
{

    public enum FlowAttributeType
    {
        Provider,
        State
    }

    [ImplementPropertyChanged]
    public abstract class IMainModel
    {
        public List<IDataStream> Connections { get; set; }
        public List<ISoftwareCell> SoftwareCells { get; set; }
    }

    [ImplementPropertyChanged]
    public abstract class IFlowAttribute
    {
        public string Name { get; set; }
        public FlowAttributeType Type { get; set; }
    }

    [ImplementPropertyChanged]
    public abstract class IDataStreamDefinition
    {
        public Guid ID { get; set; }
        public string ActionName { get; set; }
        public bool Connected { get; set; }
        public string DataNames { get; set; }
        public ISoftwareCell Parent { get; set; }
    }

    [ImplementPropertyChanged]
    public abstract class IDataStream
    {
        public Guid ID { get; set; }
        public string ActionName { get; set; }
        public string DataNames { get; set; }
        public List<IDataStreamDefinition> Sources { get; set; }
        public List<IDataStreamDefinition> Destinations { get; set; }
    }

    [ImplementPropertyChanged]
    public abstract class ISoftwareCell
    {
        public Guid ID { get; set; }
        public IFlowAttribute Attribute { get; set; }
        public List<ISoftwareCell> Integration { get; set; }
        public string Name { get; set; }
        public Point Position { get; set; }
        public List<IDataStreamDefinition> InputStreams { get; set; }
        public List<IDataStreamDefinition> OutputStreams { get; set; }
    }
}
