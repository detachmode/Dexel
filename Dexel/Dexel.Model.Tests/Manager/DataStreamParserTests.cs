using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dexel.Model.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dexel.Model.Manager.Tests
{
    [TestClass()]
    public class DataStreamParserTests
    {
        [TestMethod()]
        public void GetOutputPartTest()
        {
            var nts = DataStreamParser.GetOutputPart("(age:int*)*");
            Assert.AreEqual("age", nts.First().Name);
            Assert.AreEqual("int", nts.First().Type);
            Assert.AreEqual(true, nts.First().IsList);
            Assert.AreEqual(true, nts.First().IsInsideStream);

        }
    }
}