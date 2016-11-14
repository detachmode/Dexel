using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dexel.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dexel.Model.DataTypes;

namespace Dexel.Model.Tests
{
    [TestClass()]
    public class MainModelManagerTests
    {
        [TestMethod()]
        public void TraverseChildrenTest()
        {
            var testmodel = Mockdata.MakeRandomPerson2();
            var randname = testmodel.SoftwareCells[0];
            var found = new List<SoftwareCell>();
            MainModelManager.TraverseChildren(randname, cell => found.Add(cell), testmodel);
            Assert.IsTrue(found.Any(sc => sc.Name == "Random Age"));
            Assert.IsTrue(found.Any(sc => sc.Name == "Create Person"));
        }

        [TestMethod()]
        public void RemoveFromIntegrationIncludingChildrenTest()
        {
            var testModel = new MainModel();
            var main = MainModelManager.AddNewSoftwareCell("Main", testModel);

            var firstOp = MainModelManager.AddNewSoftwareCell("Operation 1", testModel);
            var secondOp = MainModelManager.AddNewSoftwareCell("Operation 2", testModel);
            var thirdOp = MainModelManager.AddNewSoftwareCell("Operation 3", testModel);
            main.Integration.Add(firstOp);
            MainModelManager.ConnectTwoCells(firstOp, secondOp, "", "", testModel);
            MainModelManager.ConnectTwoCells(secondOp, thirdOp, "", "", testModel);



            var found = new List<SoftwareCell>();
            MainModelManager.TraverseChildren(firstOp, cell => found.Add(cell), testModel);
            Assert.IsTrue(found.Any(sc => sc == secondOp));
            Assert.IsTrue(found.Any(sc => sc == thirdOp));

            var connectionToRemove = testModel.Connections.First(c => c.Sources.Any(dsd => dsd.Parent == firstOp));
            MainModelManager.RemoveFromIntegrationIncludingChildren(connectionToRemove, testModel);

            Assert.AreEqual(0, main.Integration.Count);


        }

        [TestMethod()]
        public void RemoveConnectionTest()
        {
            Assert.Fail();
        }
    }
}