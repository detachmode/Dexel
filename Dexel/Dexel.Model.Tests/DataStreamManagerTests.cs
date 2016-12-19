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

            var ds = DataStreamManager.NewDataStream("(int)* | (... string)*");
            DataStreamManager.SolveWithPipeNotation(ds.DataNames, (s1, s2) =>
            {
                Assert.AreEqual("(int)*", s1.Trim());
                Assert.AreEqual("(int, string)*", s2.Trim());
            }, Assert.Fail );



            ds = DataStreamManager.NewDataStream("(int) | (string) ");
            DataStreamManager.SolveWithPipeNotation(ds.DataNames, (s1, s2) =>
            {
                Assert.AreEqual("(int)", s1.Trim() );
                Assert.AreEqual("(string)", s2.Trim());
            }, Assert.Fail);
            

            ds = DataStreamManager.NewDataStream("(int, string)");
            DataStreamManager.SolveWithPipeNotation(ds.DataNames, (s1, s2) => Assert.Fail(), () => {} );

        }
    }
}