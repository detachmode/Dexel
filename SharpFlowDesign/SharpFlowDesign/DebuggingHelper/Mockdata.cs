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
            var splitterID = MainModelManager.AddNewSoftwareCell("Splitt Roman Numerals", mainModel);
            SoftwareCellsManager.GetFirst(splitterID,mainModel).Position = new Point(20,50);
           
            Interactions.AddNewInput(splitterID, "RomanNumber", mainModel, actionName:".test");
            var convertEachID = MainModelManager.AddNewSoftwareCell("Convert to decimal", mainModel);
            SoftwareCellsManager.GetFirst(convertEachID, mainModel).Position = new Point(280, 250);
            MainModelManager.Connect(splitterID, convertEachID, "Roman Numeral*", mainModel, actionName:".eachSplitted");

            var negatelogicID = MainModelManager.AddNewSoftwareCell("Negate when larger", mainModel);
            SoftwareCellsManager.GetFirst(negatelogicID, mainModel).Position = new Point(540, 50);
            MainModelManager.Connect(convertEachID, negatelogicID, "Decimal*", mainModel);

            return mainModel;

        }
    }
}
