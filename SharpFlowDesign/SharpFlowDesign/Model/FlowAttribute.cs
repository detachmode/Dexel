namespace SharpFlowDesign.Model
{

    public enum FlowAttributeType
    {
        Provider,
        State
    }

    public class FlowAttribute
    {
        public string Name { get; set; }
        public FlowAttributeType Type { get; set; }
    }
}