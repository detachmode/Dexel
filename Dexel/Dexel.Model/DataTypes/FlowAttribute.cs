
using PropertyChanged;

namespace Dexel.Model.DataTypes
{

    [ImplementPropertyChanged]
    public class FlowAttribute
    {
        public string Name { get; set; }
        public FlowAttributeType Type { get; set; }
    }

    public enum FlowAttributeType
    {
        State,
        Provider
    }
}