using System;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [ImplementPropertyChanged]
    public class DataStreamDefinition
    {
        public Guid ID { get; internal set; }
        public string ActionName { get;  set; }
        public bool Connected { get; internal set; }
        public string DataNames { get;  set; }
        public SoftwareCell Parent { get; internal set; }


        internal DataStreamDefinition()
        {
            
        }
    }
}
