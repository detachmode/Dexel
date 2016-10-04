using SharpFlowDesign.Model;

namespace SharpFlowDesign
{
    static class Mockdata
    {
 
        public static void RomanNumbers()
        {

            var romanNumbersConverter =  FlowDesignManager.NewSoftwareCell("Roman Numbers Converter");
            romanNumbersConverter.AddInput("RomanNumber");


            var splitter = FlowDesignManager.NewSoftwareCell("Splitt Roman Numerals");
            splitter.AddInput("RomanNumber");
            romanNumbersConverter.SetIntegration(splitter);

            var convertEach = FlowDesignManager.NewSoftwareCell("Convert to decimal");
            FlowDesignManager.Connect(splitter, convertEach, "Roman Numeral*", actionName:".eachSplitted");


            var negatelogic = FlowDesignManager.NewSoftwareCell("Negate when larger");
            FlowDesignManager.Connect(convertEach, negatelogic, "Decimal*");

            FlowDesignManager.Root = splitter;

        }
    }
}
