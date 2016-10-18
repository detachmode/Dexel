using System;
using Dexel.Contracts.Model;
using PropertyChanged;

namespace Dexel.Model
{
    [ImplementPropertyChanged]
    public class DataStreamDefinition : IDataStreamDefinition
    {
        public bool Connected { get; set; }
        public Guid ID { get; set; }
        public string ActionName { get; set; }
        public string DataNames { get; set; }
        public ISoftwareCell Parent { get; set; }
    }
}
