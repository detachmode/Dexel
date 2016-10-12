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
            var splitter= MainModelManager.AddNewSoftwareCell("Splitt Roman Numerals", mainModel);
           
            MainModelManager.AddNewInput(splitter, "RomanNumber", actionName:".test");
            var convertEach = MainModelManager.AddNewSoftwareCell("Convert to decimal", mainModel);
            MainModelManager.Connect(splitter, convertEach, "Roman Numeral*", mainModel, actionName:".eachSplitted");

            var negatelogicID = MainModelManager.AddNewSoftwareCell("Negate when larger", mainModel);
            MainModelManager.Connect(convertEach, negatelogicID, "Decimal*", mainModel);
            //var definition = DataStreamManager.CreateNewDefinition("Decimal*");
            //person.OutputStreams.Add(definition);

            return mainModel;

        }

        public static MainModel MakeRandomPerson()
        {

            var testModel = MainModel.Get();
            var first = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            first.Position = new Point(20, 50);
            MainModelManager.AddNewInput(first, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
             alter.Position = new Point(280, 50);
            MainModelManager.Connect(first, alter, "string | ", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            person.Position = new Point(540, 50);
            MainModelManager.Connect(alter, person, "int | ... string", testModel);
            var definition = DataStreamManager.CreateNewDefinition(person, "Person");
            person.OutputStreams.Add(definition);


            return testModel;

        }
        public static MainModel MakeRandomPerson2()
        {

            var testModel = MainModel.Get();
            var first = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            first.Position = new Point(20, 50);
            MainModelManager.AddNewInput(first, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            alter.Position = new Point(280, 50);
            MainModelManager.Connect(first, alter, "name:string | ", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            person.Position = new Point(540, 50);
            MainModelManager.Connect(alter, person, "age:int | age:int, name:string", testModel);
            var definition = DataStreamManager.CreateNewDefinition(person, "rndPerson:Person");
            person.OutputStreams.Add(definition);


            return testModel;

        }
    }
}
