using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFlowDesign.UserControls;
using SharpFlowDesign.XML;

namespace SharpFlowDesign.ViewModels
{
    class MainViewModel
    {

        public FlowContainer root { get; set; }


        public MainViewModel()
        {
            root = new FlowContainer();
            root.FlowElements.Add(new FunctionUnit {Name="Test"});
            root.FlowElements.Add(new FunctionUnit{Name = "Is_Customer_has_credit"});
            root.FlowElements.Add(new Flow{Name = "Arrow"});
            //root.FlowElements.Add(new Column());
            var split = new FlowSplitter();
            var flow1 = new FlowContainer();

            flow1.FlowElements.Add(new Flow {Name = "string"});
            flow1.FlowElements.Add(new FunctionUnit {Name="Test"});
            flow1.FlowElements.Add(new Flow {Name = "string"});
            //var flow2 = new FlowContainer();
            //flow2.FlowElements.Add(new Flow("string"));
            //flow2.FlowElements.Add(new FunctionUnit("testsub2"));
            //flow2.FlowElements.Add(new Flow("string"));
            split.FlowElements.Add((flow1));
            //split.FlowElements.Add((flow2));
            root.FlowElements.Add(split);

            //root.FlowElements.Add(new Flow("()"));

            var converter = new XmlConverter();
            var xml = converter.ConvertObject(root);
            Debug.Write(xml);
        }

    }
}
