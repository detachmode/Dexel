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
            main.Integration.Add(firstOp);

            MainModelManager.ConnectTwoFunctionUnits(firstOp, secondOp, "", "", testModel);
            MainModelManager.ConnectTwoFunctionUnits(secondOp, thirdOp, "", "", testModel);

            // first functionunit in integration deleted -> assert that only this will get deleted
            MainModelManager.DeleteFunctionUnit(firstOp,testModel);

            CollectionAssert.DoesNotContain(main.Integration, firstOp);
            CollectionAssert.Contains(main.Integration, secondOp);
            CollectionAssert.Contains(main.Integration, thirdOp);


            testModel = new MainModel();
            main = MainModelManager.AddNewFunctionUnit("Main", testModel);

            firstOp = MainModelManager.AddNewFunctionUnit("Operation 1", testModel);
            secondOp = MainModelManager.AddNewFunctionUnit("Operation 2", testModel);
            thirdOp = MainModelManager.AddNewFunctionUnit("Operation 3", testModel);
            main.Integration.Add(firstOp);

            MainModelManager.ConnectTwoFunctionUnits(firstOp, secondOp, "", "", testModel);
            MainModelManager.ConnectTwoFunctionUnits(secondOp, thirdOp, "", "", testModel);

            // second functionunit in integration deleted -> assert that all following will also be removed from integration
            MainModelManager.DeleteFunctionUnit(secondOp, testModel);

            CollectionAssert.Contains(main.Integration, firstOp);
            CollectionAssert.DoesNotContain(main.Integration, secondOp);
            CollectionAssert.DoesNotContain(main.Integration, thirdOp);




        }
    }
}