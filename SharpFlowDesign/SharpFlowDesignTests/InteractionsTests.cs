using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpFlowDesign;
using SharpFlowDesign.DebuggingHelper;
using SharpFlowDesign.Model;

namespace SharpFlowDesign.Tests
{
    [TestClass()]
    public class InteractionsTests
    {
        [TestMethod()]
        public void ConnectTwoDangelingConnectionsTest()
        {
            var testModel = new MainModel();
            var sA = SoftwareCellsManager.CreateNew("A");
            var sB = SoftwareCellsManager.CreateNew("B");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);
            MainModelManager.Connect(sA.ID, sB.ID, "dataAB", testModel);

            var fristconnection = testModel.Connections.First();
            Interactions.DeConnect(fristconnection, testModel);

            var d1 = testModel.SoftwareCells.First(x => x.Name == "A").OutputStreams.First();
            Interactions.ConnectTwoDangelingConnections(d1, sA, sB, testModel);

            Assert.IsTrue(testModel.Connections.First().DataNames == "dataAB");
            Assert.IsTrue(testModel.Connections.First().Sources.First() == sA);
            Assert.IsTrue(testModel.Connections.First().Destinations.First() == sB);

            Assert.IsTrue(sA.OutputStreams.First().Connected);
            Assert.IsTrue(sB.InputStreams.First().Connected);

        }

        [TestMethod()]
        public void DeConnectTest()
        {
            var testModel = new MainModel();
            var sA = SoftwareCellsManager.CreateNew("A");
            var sB = SoftwareCellsManager.CreateNew("B");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);
            MainModelManager.Connect(sA.ID, sB.ID, "dataAB", testModel);

            var fristconnection = testModel.Connections.First();
            Interactions.DeConnect(fristconnection, testModel);

            Assert.IsTrue(testModel.Connections.Count == 0);
            Assert.IsTrue(testModel.SoftwareCells.First(x => x.Name == "A").OutputStreams.First().DataNames == "dataAB");
            Assert.IsTrue(testModel.SoftwareCells.First(x => x.Name == "B").InputStreams.First().DataNames == "dataAB");
        }

        [TestMethod()]
        public void ChangeConnectionDestinationTest()
        {

        }

        [TestMethod()]
        public void ConnectDangelingConnectionAndSoftwareCellTest()
        {
            var testModel = new MainModel();
            var sA = SoftwareCellsManager.CreateNew("A");
            var sB = SoftwareCellsManager.CreateNew("B");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);
            MainModelManager.Connect(sA.ID, sB.ID, "dataAB", testModel);

            var firstConn = testModel.Connections.First();
            Interactions.DeConnect(firstConn, testModel);

            var d1 = sA.OutputStreams.First();

            Interactions.ConnectDangelingConnectionAndSoftwareCell(d1, sA, sB, testModel);

            Assert.IsTrue(testModel.Connections.First().DataNames == "dataAB");
            Assert.IsTrue(testModel.Connections.First().Sources.First() == sA);
            Assert.IsTrue(testModel.Connections.First().Destinations.First() == sB);

            Assert.IsTrue(sA.OutputStreams.First().Connected);
            Assert.IsTrue(sA.OutputStreams.Count == 1);

            Assert.IsTrue(sB.InputStreams.First().Connected);
            Assert.IsTrue(sB.InputStreams.Count == 1);

            // Test connect to no-input cell

            testModel = new MainModel();
            sA = SoftwareCellsManager.CreateNew("A");
            sB = SoftwareCellsManager.CreateNew("B");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);

            Interactions.AddNewOutput(sA, "dataAB");
            d1 = sA.OutputStreams.First();

            Interactions.ConnectDangelingConnectionAndSoftwareCell(d1, sA, sB, testModel);

            Assert.IsTrue(testModel.Connections.First().DataNames == "dataAB");
            Assert.IsTrue(testModel.Connections.First().Sources.First() == sA);
            Assert.IsTrue(testModel.Connections.First().Destinations.First() == sB);

            Assert.IsTrue(sA.OutputStreams.First().Connected);
            Assert.IsTrue(sA.OutputStreams.Count == 1);

            Assert.IsTrue(sB.InputStreams.First().Connected);
            Assert.IsTrue(sB.InputStreams.Count == 1);




        }

        [TestMethod()]
        public void ChangeConnectionDestinationTest1()
        {
            var testModel = new MainModel();
            var sA = SoftwareCellsManager.CreateNew("A");
            var sB = SoftwareCellsManager.CreateNew("B");
            var sC = SoftwareCellsManager.CreateNew("C");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);
            testModel.SoftwareCells.Add(sC);
            MainModelManager.Connect(sA.ID, sB.ID, "dataAB", testModel);

            Interactions.ChangeConnectionDestination(testModel.Connections.First(), sC, testModel);

            Assert.IsTrue(sA.OutputStreams.Count == 1);
            Assert.IsTrue(sA.OutputStreams.First().Connected);
            Assert.IsTrue(sB.InputStreams.Count == 1);
            Assert.IsTrue(sB.InputStreams.First().Connected == false);
            Assert.IsTrue(sC.InputStreams.Count == 1);
            Assert.IsTrue(sC.InputStreams.First().Connected);

            Assert.IsTrue(testModel.Connections.Count == 1);
            Assert.IsTrue(testModel.Connections.First().Sources.First() == sA);
            Assert.IsTrue(testModel.Connections.First().Destinations.First() == sC);

        }
    }
}

namespace SharpFlowDesignTests
{
    [TestClass()]
    public class InteractionsTests
    {
        [TestMethod()]
        public void AddNewIOCellTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DragSelectionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSelectionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddNewConnectionNoDestinationTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RemoveConnectionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddNewSoftwareCellTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddNewInputTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CheckForStreamWithSameNameTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddToExistingConnectionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddNewConnectionTest()
        {
           var mainModel = new MainModel();
            var source = new SoftwareCell();
            var desination = new SoftwareCell();
            var datastream = new DataStream();

            //Interactions.AddNewConnection(source,desination, datastream, mainModel);
            //Assert.IsTrue(datastream.Sources.Contains(source));
            //Assert.IsTrue(datastream.Destinations.Contains(desination));
            //Assert.IsTrue(desination.InputStreams.Contains( datastream));
            //Assert.IsTrue(source.OutputStreams.Contains( datastream));
            //Assert.IsTrue(mainModel.Connections.Contains(datastream));
            
        }

        [TestMethod()]
        public void ConnectTest()
        {
            var mainModel = new MainModel();
            var oneID = MainModelManager.AddNewSoftwareCell("one", mainModel);
            var secondID = MainModelManager.AddNewSoftwareCell("second", mainModel);
            MainModelManager.Connect(oneID, secondID, "testdata", mainModel);

            Assert.IsTrue(mainModel.Connections.Any(x => x.DataNames.Equals("testdata")));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Sources.Any(y => y.ID.Equals(oneID))));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Destinations.Any(y => y.ID.Equals(secondID))));
        }
    }
}