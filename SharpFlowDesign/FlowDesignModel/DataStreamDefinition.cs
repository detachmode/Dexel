using System;
using PropertyChanged;

namespace FlowDesignModel
{
    [ImplementPropertyChanged]
    public class DataStreamDefinition
    {
        public Guid ID;
        public bool Connected { get; set; }
        public string ActionName { get; set; }
        public string DataNames { get; set; }

    }
}
