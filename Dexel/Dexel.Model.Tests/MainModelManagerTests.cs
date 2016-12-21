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
using Dexel.Model.Manager;

namespace Dexel.Model.Tests
{
    [TestClass()]
    public class MainModelManagerTests
    {
        [TestMethod()]
        public void TraverseChildrenTest()
        {
            var testmodel = Mockdata.Mockdata.MakeRandomPerson2();
            var randname = testmodel.FunctionUnits[0];
            var found = new List<FunctionUnit>();
            MainModelManager.TraverseChildren(randname, fu => found.Add(fu), testmodel);
            Assert.IsTrue(found.Any(sc => sc.Name == "Random Age"));
            Assert.IsTrue(found.Any(sc => sc.Name == "Create Person"));
        }



        [TestMethod()]
        public void DeleteFunctionUnitTest()
        {
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("Main", testModel);

            var firstOp = MainModelManager.AddNewFunctionUnit("Operation 1", testModel);
            var secondOp = MainModelManager.AddNewFunctionUnit("Operation 2", testModel);
            var thirdOp = MainModelManager.AddNewFunctionUnit("Operation 3", testModel);
            main.IsIntegrating.Add(firstOp);

            MainModelManager.ConnectTwoFunctionUnits(firstOp, secondOp, "", "", testModel);
            MainModelManager.ConnectTwoFunctionUnits(secondOp, thirdOp, "", "", testModel);

            // first functionunit in integration deleted -> assert that only this will get deleted
            MainModelManager.DeleteFunctionUnit(firstOp,testModel);

            CollectionAssert.DoesNotContain(main.IsIntegrating, firstOp);
            CollectionAssert.Contains(main.IsIntegrating, secondOp);
            CollectionAssert.Contains(main.IsIntegrating, thirdOp);


            testModel = new MainModel();
            main = MainModelManager.AddNewFunctionUnit("Main", testModel);

            firstOp = MainModelManager.AddNewFunctionUnit("Operation 1", testModel);
            secondOp = MainModelManager.AddNewFunctionUnit("Operation 2", testModel);
            thirdOp = MainModelManager.AddNewFunctionUnit("Operation 3", testModel);
            main.IsIntegrating.Add(firstOp);

            MainModelManager.ConnectTwoFunctionUnits(firstOp, secondOp, "", "", testModel);
            MainModelManager.ConnectTwoFunctionUnits(secondOp, thirdOp, "", "", testModel);

            // second functionunit in integration deleted -> assert that all following will also be removed from integration
            MainModelManager.DeleteFunctionUnit(secondOp, testModel);

            CollectionAssert.Contains(main.IsIntegrating, firstOp);
            CollectionAssert.DoesNotContain(main.IsIntegrating, secondOp);
            CollectionAssert.DoesNotContain(main.IsIntegrating, thirdOp);




        }
    }
}