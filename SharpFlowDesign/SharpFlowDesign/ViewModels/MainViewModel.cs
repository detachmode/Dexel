using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFlowDesign.UserControls;

namespace SharpFlowDesign.ViewModels
{
    class MainViewModel
    {

        public FlowContainer root { get; set; }


        public MainViewModel()
        {
            root = new FlowContainer();
            //root.FlowElements.Add(new Flow(""));
            //root.FlowElements.Add(new FunctionUnit(""));
            //root.FlowElements.Add(new Flow(""));
            root.FlowElements.Add(new Flow("customer"));
            root.FlowElements.Add(new FunctionUnit("Is_Customer_has_credit"));
            root.FlowElements.Add(new Flow("Arrow"));
            root.FlowElements.Add(new FunctionUnit("Test"));
            root.FlowElements.Add(new Flow(""));
            //root.FlowElements.Add(new Column());
            var split = new FlowSplitter();
            var flow1 = new FlowContainer();

            flow1.FlowElements.Add(new Flow("string"));
            flow1.FlowElements.Add(new FunctionUnit("testsub1"));
            flow1.FlowElements.Add(new Flow("string"));
            var flow2 = new FlowContainer();
            flow2.FlowElements.Add(new Flow("string"));
            flow2.FlowElements.Add(new FunctionUnit("testsub2"));
            flow2.FlowElements.Add(new Flow("string"));
            split.FlowElements.Add((flow1));
            split.FlowElements.Add((flow2));
            root.FlowElements.Add(split);

            root.FlowElements.Add(new Flow("()"));
        }

    }
}
