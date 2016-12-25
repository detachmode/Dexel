using System.Linq;
using System.Text.RegularExpressions;
using Dexel.Library;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
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
            Assert.IsTrue(dependecies.Any(x => x.Source.Parent == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source.Parent == newName));

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
            Assert.IsTrue(dependecies.Any(x => x.Source.Parent == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source.Parent == newName));
        }

        [TestMethod]
        public void FindParameters_From_Action_DSD()
        {
            // unnamed syntax test
            var testModel = new MainModel();

            var person = MainModelManager.AddNewFunctionUnit("Create Person", testModel);
            MainModelManager.AddNewInput(person, "()");
            var outperson = MainModelManager.AddNewOutput(person, "(Person)", actionName: "onPerson");


            var addage = MainModelManager.AddNewFunctionUnit("Add Age", testModel);
            var inAge = MainModelManager.AddNewInput(addage, "(Person)");
            MainModelManager.AddNewOutput(addage, "(Person)");

            MainModelManager.ConnectTwoDefintions(outperson, inAge, testModel);

            var dependecies = IntegrationGenerator.FindParameters(addage, testModel.Connections, null);
            var firstdep = dependecies.First();
            Assert.AreEqual(outperson, firstdep.Source);

        }
        [TestMethod]
        public void FindParameters_From_Parent()
        {
            // unnamed syntax test
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            var countinParent = MainModelManager.AddNewInput(main, "(count:int)");
            MainModelManager.AddNewOutput(main, "()");

            var person = MainModelManager.AddNewFunctionUnit("Create Person", testModel);
            MainModelManager.AddNewInput(person, "(count:int)");
            //var outperson = MainModelManager.AddNewOutput(person, "(Person)", actionName:"onPerson");
            main.IsIntegrating.Add(person);

            var dependecies = IntegrationGenerator.FindParameters(person, testModel.Connections, main);
            var firstdep = dependecies.First();
            Assert.AreEqual(countinParent, firstdep.Source);

        }

        [TestMethod]
        public void FindParameters_NoDependecies()
        {
            // unnamed syntax test
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            var countinParent = MainModelManager.AddNewInput(main, "(count:int)");
            MainModelManager.AddNewOutput(main, "()");

            var person = MainModelManager.AddNewFunctionUnit("Create Person", testModel);
            MainModelManager.AddNewInput(person, "()");
            var outperson = MainModelManager.AddNewOutput(person, "(Person)", actionName: "onPerson");
            main.IsIntegrating.Add(person);

            var dependecies = IntegrationGenerator.FindParameters(person, testModel.Connections, main);
            Assert.AreEqual(0, dependecies.Count);

        }

        [TestMethod]
        public void TwoOptionalOutputs()
        {

            var testModel = new MainModel();
            var x = MainModelManager.AddNewFunctionUnit("X", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "()");

            var createPersons = MainModelManager.AddNewFunctionUnit("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            MainModelManager.AddNewOutput(createPersons, "(Person)*", actionName:"onPerson");

            var checkage = MainModelManager.AddNewFunctionUnit("Check Age", testModel);
            MainModelManager.AddNewInput(checkage, "(Person)*");
           var onadult = MainModelManager.AddNewOutput(checkage, "(Person)*", actionName:"onAdult");
            var onchild = MainModelManager.AddNewOutput(checkage, "(Person)*", actionName:"onChild");

            var print = MainModelManager.AddNewFunctionUnit("Print", testModel);
            MainModelManager.AddNewInput(print, "(Person)*");
            MainModelManager.AddNewOutput(print, "()");

            var printchild = MainModelManager.AddNewFunctionUnit("Print", testModel);
            MainModelManager.AddNewInput(printchild, "(Person)*");
            MainModelManager.AddNewOutput(printchild, "()");




            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams.First(),
                checkage.InputStreams.First(), testModel);

            MainModelManager.ConnectTwoDefintions(onadult,
                print.InputStreams.First(), testModel);

            MainModelManager.ConnectTwoDefintions(onchild,
                printchild.InputStreams.First(), testModel);

            x.IsIntegrating.AddUnique(createPersons);
            x.IsIntegrating.AddUnique(checkage);
            x.IsIntegrating.AddUnique(print);
            x.IsIntegrating.AddUnique(printchild);

            var res = IntegrationGenerator.CreateIntegrationBody(_mygen.Generator, testModel, x);
            var formatted = _mygen.CompileToString(res.ToList());


            // finds matching outgoing Action of IsIntegrating
            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons.*person.*CheckAge\(person, person2.*Print\(person2\);.*person3.*Print\(person3\);.*",
                    RegexOptions.Singleline));
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

            MainModelManager.ConnectTwoDefintions(addName.OutputStreams.First(),
    sumAges.InputStreams.First(), testModel);

            x.IsIntegrating.AddUnique(createPersons);
            x.IsIntegrating.AddUnique(addAge);
            x.IsIntegrating.AddUnique(addName);
            x.IsIntegrating.AddUnique(sumAges);

            var res = IntegrationGenerator.CreateIntegrationBody(_mygen.Generator, testModel, x);
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
                   @".*CreatePersons\(person =>.*\S* AddAge\(person\);.*AddName\(person\);.*SumAges\(person\);.*",
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

            x.IsIntegrating.Add(createPersons);
            x.IsIntegrating.Add(addAge);


            var res = IntegrationGenerator.CreateIntegrationBody(_mygen.Generator, testModel, x);
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

            // finds matching outgoing Action of IsIntegrating
            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons\(person =>.*\S* aPerson = AddAge\(person\);.*onPerson\(aPerson\);.*",
                    RegexOptions.Singleline));
        }
    }

}