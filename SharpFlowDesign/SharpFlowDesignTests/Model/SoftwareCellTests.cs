using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpFlowDesign.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpFlowDesign.Model.Tests
{
    [TestClass()]
    public class SoftwareCellTests
    {


        [TestMethod()]
        public void MainTest()
        {


            var romanNumbersConverter = new SoftwareCell { Name = "Roman Numbers Converter" };
            romanNumbersConverter.AddInput("RomanNumber");

            var splitter = new SoftwareCell { Name = "Splitt Roman Numerals" };
            splitter.AddInput("RomanNumber");
            romanNumbersConverter.SetIntegration(splitter);

            var convertEach = new SoftwareCell { Name = "Convert to decimal" };
            splitter.Connect(convertEach, "Roman Numeral*");

            var negatelogic = new SoftwareCell { Name = "Negate when larger" };
            convertEach.Connect(negatelogic, "Decimal*");

            romanNumbersConverter.PrintOutputs();
            romanNumbersConverter.PrintIntegration();
            SoftwareCell.PrintRecursive(splitter);
            Console.WriteLine(@"DONE");

        }



        [TestMethod()]
        public void SoftwareCellTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ConnectTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PrintRecursiveTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PrintOutputsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PrintIntegrationTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddOutputTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AddInputTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SetIntegrationTest()
        {
            Assert.Fail();
        }
    }
}