using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

namespace Dexel.Contracts.Model
{

    public enum FlowAttributeType
    {
        Provider,
        State
    }

    public interface IMainModel
    {
        List<IDataStream> Connections { get; set; }
        List<ISoftwareCell> SoftwareCells { get; set; }
    }

    public interface IFlowAttribute
    {
        string Name { get; set; }
        FlowAttributeType Type { get; set; }
    }

    public interface IDataStreamDefinition
    {
        Guid ID { get; set; }
        string ActionName { get; set; }
        bool Connected { get; set; }
        string DataNames { get; set; }
        ISoftwareCell Parent { get; set; }
    }


    public interface IDataStream
    {
        Guid ID { get; set; }
        string ActionName { get; set; }
        string DataNames { get; set; }
        List<IDataStreamDefinition> Sources { get; }
        List<IDataStreamDefinition> Destinations { get; }
    }

    public interface ISoftwareCell
    {
        Guid ID { get; set; }
        IFlowAttribute Attribute { get; set; }
        List<ISoftwareCell> Integration { get; }
        string Name { get; set; }
        System.Windows.Point Position { get; set; }
        List<IDataStreamDefinition> InputStreams { get; set; }
        List<IDataStreamDefinition> OutputStreams { get; set; }
    }
}
