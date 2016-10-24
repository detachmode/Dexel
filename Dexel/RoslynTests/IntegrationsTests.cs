using System.Linq;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Roslyn.Tests
{
    [TestClass()]
    public class IntegrationsTests
    {
        private readonly MyGenerator _gen = new MyGenerator();

        [TestMethod()]
        public void CreateIntegrationBodyTest()
        {
            var testModel = new MainModel();
            var newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.ConnectTwoCells(newName, alter, "string", "", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.ConnectTwoCells(alter, person, "int ","int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            Integrations.CreateIntegrationBody(_gen.Generator, testModel.Connections, testModel.SoftwareCells);
        }


        [TestMethod()]
        public void FindParametersTest()
        {
            // unnamed syntax test
            var testModel = new MainModel();
            var newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.ConnectTwoCells(newName, alter, "string", "", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.ConnectTwoCells(alter, person, "int","int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            var dependecies = Integrations.FindParameters(testModel.Connections, person);
            Assert.IsTrue(dependecies.Any(x => x.Source == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source == newName));

            // ...  syntax test
            testModel = new MainModel();
            newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.ConnectTwoCells(newName, alter, "string", "", testModel);

            person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.ConnectTwoCells(alter, person, "int", "int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            dependecies = Integrations.FindParameters(testModel.Connections, person);
            Assert.IsTrue(dependecies.Any(x => x.Source == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source == newName));
        }
    }
}