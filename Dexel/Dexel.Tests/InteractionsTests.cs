using System.Linq;
using Dexel.Editor;
using Dexel.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace Dexel.Editor.Tests
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

            Interactions.AddNewOutput(sA, "dataA");
            Interactions.AddNewInput(sB, "");
           

            Interactions.ConnectTwoDangelingConnections(sA.OutputStreams.First(), sB.InputStreams.First(), testModel);

            Assert.IsTrue(testModel.Connections.First().DataNames == "dataA | ");
            Assert.IsTrue(testModel.Connections.First().Sources.First().Parent == sA);
            Assert.IsTrue(testModel.Connections.First().Destinations.First().Parent == sB);

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


            testModel = new MainModel();
            sA = _softwareCellsManager.CreateNew("A");
            testModel.SoftwareCells.Add(sA);
            Interactions.AddNewOutput(sA, "dataA");

            sB = _softwareCellsManager.CreateNew("B");        
            testModel.SoftwareCells.Add(sB);
            
            Interactions.ConnectDangelingConnectionAndSoftwareCell(sA.OutputStreams.First(), sB, testModel);
            
            Interactions.DeConnect(testModel.Connections.First(), testModel);

            Assert.IsTrue(testModel.Connections.Count == 0);
            Assert.IsTrue(sA.OutputStreams.First().DataNames == "dataA");
            Assert.IsTrue(sA.OutputStreams.First().Connected == false);

            Assert.IsTrue(sB.InputStreams.First().DataNames == "");
            Assert.IsTrue(sB.InputStreams.First().Connected == false);

        }


   

        [TestMethod()]
        public void ChangeConnectionDestinationTest()
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

        [TestMethod()]
        public void ConnectDangelingConnectionAndSoftwareCellTest()
        {
            var mainModel = new MainModel();
            var oneCell = _mainModelManager.AddNewSoftwareCell("one", mainModel);
            Interactions.AddNewOutput(oneCell, "testdata");
            var secondCell = _mainModelManager.AddNewSoftwareCell("second", mainModel);
            Interactions.ConnectDangelingConnectionAndSoftwareCell(oneCell.OutputStreams.First(), secondCell, mainModel);

            Assert.IsTrue(mainModel.Connections.Any(x => x.DataNames.Equals("testdata | ")));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Sources.Any(dsd => dsd.Parent == oneCell)));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Destinations.Any(dsd => dsd.Parent == secondCell)));

            Assert.IsTrue(oneCell.OutputStreams.First().DataNames == "testdata");
            Assert.IsTrue(oneCell.OutputStreams.First().Connected == true);

            Assert.IsTrue(secondCell.InputStreams.First().Connected == true);
            Assert.IsTrue(secondCell.InputStreams.First().DataNames == "");

        }


    }
}

