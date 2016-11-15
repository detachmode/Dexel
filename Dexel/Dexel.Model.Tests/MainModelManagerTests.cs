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
        public void DeleteCellTest()
        {
            var testModel = new MainModel();
            var main = MainModelManager.AddNewSoftwareCell("Main", testModel);

            var firstOp = MainModelManager.AddNewSoftwareCell("Operation 1", testModel);
            var secondOp = MainModelManager.AddNewSoftwareCell("Operation 2", testModel);
            var thirdOp = MainModelManager.AddNewSoftwareCell("Operation 3", testModel);
            main.Integration.Add(firstOp);

            MainModelManager.ConnectTwoCells(firstOp, secondOp, "", "", testModel);
            MainModelManager.ConnectTwoCells(secondOp, thirdOp, "", "", testModel);

            // first Cell in integration deleted -> assert that only this will get deleted
            MainModelManager.DeleteCell(firstOp,testModel);

            CollectionAssert.DoesNotContain(main.Integration, firstOp);
            CollectionAssert.Contains(main.Integration, secondOp);
            CollectionAssert.Contains(main.Integration, thirdOp);


            testModel = new MainModel();
            main = MainModelManager.AddNewSoftwareCell("Main", testModel);

            firstOp = MainModelManager.AddNewSoftwareCell("Operation 1", testModel);
            secondOp = MainModelManager.AddNewSoftwareCell("Operation 2", testModel);
            thirdOp = MainModelManager.AddNewSoftwareCell("Operation 3", testModel);
            main.Integration.Add(firstOp);

            MainModelManager.ConnectTwoCells(firstOp, secondOp, "", "", testModel);
            MainModelManager.ConnectTwoCells(secondOp, thirdOp, "", "", testModel);

            // second Cell in integration deleted -> assert that all following will also be removed from integration
            MainModelManager.DeleteCell(secondOp, testModel);

            CollectionAssert.Contains(main.Integration, firstOp);
            CollectionAssert.DoesNotContain(main.Integration, secondOp);
            CollectionAssert.DoesNotContain(main.Integration, thirdOp);




        }
    }
}