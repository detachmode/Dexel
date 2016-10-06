using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpFlowDesignTests.Model
{
    [TestClass()]
    public class SoftwareCellTests
    {


        [TestMethod()]
        public void MainTest()
        {


            //var romanNumbersConverter = new SoftwareCell { DataNames = "Roman Numbers Converter" };
            //romanNumbersConverter.AddInput("RomanNumber");

            //var splitter = new SoftwareCell { DataNames = "Splitt Roman Numerals" };
            //splitter.AddInput("RomanNumber");
            //romanNumbersConverter.SetIntegration(splitter);

            //var convertEach = new SoftwareCell { DataNames = "Convert to decimal" };
            //splitter.Connect(convertEach, "Roman Numeral*");

            //var negatelogic = new SoftwareCell { DataNames = "Negate when larger" };
            //convertEach.Connect(negatelogic, "Decimal*");

            //DebugPrinter.PrintOutputs(romanNumbersConverter);
            //DebugPrinter.PrintIntegration(romanNumbersConverter);
            //DebugPrinter.PrintRecursive(splitter);
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