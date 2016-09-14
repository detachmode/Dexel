using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SharpFlowDesign.ViewModels
{
    [XmlInclude(typeof(FunctionUnit))]
    [XmlInclude(typeof(Flow))]
    [XmlInclude(typeof(FlowContainer))]
    [XmlInclude(typeof(FlowSplitter))]
    public class FlowElement
    {
       
    }

    public class Flow : FlowElement
    {
        public string Name { get; set; }


    }

    public class Column : FlowElement
    {
      
    }

    public class FlowContainer : FlowElement
    {
        public List<FlowElement> FlowElements { get; set; }
        internal FlowContainer()
        {
            FlowElements = new List<FlowElement>();
        }
    }

    public class FlowSplitter : FlowElement
    {
        public List<FlowElement> FlowElements { get; set; }
        internal FlowSplitter()
        {
            FlowElements = new List<FlowElement>();
        }
    }

    public class FunctionUnit  : FlowElement
    {
        public string Name { get; set; }

    }
}
