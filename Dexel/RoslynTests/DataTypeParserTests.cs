using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model;

namespace Roslyn.Tests
{
    [TestClass()]
    public class DataTypeParserTests
    {
        private readonly MyGenerator _gen = new MyGenerator();
        [TestMethod()]
        public void ConvertToTypeExpressionTest()
        {
            var test = DataTypeParser.ConvertToTypeExpression(_gen.Generator, new NameType
            {
                IsList = true,
                Name = "zahlen",
                Type = "int"
            });
            var fullstring = test.ToFullString();
            Assert.AreEqual("List<int>", fullstring);


            test = DataTypeParser.ConvertToTypeExpression(_gen.Generator, new NameType
            {
                IsArray = true,
                Name = "zahlen",
                Type = "int"
            });
            fullstring = test.ToFullString();
            Assert.AreEqual("int[]", fullstring);
        }
    }
}