using System.Linq;
using System.Text.RegularExpressions;
using Dexel.Library;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Roslyn.Tests
{

    [TestClass]
    public class IntegrationsTests
    {
        private readonly MyGenerator _mygen = new MyGenerator();


        [TestMethod]
        public void FindParametersTest()
        {
            // unnamed syntax test
            var testModel = new MainModel();
            var newName = MainModelManager.AddNewFunctionUnit("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            var alter = MainModelManager.AddNewFunctionUnit("Random Age", testModel);
            MainModelManager.ConnectTwoFunctionUnits(newName, alter, "string", "", testModel);

            var person = MainModelManager.AddNewFunctionUnit("Create Person", testModel);
            MainModelManager.ConnectTwoFunctionUnits(alter, person, "int", "int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            var dependecies = IntegrationGenerator.FindParameters(person, testModel.Connections, newName);
            Assert.IsTrue(dependecies.Any(x => x.Source == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source == newName));

            // ...  syntax test
            testModel = new MainModel();
            newName = MainModelManager.AddNewFunctionUnit("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            alter = MainModelManager.AddNewFunctionUnit("Random Age", testModel);
            MainModelManager.ConnectTwoFunctionUnits(newName, alter, "string", "", testModel);

            person = MainModelManager.AddNewFunctionUnit("Create Person", testModel);
            MainModelManager.ConnectTwoFunctionUnits(alter, person, "int", "int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            dependecies = IntegrationGenerator.FindParameters(person, testModel.Connections, newName);
            Assert.IsTrue(dependecies.Any(x => x.Source == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source == newName));
        }


        [TestMethod]
        public void CreateIntegrationBodyTest()
        {
            InnerStreamOnly();
            StreamOutput();
        }


        [TestMethod]
        public void InnerStreamOnly()
        {



            var testModel = new MainModel();
            var x = MainModelManager.AddNewFunctionUnit("X", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "(int)");

            var createPersons = MainModelManager.AddNewFunctionUnit("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            MainModelManager.AddNewOutput(createPersons, "(Person)*");

            var addAge = MainModelManager.AddNewFunctionUnit("Add Age", testModel);
            MainModelManager.AddNewInput(addAge, "(Person)*");
            MainModelManager.AddNewOutput(addAge, "()");

            var addName = MainModelManager.AddNewFunctionUnit("Add Name", testModel);
            MainModelManager.AddNewInput(addName, "(Person)*");
            MainModelManager.AddNewOutput(addName, "()");

            var sumAges = MainModelManager.AddNewFunctionUnit("Sum Ages", testModel);
            MainModelManager.AddNewInput(sumAges, "(Person)*");
            MainModelManager.AddNewOutput(sumAges, "(int)");


            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams.First(),
                addAge.InputStreams.First(), testModel);

            MainModelManager.ConnectTwoDefintions(addAge.OutputStreams.First(),
                addName.InputStreams.First(), testModel);

            x.Integration.AddUnique(createPersons);
            x.Integration.AddUnique(addAge);
            x.Integration.AddUnique(addName);
            x.Integration.AddUnique(sumAges);

            var res = IntegrationGenerator.CreateIntegrationBody(_mygen.Generator, testModel.Connections, x);
            var formatted = _mygen.CompileToString(res.ToList());

            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons.*AddAge.*", RegexOptions.Singleline));

            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons\(person =>.*\S* AddAge\(person\);.*AddName\(person\);.*",
                    RegexOptions.Singleline));

            Assert.IsTrue(
               Regex.IsMatch(
                   formatted,
                   @".*CreatePersons\(person =>.*\S* AddAge\(person\);.*AddName\(person\);.*AddName\(person\);.*",
                   RegexOptions.Singleline));
        }

        [TestMethod]
        public void StreamOutput()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewFunctionUnit("X", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "(onPerson:Person)*");

            var createPersons = MainModelManager.AddNewFunctionUnit("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            MainModelManager.AddNewOutput(createPersons, "(Person)*");

            var addAge = MainModelManager.AddNewFunctionUnit("Add Age", testModel);
            MainModelManager.AddNewInput(addAge, "(Person)*");
            MainModelManager.AddNewOutput(addAge, "(Person)*");


            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams.First(),
                addAge.InputStreams.First(), testModel);

            x.Integration.Add(createPersons);
            x.Integration.Add(addAge);


            var res = IntegrationGenerator.CreateIntegrationBody(_mygen.Generator, testModel.Connections, x);
            var formatted = _mygen.CompileToString(res.ToList());

            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons.*AddAge.*", RegexOptions.Singleline));

            Assert.IsTrue(
               Regex.IsMatch(
                   formatted,
                   @".*CreatePersons\(person =>.*\S* aPerson = AddAge\(person\);.*",
                   RegexOptions.Singleline));

            // finds matching outgoing Action of Integration
            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons\(person =>.*\S* aPerson = AddAge\(person\);.*onPerson\(aPerson\);.*",
                    RegexOptions.Singleline));
        }
    }

}