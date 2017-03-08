using System.Collections.Generic;
using System.Windows;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;

namespace CodeAnalyser
{
    public static class CSharpToFlowDesign
    {
        // TODO: add references of roslyn for C# code analyse features.

        public static MainModel FromFile(string filename)
        {
            // TODO: currently just ignores file (filename) and just returns an example MainModel (Flow Design)

            // MainModel stores whole state of Flow Design diagramm in it.
            var mainModel = new MainModel();

            // Make first Function Unit
            var first = MainModelManager.AddNewFunctionUnit("Ignores the file", mainModel);
            first.Position = new Point(80, 70);
            MainModelManager.AddNewInput(first, "()");
                // if we don't want to connect this input with something, we don't need to save the returned value (DataStreamDefinition)
            var firstoutput = MainModelManager.AddNewOutput(first, "(age:int)");
                // here we save the returned value so we can connect it later with another DataStreamDefinition (Inputs or Outputs)

            // Create a new function unit and add it to the first function unit as Operation
            var operation1 = MainModelManager.AddNewFunctionUnit("bla lalal", mainModel);
            operation1.Position = new Point(80, 180);
            MainModelManager.AddNewInput(operation1, "()");
            MainModelManager.AddNewOutput(operation1, "(age:int)");
            first.IsIntegrating.Add(operation1);
                // here we make the first function unit to an integration of the operation1


            var personCreator = MainModelManager.AddNewFunctionUnit("create person", mainModel);
            personCreator.Position = new Point(450, 50);
            var personCreatorInput = MainModelManager.AddNewInput(personCreator, "(age:int, name:string)");
            MainModelManager.AddNewOutput(personCreator, "(rndPerson:Person)");

            // connect an output with an input of another function unit
            MainModelManager.ConnectTwoDefintions(firstoutput, personCreatorInput, mainModel);

            // custom data types on the left side of the UI
            var inttype = new SubDataType {Name = "age", Type = "int"};
            var nametype = new SubDataType {Name = "name", Type = "string"};
            var sublist = new List<SubDataType> {inttype, nametype};
            mainModel.DataTypes.Add(new CustomDataType {Name = "Person", SubDataTypes = sublist});

            return mainModel; // returns the mainModel so the UI can show it
        }
    }
}