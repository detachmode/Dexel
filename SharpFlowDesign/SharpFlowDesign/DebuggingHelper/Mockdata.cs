using System.Windows;
using SharpFlowDesign.Model;

namespace SharpFlowDesign.DebuggingHelper
{
    public static class Mockdata
    {
 
        public static MainModel RomanNumbers()
        {

            //var romanNumbersConverter =  new SoftwareCell()#;
            //romanNumbersConverter.AddInput("RomanNumber");
            var mainModel = MainModel.Get();
            var splitterID = Interactions.AddNewSoftwareCell("Splitt Roman Numerals", mainModel);
            SoftwareCellsManager.GetFirst(splitterID,mainModel).Position = new Point(20,50);
           
            Interactions.AddNewInput(splitterID, "RomanNumber", mainModel, actionName:".test");
            var convertEachID = Interactions.AddNewSoftwareCell("Convert to decimal", mainModel);
            SoftwareCellsManager.GetFirst(convertEachID, mainModel).Position = new Point(280, 250);
            Interactions.Connect(splitterID, convertEachID, "Roman Numeral*", mainModel, actionName:".eachSplitted");

            var negatelogicID = Interactions.AddNewSoftwareCell("Negate when larger", mainModel);
            SoftwareCellsManager.GetFirst(negatelogicID, mainModel).Position = new Point(540, 50);
            Interactions.Connect(convertEachID, negatelogicID, "Decimal*", mainModel);

            return mainModel;

        }
    }
}
