using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpFlowDesign.ViewModels
{
    class FlowElement
    {
        public string Name { get; set; }

        public FlowElement(string name)
        {
            Name = name;
        }
    }

    class Flow : FlowElement
    {
        public Flow(string name) : base(name)
        {
        }
    }

    class SubFlow : FlowElement
    {
        public List<FlowElement> FlowElements { get; set; }
        public SubFlow(string name) : base(name)
        {
            FlowElements = new List<FlowElement>();
        }
    }

    class FunctionUnit : FlowElement
    {
        public FunctionUnit(string name) : base(name)
        {
        }
    }
}
