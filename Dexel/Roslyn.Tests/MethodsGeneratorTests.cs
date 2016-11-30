using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn.Tests
{
    [TestClass()]
    public class MethodsGeneratorTests
    {
        [TestMethod()]
        public void GetNotImplementatedExceptionTest()
        {
            Workspace _workspace = new AdhocWorkspace();
            //var res = MethodsGenerator.GetNotImplementatedException(SyntaxGenerator.GetGenerator(_workspace, LanguageNames.CSharp));
            //string s = res.ToFullString();
        }
    }
}