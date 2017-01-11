using System.Collections.Generic;
using Dexel.Editor;
using System.Linq;
using Dexel.Editor.Views;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dexel.Editor.Tests
{

    [TestClass]
    public class InteractionsTests
    {

        [TestMethod]
        public void ConnectTwoDangelingConnectionsTest()
        {
            var testModel = new MainModel();
            var sA = FunctionUnitManager.CreateNew("A");
            var sB = FunctionUnitManager.CreateNew("B");
            testModel.FunctionUnits.Add(sA);
            testModel.FunctionUnits.Add(sB);

            Interactions.AddNewOutput(sA, "dataA");
            Interactions.AddNewInput(sB, "");


            Interactions.DragDroppedTwoDangelingConnections(sA.OutputStreams.First(), sB.InputStreams.First(), testModel);

            Assert.IsTrue(testModel.Connections.First().DataNames == "dataA | ");
            Assert.IsTrue(testModel.Connections.First().Sources.First().Parent == sA);
            Assert.IsTrue(testModel.Connections.First().Destinations.First().Parent == sB);

            Assert.IsTrue(sA.OutputStreams.First().Connected);
            Assert.IsTrue(sB.InputStreams.First().Connected);
        }


        [TestMethod]
        public void DeConnectTest()
        {
            var testModel = new MainModel();
            var sA = FunctionUnitManager.CreateNew("A");
            var sB = FunctionUnitManager.CreateNew("B");
            testModel.FunctionUnits.Add(sA);
            testModel.FunctionUnits.Add(sB);
            MainModelManager.ConnectTwoFunctionUnits(sA, sB, "(dataAB)", "(dataAB)", testModel);

            var fristconnection = testModel.Connections.First();
            Interactions.DeConnect(fristconnection, testModel);

            Assert.AreEqual(0, testModel.Connections.Count);
            Assert.AreEqual("(dataAB)", testModel.FunctionUnits.First(x => x.Name == "A").OutputStreams.First().DataNames);
            Assert.AreEqual("(dataAB)", testModel.FunctionUnits.First(x => x.Name == "B").InputStreams.First().DataNames);


            testModel = new MainModel();
            sA = FunctionUnitManager.CreateNew("A");
            testModel.FunctionUnits.Add(sA);
            Interactions.AddNewOutput(sA, "(dataA)");

            sB = FunctionUnitManager.CreateNew("B");
            testModel.FunctionUnits.Add(sB);

            Interactions.ConnectDangelingConnectionAndFunctionUnit(sA.OutputStreams.First(), sB, testModel);

            Interactions.DeConnect(testModel.Connections.First(), testModel);

            Assert.AreEqual(0, testModel.Connections.Count);
            Assert.AreEqual("dataA", sA.OutputStreams.First().DataNames);
            Assert.AreEqual(false, sA.OutputStreams.First().Connected);

            Assert.AreEqual("",sB.InputStreams.First().DataNames);
            Assert.AreEqual(false, sB.InputStreams.First().Connected);
        }


        [TestMethod]
        public void ChangeConnectionDestinationTest()
        {
            var testModel = new MainModel();

            var sA = FunctionUnitManager.CreateNew("A", "()" , "(dataAB)");
            var sB = FunctionUnitManager.CreateNew("B");
            var sC = FunctionUnitManager.CreateNew("C");
            testModel.FunctionUnits.Add(sA);
            testModel.FunctionUnits.Add(sB);
            testModel.FunctionUnits.Add(sC);
            MainModelManager.ConnectTwoFunctionUnits(sA, sB, "dataAB", "dataAB", testModel);

            Interactions.ChangeConnectionDestination(testModel.Connections.First(), sC, testModel);

            Assert.IsTrue(sA.OutputStreams.Count == 1);
            Assert.IsTrue(sA.OutputStreams.First().Connected);
            Assert.IsTrue(sB.InputStreams.Count == 1);
            Assert.IsTrue(sB.InputStreams.First().Connected == false);
            Assert.IsTrue(sC.InputStreams.Count == 1);
            Assert.IsTrue(sC.InputStreams.First().Connected);

            Assert.IsTrue(testModel.Connections.Count == 1);
            Assert.IsTrue(testModel.Connections.First().Sources.First().Parent == sA);
            Assert.IsTrue(testModel.Connections.First().Destinations.First().Parent == sC);
        }


        [TestMethod]
        public void ConnectTest()
        {
            var mainModel = new MainModel();
            var oneFu = MainModelManager.AddNewFunctionUnit("one", mainModel);
            var secondFu = MainModelManager.AddNewFunctionUnit("second", mainModel);
            MainModelManager.ConnectTwoFunctionUnits(oneFu, secondFu, "testdata", "testdata", mainModel);

            Assert.IsTrue(mainModel.Connections.Any(x => x.DataNames.Equals("testdata")));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Sources.Any(dsd => dsd.Parent == oneFu)));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Destinations.Any(dsd => dsd.Parent == secondFu)));
        }


        [TestMethod]
        public void ConnectDangelingConnectionAndFunctionUnitTest()
        {

            var mainModel = new MainModel();
            var oneFu = MainModelManager.AddNewFunctionUnit("one", mainModel);
            Interactions.AddNewOutput(oneFu, "(testdata)");

            var secondFu = MainModelManager.AddNewFunctionUnit("second", mainModel);
            MainModelManager.AddNewInput(secondFu, "()");

            Interactions.ConnectDangelingConnectionAndFunctionUnit(oneFu.OutputStreams.First(), secondFu, mainModel);

            Assert.AreEqual("(testdata) | ()", mainModel.Connections.First().DataNames);
            Assert.AreEqual(oneFu, mainModel.Connections.First().Sources.First().Parent);
            Assert.AreEqual(secondFu, mainModel.Connections.First().Destinations.First().Parent);

            Assert.AreEqual("(testdata)", oneFu.OutputStreams.First().DataNames);
            Assert.AreEqual(true, oneFu.OutputStreams.First().Connected);

            Assert.AreEqual(true, secondFu.InputStreams.First().Connected);
            Assert.AreEqual("()", secondFu.InputStreams.First().DataNames);
        }


        [TestMethod()]
        public void DeleteTest()
        {
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("Main", testModel);

            var firstOp = MainModelManager.AddNewFunctionUnit("Operation 1", testModel);
            var secondOp = MainModelManager.AddNewFunctionUnit("Operation 2", testModel);
            var thirdOp = MainModelManager.AddNewFunctionUnit("Operation 3", testModel);
            main.IsIntegrating.Add(firstOp);
            MainModelManager.ConnectTwoFunctionUnits(firstOp, secondOp, "", "", testModel);
            MainModelManager.ConnectTwoFunctionUnits(secondOp, thirdOp, "", "", testModel);

            var selected = new List<FunctionUnit> { thirdOp };
            Interactions.Delete(selected, testModel);
            Assert.AreEqual(3, testModel.FunctionUnits.Count);
            Assert.AreEqual(2, main.IsIntegrating.Count);
            CollectionAssert.Contains(main.IsIntegrating, secondOp);
            CollectionAssert.Contains(main.IsIntegrating, firstOp);

            selected = new List<FunctionUnit> { firstOp };
            Interactions.Delete(selected, testModel);
            Assert.AreEqual(2, testModel.FunctionUnits.Count);
            Assert.AreEqual(1, main.IsIntegrating.Count);
            Assert.AreEqual(secondOp.Name, main.IsIntegrating.First().Name);
        }

      
    }

}