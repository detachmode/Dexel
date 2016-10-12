using System.Windows;

namespace FlowDesignModel
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
           
            MainModelManager.AddNewInput(splitterID, "RomanNumber", mainModel, actionName:".test");
            var convertEachID = MainModelManager.AddNewSoftwareCell("Convert to decimal", mainModel);
            SoftwareCellsManager.GetFirst(convertEachID, mainModel).Position = new Point(280, 250);
            MainModelManager.Connect(splitterID, convertEachID, "Roman Numeral*", mainModel, actionName:".eachSplitted");

            var negatelogicID = MainModelManager.AddNewSoftwareCell("Negate when larger", mainModel);
            SoftwareCellsManager.GetFirst(negatelogicID, mainModel).Position = new Point(540, 50);
            MainModelManager.Connect(convertEachID, negatelogicID, "Decimal*", mainModel);
            //var definition = DataStreamManager.CreateNewDefinition("Decimal*");
            //person.OutputStreams.Add(definition);

            return mainModel;

        }

        public static MainModel MakeRandomPerson()
        {

            var testModel = MainModel.Get();
            var firstID = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            SoftwareCellsManager.GetFirst(firstID, testModel).Position = new Point(20, 50);
            MainModelManager.AddNewInput(firstID, "", testModel);

            var alterID = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            SoftwareCellsManager.GetFirst(alterID, testModel).Position = new Point(280, 50);
            MainModelManager.Connect(firstID, alterID, "string | ", testModel);

            var personID = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            SoftwareCellsManager.GetFirst(personID, testModel).Position = new Point(540, 50);
            MainModelManager.Connect(alterID, personID, "int | ... string", testModel);
            var person = SoftwareCellsManager.GetFirst(personID, testModel);
            var definition = DataStreamManager.CreateNewDefinition(person, "Person");
            person.OutputStreams.Add(definition);


            return testModel;

        }
        public static MainModel MakeRandomPerson2()
        {

            var testModel = MainModel.Get();
            var firstID = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            SoftwareCellsManager.GetFirst(firstID, testModel).Position = new Point(20, 50);
            MainModelManager.AddNewInput(firstID, "", testModel);

            var alterID = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            SoftwareCellsManager.GetFirst(alterID, testModel).Position = new Point(280, 50);
            MainModelManager.Connect(firstID, alterID, "name:string | ", testModel);

            var personID = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            SoftwareCellsManager.GetFirst(personID, testModel).Position = new Point(540, 50);
            MainModelManager.Connect(alterID, personID, "age:int | age:int, name:string", testModel);
            var person = SoftwareCellsManager.GetFirst(personID, testModel);
            var definition = DataStreamManager.CreateNewDefinition(person, "rndPerson:Person");
            person.OutputStreams.Add(definition);


            return testModel;

        }
    }
}
