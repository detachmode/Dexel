using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dexel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dexel.Model.Tests
{
    [TestClass()]
    public class DataStreamManagerTests
    {
        [TestMethod()]
        public void SolvePipeLogicTest()
        {
            var datastreamManager = new DataStreamManager();

            var ds = datastreamManager.CreateNew("(int)* | (... string)*");
            var splitted = datastreamManager.SolvePipeLogic(ds);
            Assert.IsTrue(splitted[0].Trim() == "(int)*");
            Assert.IsTrue(splitted[1].Trim() == "(int, string)*");

            ds = datastreamManager.CreateNew("int | ... string ");
            splitted = datastreamManager.SolvePipeLogic(ds);
            Assert.IsTrue(splitted[0].Trim() == "int");
            Assert.IsTrue(splitted[1].Trim() == "int, string");

            ds = datastreamManager.CreateNew("int | string ");
            splitted = datastreamManager.SolvePipeLogic(ds);
            Assert.IsTrue(splitted[0].Trim() == "int");
            Assert.IsTrue(splitted[1].Trim() == "string");

            ds = datastreamManager.CreateNew("int, string");
            splitted = datastreamManager.SolvePipeLogic(ds);
            Assert.IsTrue(splitted[0].Trim() == "int, string");
            Assert.IsTrue(splitted[1].Trim() == "int, string");



        }
    }
}