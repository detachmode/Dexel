using SharpFlowDesign.Model;

namespace SharpFlowDesign
{
    public static class Mockdata
    {
 
        public static MainModel RomanNumbers()
        {

            //var romanNumbersConverter =  new SoftwareCell()#;
            //romanNumbersConverter.AddInput("RomanNumber");
            var mainModel = MainModel.Get();
            var splitterID = Interactions.AddNewSoftwareCell("Splitt Roman Numerals", mainModel);
            Interactions.AddNewInput(splitterID, "RomanNumber", mainModel);

            var convertEachID = Interactions.AddNewSoftwareCell("Convert to decimal", mainModel);
            Interactions.Connect(splitterID, convertEachID, "Roman Numeral*", mainModel, actionName:".eachSplitted");

            var negatelogicID = Interactions.AddNewSoftwareCell("Negate when larger", mainModel);
            Interactions.Connect(convertEachID, negatelogicID, "Decimal*", mainModel);

            return mainModel;

        }
    }
}
