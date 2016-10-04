using PropertyChanged;

namespace SharpFlowDesign.Model
{
    [ImplementPropertyChanged]
    public class ConnectionBase
    {
        public string ActionName { get; set; }
        public string DataNames { get; set; }
    }

}