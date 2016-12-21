using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model;
using Dexel.Model.Manager;

namespace Roslyn.Tests
{
    [TestClass()]
    public class DataStreamParserTests
    {
        [TestMethod()]
        public void ConvertToNameTypeTest()
        {

            DataStreamParser.ConvertToNameType("age:int", false, nametype =>
            {
                Assert.IsTrue(nametype.Name == "age");
                Assert.IsTrue(nametype.Type == "int");
            });

            DataStreamParser.ConvertToNameType("age:int[]", false, nametype =>
            {
                Assert.IsTrue(nametype.Name == "age");
                Assert.IsTrue(nametype.Type == "int");
                Assert.IsTrue(nametype.IsArray);
            });

            DataStreamParser.ConvertToNameType("age:int*", false, nametype =>
            {
                Assert.IsTrue(nametype.Name == "age");
                Assert.IsTrue(nametype.Type == "int");
                Assert.IsTrue(nametype.IsList);
            });
        }



        [TestMethod()]
        public void GetInputPartTest()
        {

            var inputDataTypes = DataStreamParser.GetInputPart("( double | int, string)*").ToList();
            Assert.IsTrue(inputDataTypes.Any( dt => dt.IsInsideStream == true && dt.IsList== false && dt.Type == "int"));
            Assert.IsTrue(inputDataTypes.Any(dt => dt.IsInsideStream == true && dt.IsList == false && dt.Type == "string"));
            Assert.IsFalse(inputDataTypes.Any(dt => dt.Type == "double"));

            inputDataTypes = DataStreamParser.GetInputPart("double | ... string*").ToList();
            Assert.IsTrue(inputDataTypes.Any(dt => dt.IsInsideStream == false && dt.IsList == true && dt.Type == "string"));
            Assert.IsTrue(inputDataTypes.Any(dt => dt.IsInsideStream == false && dt.IsList == false && dt.Type == "double"));

            inputDataTypes = DataStreamParser.GetInputPart("( double | ... string)*").ToList();
            Assert.IsTrue(inputDataTypes.Any(dt => dt.IsInsideStream == true && dt.IsList == false && dt.Type == "string"));
            Assert.IsTrue(inputDataTypes.Any(dt => dt.IsInsideStream == true && dt.IsList == false && dt.Type == "double"));
        }
    
    }
}