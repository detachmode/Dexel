using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowDesignModel;

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
            MainModelManager.Connect(newName, alter, "string | ", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.Connect(alter, person, "int | int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            var nodes = Integrations.CreateIntegrationBody(_gen.Generator, testModel.Connections, testModel.SoftwareCells);
        }

        [TestMethod()]
        public void FindParameterDependenciesTest()
        {
           
        }

        [TestMethod()]
        public void FindParametersTest()
        {
            // unnamed syntax test
            var testModel = new MainModel();
            var newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.Connect(newName, alter, "string | ", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.Connect(alter, person, "int | int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            var dependecies = Integrations.FindParameters(testModel.Connections, person);
            Assert.IsTrue(dependecies.Any(x => x.Source == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source == newName));

            // ...  syntax test
            testModel = new MainModel();
            newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.Connect(newName, alter, "string | ", testModel);

            person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.Connect(alter, person, "int | ... string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            dependecies = Integrations.FindParameters(testModel.Connections, person);
            Assert.IsTrue(dependecies.Any(x => x.Source == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source == newName));
        }
    }
}