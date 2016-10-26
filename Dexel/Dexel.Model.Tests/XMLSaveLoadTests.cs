using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dexel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model.DataTypes;

namespace Dexel.Model.Tests
{
    [TestClass()]
    public class XMLSaveLoadTests
    {
        [TestMethod()]
        public void SaveToXMLTest()
        {
            var testmodel = Mockdata.MakeRandomPerson2();
            testmodel.SaveToXML("test.xml");

            var loadedTestModel = XMLSaveLoad.LoadFromXml("test.xml");
        }
    }
}