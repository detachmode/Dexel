using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class Stream
    {
        public string Datanames { get; set; }
        public string Actionname { get; set; }
    }
}
