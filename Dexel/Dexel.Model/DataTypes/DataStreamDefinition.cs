using System;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [ImplementPropertyChanged]
    public class DataStreamDefinition
    {
        public Guid ID { get; set; }
        public string ActionName { get; set; }
        public bool Connected { get; set; }
        public string DataNames { get; set; }
        public SoftwareCell Parent { get; set; }
    }
}
