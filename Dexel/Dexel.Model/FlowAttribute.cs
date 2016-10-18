using Dexel.Contracts.Model;

namespace Dexel.Model
{

    public class FlowAttribute : IFlowAttribute
    {
        public string Name { get; set; }
        public FlowAttributeType Type { get; set; }
    }
}