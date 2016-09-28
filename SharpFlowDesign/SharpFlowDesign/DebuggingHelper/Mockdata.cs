using SharpFlowDesign.Model;

namespace SharpFlowDesign
{
    static class Mockdata
    {
        public static void Print()
        {
            //var f = new SoftwareCell {Name = "F"};
            //f.AddInput("csv");        
            //f.AddOutput("", actionName: ".OnResult", optional: true);

            //var g = new SoftwareCell {Name = "G"};
            //var h = new SoftwareCell { Name = "H" };
            //var j = new SoftwareCell { Name = "J" };


            //f.Connect(j,"", actionName:".OnResult", optional:true);
            //f.Connect(g, "number");          
            //f.Connect(h, "number");
            //f.PrintOutputs();



            //var op = new SoftwareCell {Name = "Operation"};
            //op.AddInput("csv");
            //op.AddOutput("int", actionName:"onResult");
            //f.SetIntegration(op);
            //f.PrintIntegration();






        }


        public static SoftwareCell RomanNumbers()
        {
            var romanNumbersConverter = new SoftwareCell { Name = "Roman Numbers Converter" };
            romanNumbersConverter.AddInput("RomanNumber");

            var splitter = new SoftwareCell { Name = "Splitt Roman Numerals" };
            splitter.AddInput("RomanNumber");
            romanNumbersConverter.SetIntegration(splitter);

            var convertEach = new SoftwareCell { Name = "Convert to decimal" };
            splitter.Connect(convertEach, "Roman Numeral*", actionName:".eachSplitted");


            var negatelogic = new SoftwareCell { Name = "Negate when larger" };
            convertEach.Connect(negatelogic, "Decimal*");



            return splitter;
        }
    }
}
