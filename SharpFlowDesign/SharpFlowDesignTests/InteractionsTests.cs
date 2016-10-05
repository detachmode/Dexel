using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpFlowDesign;
using SharpFlowDesign.Model;

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

            Interactions.AddNewConnection(source,desination, datastream, mainModel);
            Assert.IsTrue(datastream.Sources.Contains(source));
            Assert.IsTrue(datastream.Destinations.Contains(desination));
            Assert.IsTrue(desination.InputStreams.Contains( datastream));
            Assert.IsTrue(source.OutputStreams.Contains( datastream));
            Assert.IsTrue(mainModel.Connections.Contains(datastream));
            
        }

        [TestMethod()]
        public void ConnectTest()
        {
            var mainModel = new MainModel();
            var oneID = Interactions.AddNewSoftwareCell("one", mainModel);
            var secondID = Interactions.AddNewSoftwareCell("second", mainModel);
            Interactions.Connect(oneID, secondID, "testdata", mainModel);

            Assert.IsTrue(mainModel.Connections.Any(x => x.DataNames.Equals("testdata")));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Sources.Any(y => y.ID.Equals(oneID))));
            Assert.IsTrue(mainModel.Connections.Any(x => x.Destinations.Any(y => y.ID.Equals(secondID))));
        }
    }
}