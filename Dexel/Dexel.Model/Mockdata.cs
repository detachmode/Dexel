using System.Windows;
using Dexel.Model.DataTypes;

namespace Dexel.Model
{
    public static class Mockdata
    {

        public static MainModel RomanNumbers()
        {

            var mainModel = new MainModel();
            var splitter= MainModelManager.AddNewSoftwareCell("Splitt Roman Numerals", mainModel);

            MainModelManager.AddNewInput(splitter, "RomanNumber", actionName:".test");
            var convertEach = MainModelManager.AddNewSoftwareCell("Convert to decimal", mainModel);
            MainModelManager.ConnectTwoCells(splitter, convertEach, "Roman Numeral*", "Roman Numeral*", mainModel, actionName:".eachSplitted");

            var negatelogicID = MainModelManager.AddNewSoftwareCell("Negate when larger", mainModel);
            MainModelManager.ConnectTwoCells(convertEach, negatelogicID, "Decimal*", "Decimal*", mainModel);


            return mainModel;

        }

        public static MainModel MakeRandomPerson()
        {

            var testModel = new MainModel();
            var first = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            first.Position = new Point(20, 50);
            MainModelManager.AddNewInput(first, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
             alter.Position = new Point(280, 50);
            MainModelManager.ConnectTwoCells(first, alter, "string", "", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            person.Position = new Point(540, 50);
            MainModelManager.ConnectTwoCells(alter, person, "int ","int, string", testModel);
            var definition = DataStreamManager.NewDefinition(person, "Person");
            person.OutputStreams.Add(definition);


            return testModel;

        }
        public static MainModel MakeRandomPerson2()
        {

            var testModel = new MainModel();
            var first = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            first.Position = new Point(20, 50);
            MainModelManager.AddNewInput(first, "");

            var firstOp = MainModelManager.AddNewSoftwareCell("Operation", testModel);
            firstOp.Position = new Point(60, 100);
            MainModelManager.AddNewInput(firstOp, "");
           first.Integration.Add(firstOp);
            var firstOp2 = MainModelManager.AddNewSoftwareCell("Operation", testModel);
            firstOp2.Position = new Point(160, 100);
            MainModelManager.AddNewInput(firstOp2, "");
            first.Integration.Add(firstOp2);

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            alter.Position = new Point(280, 50);
            MainModelManager.ConnectTwoCells(first, alter, "name:string", "", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            person.Position = new Point(540, 50);
            MainModelManager.ConnectTwoCells(alter, person, "age:int ","age:int, name:string", testModel);
            var definition = DataStreamManager.NewDefinition(person, "rndPerson:Person");
            person.OutputStreams.Add(definition);


            return testModel;

        }
    }
}
