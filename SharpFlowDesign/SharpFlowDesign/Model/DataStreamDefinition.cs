using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace SharpFlowDesign.Model
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
