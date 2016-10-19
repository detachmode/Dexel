using System.Linq;
using Dexel.Editor;
using Dexel.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dexel.Tests
{
    [TestClass()]
    public class InteractionsTests
    {
        private static DataStreamManager _dataStreamManager = new DataStreamManager();
        private static SoftwareCellsManager _softwareCellsManager = new SoftwareCellsManager();
        private static MainModelManager _mainModelManager = new MainModelManager(_softwareCellsManager, _dataStreamManager);



        [TestMethod()]
        public void ConnectTwoDangelingConnectionsTest()
        {
            var testModel = new MainModel();
            var sA = _softwareCellsManager.CreateNew("A");
            var sB = _softwareCellsManager.CreateNew("B");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);
            _mainModelManager.Connect(sA, sB, "dataAB", testModel);

            var fristconnection = testModel.Connections.First();
            Interactions.DeConnect(fristconnection, testModel);

            var d1 = testModel.SoftwareCells.First(x => x.Name == "A").OutputStreams.First();
            Interactions.ConnectTwoDangelingConnections(d1, sA, sB, testModel);

            Assert.IsTrue(testModel.Connections.First().DataNames == "dataAB");
            Assert.IsTrue(testModel.Connections.First().Sources.First().Parent == sA);
            Assert.IsTrue(testModel.Connections.First().Destinations.First().Parent== sB);

            Assert.IsTrue(sA.OutputStreams.First().Connected);
            Assert.IsTrue(sB.InputStreams.First().Connected);

        }

        [TestMethod()]
        public void DeConnectTest()
        {
            var testModel = new MainModel();
            var sA = _softwareCellsManager.CreateNew("A");
            var sB = _softwareCellsManager.CreateNew("B");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);
            _mainModelManager.Connect(sA, sB, "dataAB", testModel);

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
            var sA = _softwareCellsManager.CreateNew("A");
            var sB = _softwareCellsManager.CreateNew("B");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);
            _mainModelManager.Connect(sA, sB, "dataAB", testModel);

            var firstConn = testModel.Connections.First();
            Interactions.DeConnect(firstConn, testModel);

            var d1 = sA.OutputStreams.First();

            Interactions.ConnectDangelingConnectionAndSoftwareCell(d1, sA, sB, testModel);

            Assert.IsTrue(testModel.Connections.First().DataNames == "dataAB");
            Assert.IsTrue(testModel.Connections.First().Sources.First().Parent== sA);
            Assert.IsTrue(testModel.Connections.First().Destinations.First().Parent == sB);

            Assert.IsTrue(sA.OutputStreams.First().Connected);
            Assert.IsTrue(sA.OutputStreams.Count == 1);

            Assert.IsTrue(sB.InputStreams.First().Connected);
            Assert.IsTrue(sB.InputStreams.Count == 1);

            // Test connect to no-input cell

            testModel = new MainModel();
            sA = _softwareCellsManager.CreateNew("A");
            sB = _softwareCellsManager.CreateNew("B");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);

            _mainModelManager.AddNewOutput(sA, "dataAB");
            d1 = sA.OutputStreams.First();

            Interactions.ConnectDangelingConnectionAndSoftwareCell(d1, sA, sB, testModel);

            Assert.IsTrue(testModel.Connections.First().DataNames == "dataAB");
            Assert.IsTrue(testModel.Connections.First().Sources.First().Parent == sA);
            Assert.IsTrue(testModel.Connections.First().Destinations.First().Parent == sB);

            Assert.IsTrue(sA.OutputStreams.First().Connected);
            Assert.IsTrue(sA.OutputStreams.Count == 1);

            Assert.IsTrue(sB.InputStreams.First().Connected);
            Assert.IsTrue(sB.InputStreams.Count == 1);




        }

        [TestMethod()]
        public void ChangeConnectionDestinationTest1()
        {
            var testModel = new MainModel();

            var sA = _softwareCellsManager.CreateNew("A");
            var sB = _softwareCellsManager.CreateNew("B");
            var sC = _softwareCellsManager.CreateNew("C");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);
            testModel.SoftwareCells.Add(sC);
            _mainModelManager.Connect(sA, sB, "dataAB", testModel);

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

        [TestMethod()]
        public void ConnectTest()
        {
            var mainModel = new MainModel();
            var oneCell = _mainModelManager.AddNewSoftwareCell("one", mainModel);
            var secondCell = _mainModelManager.AddNewSoftwareCell("second", mainModel);
            _mainModelManager.Connect(oneCell, secondCell, "testdata", mainModel);

            Assert.IsTrue(mainModel.Connections.Any(x => x.DataNames.Equals("testdata")));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Sources.Any(dsd => dsd.Parent == oneCell)));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Destinations.Any(dsd => dsd.Parent == secondCell)));
        }
    }
}

