using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpFlowDesign.ViewModels
{
    class MainViewModel
    {
        public ObservableCollection<FlowElement> FlowElements { get; set; }


        public MainViewModel()
        {
            FlowElements = new ObservableCollection<FlowElement>();
            FlowElements.Add(new Flow("customer"));
            FlowElements.Add(new FunctionUnit("Is_Customer_has_credit"));
            FlowElements.Add(new Flow("Arrow"));
            FlowElements.Add(new FunctionUnit("Test"));
            var sub = new SubFlow("Subflow");
            sub.FlowElements.Add(new FunctionUnit("testsub1"));
            sub.FlowElements.Add(new FunctionUnit("testsub2"));
            FlowElements.Add(sub);

            FlowElements.Add(new Flow("()"));
        }
    }
}
