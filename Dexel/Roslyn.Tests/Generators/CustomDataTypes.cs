using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model.Manager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Roslyn.Tests.Generators
{
    [TestClass]
    public class CustomDataTypes
    {
        [TestMethod]
        public void TestRegex()
        {
            var result = DataTypeManager.StripGenericType("List<string>");
            Assert.AreEqual("string", result);

            result = DataTypeManager.StripGenericType("string");
            Assert.AreEqual("string", result);

        }
    }
}
