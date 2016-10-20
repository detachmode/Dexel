using System.Linq;
using Dexel.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dexel.Editor.Tests
{

    [TestClass]
    public class InteractionsTests
    {
        private static readonly DataStreamManager DataStreamManager = new DataStreamManager();
        private static readonly SoftwareCellsManager SoftwareCellsManager = new SoftwareCellsManager();

        private static readonly MainModelManager MainModelManager = new MainModelManager(SoftwareCellsManager,
            DataStreamManager);


        [TestMethod]
        public void ConnectTwoDangelingConnectionsTest()
        {
            var testModel = new MainModel();
            var sA = SoftwareCellsManager.CreateNew("A");
            var sB = SoftwareCellsManager.CreateNew("B");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);

            Interactions.AddNewOutput(sA, "dataA");
            Interactions.AddNewInput(sB, "");


            Interactions.ConnectTwoDangelingConnections(sA.OutputStreams.First(), sB.InputStreams.First(), testModel);

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
            var sA = SoftwareCellsManager.CreateNew("A");
            var sB = SoftwareCellsManager.CreateNew("B");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);
            MainModelManager.Connect(sA, sB, "dataAB", testModel);

            var fristconnection = testModel.Connections.First();
            Interactions.DeConnect(fristconnection, testModel);

            Assert.IsTrue(testModel.Connections.Count == 0);
            Assert.IsTrue(testModel.SoftwareCells.First(x => x.Name == "A").OutputStreams.First().DataNames == "dataAB");
            Assert.IsTrue(testModel.SoftwareCells.First(x => x.Name == "B").InputStreams.First().DataNames == "dataAB");


            testModel = new MainModel();
            sA = SoftwareCellsManager.CreateNew("A");
            testModel.SoftwareCells.Add(sA);
            Interactions.AddNewOutput(sA, "dataA");

            sB = SoftwareCellsManager.CreateNew("B");
            testModel.SoftwareCells.Add(sB);

            Interactions.ConnectDangelingConnectionAndSoftwareCell(sA.OutputStreams.First(), sB, testModel);

            Interactions.DeConnect(testModel.Connections.First(), testModel);

            Assert.IsTrue(testModel.Connections.Count == 0);
            Assert.IsTrue(sA.OutputStreams.First().DataNames == "dataA");
            Assert.IsTrue(sA.OutputStreams.First().Connected == false);

            Assert.IsTrue(sB.InputStreams.First().DataNames == "");
            Assert.IsTrue(sB.InputStreams.First().Connected == false);
        }


        [TestMethod]
        public void ChangeConnectionDestinationTest()
        {
            var testModel = new MainModel();

            var sA = SoftwareCellsManager.CreateNew("A");
            var sB = SoftwareCellsManager.CreateNew("B");
            var sC = SoftwareCellsManager.CreateNew("C");
            testModel.SoftwareCells.Add(sA);
            testModel.SoftwareCells.Add(sB);
            testModel.SoftwareCells.Add(sC);
            MainModelManager.Connect(sA, sB, "dataAB", testModel);

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
            var oneCell = MainModelManager.AddNewSoftwareCell("one", mainModel);
            var secondCell = MainModelManager.AddNewSoftwareCell("second", mainModel);
            MainModelManager.Connect(oneCell, secondCell, "testdata", mainModel);

            Assert.IsTrue(mainModel.Connections.Any(x => x.DataNames.Equals("testdata")));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Sources.Any(dsd => dsd.Parent == oneCell)));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Destinations.Any(dsd => dsd.Parent == secondCell)));
        }


        [TestMethod]
        public void ConnectDangelingConnectionAndSoftwareCellTest()
        {
            var mainModel = new MainModel();
            var oneCell = MainModelManager.AddNewSoftwareCell("one", mainModel);
            Interactions.AddNewOutput(oneCell, "testdata");
            var secondCell = MainModelManager.AddNewSoftwareCell("second", mainModel);
            Interactions.ConnectDangelingConnectionAndSoftwareCell(oneCell.OutputStreams.First(), secondCell, mainModel);

            Assert.IsTrue(mainModel.Connections.Any(x => x.DataNames.Equals("testdata | ")));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Sources.Any(dsd => dsd.Parent == oneCell)));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Destinations.Any(dsd => dsd.Parent == secondCell)));

            Assert.IsTrue(oneCell.OutputStreams.First().DataNames == "testdata");
            Assert.IsTrue(oneCell.OutputStreams.First().Connected);

            Assert.IsTrue(secondCell.InputStreams.First().Connected);
            Assert.IsTrue(secondCell.InputStreams.First().DataNames == "");
        }
    }

}