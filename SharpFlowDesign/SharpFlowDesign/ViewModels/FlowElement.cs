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
       
    }

    class Flow : FlowElement
    {
        public string Name { get; set; }
        public Flow(string name)
        {
            Name = name;
        }

    }

    class Column : FlowElement
    {
      
    }

    class FlowContainer : FlowElement
    {
        public List<FlowElement> FlowElements { get; set; }
        public FlowContainer()
        {
            FlowElements = new List<FlowElement>();
        }
    }

    class FlowSplitter : FlowElement
    {
        public List<FlowElement> FlowElements { get; set; }
        public FlowSplitter()
        {
            FlowElements = new List<FlowElement>();
        }
    }

    class FunctionUnit : FlowElement
    {
        public string Name { get; set; }
        public FunctionUnit(string name)
        {
            Name = name;
        }
    }
}
