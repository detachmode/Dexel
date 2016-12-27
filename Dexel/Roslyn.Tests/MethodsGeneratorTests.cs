using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Roslyn.Generators;

namespace Roslyn.Tests
{
    [TestClass()]
    public class MethodsGeneratorTests
    {
        private readonly MyGenerator _mygen = new MyGenerator();

        [TestMethod()]
        public void GetNotImplementatedExceptionTest()
        {
            Workspace _workspace = new AdhocWorkspace();
            //var res = MethodsGenerator.GetNotImplementatedException(SyntaxGenerator.GetGenerator(_workspace, LanguageNames.CSharp));
            //string s = res.ToFullString();
        }

        [TestMethod()]
        public void GetParameters_With_Multiple_Outputs()
        {

            var fu = FunctionUnitManager.CreateNew("foo");
            MainModelManager.AddNewInput(fu, "(name:string)");
            MainModelManager.AddNewOutput(fu, "(int)", actionName: ".onSuccess");
            MainModelManager.AddNewOutput(fu, "(string)", actionName: ".onError");


            var paramSignature = MethodsGenerator.GetParameters(_mygen.Generator, fu)
                .Select(sn => sn.NormalizeWhitespace().ToFullString()).ToList();

            Assert.AreEqual("string name", paramSignature[0]);
            Assert.AreEqual("Action<int> onSuccess", paramSignature[1]);
            Assert.AreEqual("Action<string> onError", paramSignature[2]);


        }

        [TestMethod()]
        public void IOTest_WithTwo_Outputs_OnlyOneHasActionname()
        {

            var fu = FunctionUnitManager.CreateNew("foo");
            MainModelManager.AddNewInput(fu, "(name:string)");
            MainModelManager.AddNewOutput(fu, "(int)");
            MainModelManager.AddNewOutput(fu, "(string)", actionName: ".onError");

            var returnType = MethodsGenerator.GetReturnPart(_mygen.Generator, fu)
                .NormalizeWhitespace().ToFullString();

            Assert.AreEqual("int", returnType);

            var paramSignature = MethodsGenerator.GetParameters(_mygen.Generator, fu)
                .Select(sn => sn.NormalizeWhitespace().ToFullString()).ToList();

            Assert.AreEqual("string name", paramSignature[0]);
            Assert.AreEqual("Action<string> onError", paramSignature[1]);

        }

        [TestMethod()]
        public void IOTest_OutputAsAction_WithTwoTypes()
        {

            var fu = FunctionUnitManager.CreateNew("foo");
            MainModelManager.AddNewInput(fu, "(name:string)");
            MainModelManager.AddNewOutput(fu, "(string, object)", actionName: ".onError");

            var returnType = MethodsGenerator.GetReturnPart(_mygen.Generator, fu)?
                .NormalizeWhitespace().ToFullString();

            Assert.AreEqual(null, returnType);

            var paramSignature = MethodsGenerator.GetParameters(_mygen.Generator, fu)
                .Select(sn => sn.NormalizeWhitespace().ToFullString()).ToList();

            Assert.AreEqual("string name", paramSignature[0]);
            Assert.AreEqual("Action<string, object> onError", paramSignature[1]);

        }


        [TestMethod()]
        public void IOTest_WithTwo_Outputs_NoActionnames()
        {

            var fu = FunctionUnitManager.CreateNew("foo");
            MainModelManager.AddNewInput(fu, "(name:string)");
            MainModelManager.AddNewOutput(fu, "(int)");
            MainModelManager.AddNewOutput(fu, "(string)");

            var returnType = MethodsGenerator.GetReturnPart(_mygen.Generator, fu)?
                .NormalizeWhitespace().ToFullString();

            Assert.AreEqual(null, returnType);

            var paramSignature = MethodsGenerator.GetParameters(_mygen.Generator, fu)
                .Select(sn => sn.NormalizeWhitespace().ToFullString()).ToList();

            Assert.AreEqual("string name", paramSignature[0]);
            Assert.AreEqual("Action<int> onInt", paramSignature[1]);
            Assert.AreEqual("Action<string> onString", paramSignature[2]);


        }


        [TestMethod()]
        public void GetParameters_With_Input_And_OutputStream()
        {

            var fu = FunctionUnitManager.CreateNew("foo");
            MainModelManager.AddNewInput(fu, "(name:string)");
            MainModelManager.AddNewOutput(fu, "(int)*", actionName: ".onNumber");

            var paramSignature = MethodsGenerator.GetParameters(_mygen.Generator, fu)
                .Select(sn => sn.NormalizeWhitespace().ToFullString()).ToList();

            Assert.AreEqual("string name", paramSignature[0]);
            Assert.AreEqual("Action<int> onNumber", paramSignature[1]);

        }


        [TestMethod()]
        public void IOTest_TupelOutput()
        {

            var fu = FunctionUnitManager.CreateNew("foo");
            MainModelManager.AddNewInput(fu, "(name:string)");
            MainModelManager.AddNewOutput(fu, "(int,string)");

            var returnType = MethodsGenerator.GetReturnPart(_mygen.Generator, fu)?
                .NormalizeWhitespace().ToFullString();

            Assert.AreEqual("Tupel<int, string>", returnType);
        }

        [TestMethod()]
        public void IOTest_IEnumerbaleActionOutput()
        {

            var fu = FunctionUnitManager.CreateNew("foo");
            MainModelManager.AddNewInput(fu, "(name:string)");
            MainModelManager.AddNewOutput(fu, "(int*)*");


            var paramSignature = MethodsGenerator.GetParameters(_mygen.Generator, fu)
                     .Select(sn => sn.NormalizeWhitespace().ToFullString()).ToList();

            Assert.AreEqual("Action<IEnumerable<int>> onInt", paramSignature[1]);
        }

        [TestMethod()]
        public void IOTest_actionWithoutOutput()
        {

            var fu = FunctionUnitManager.CreateNew("foo");
            MainModelManager.AddNewInput(fu, "()");
            MainModelManager.AddNewOutput(fu, "()", actionName:"onEach");


            var paramSignature = MethodsGenerator.GetParameters(_mygen.Generator, fu)
                     .Select(sn => sn.NormalizeWhitespace().ToFullString()).ToList();

            Assert.AreEqual("Action onEach", paramSignature[0]);
        }

        [TestMethod()]
        public void IOTest_InputStreamOutputStream()
        {

            var fu = FunctionUnitManager.CreateNew("foo");
            MainModelManager.AddNewInput(fu, "(name:string)*");
            MainModelManager.AddNewOutput(fu, "(Person)*");
            MainModelManager.AddNewOutput(fu, "(string)", actionName: "onError");


            var returnType = MethodsGenerator.GetReturnPart(_mygen.Generator, fu)?
                .NormalizeWhitespace().ToFullString();

            Assert.AreEqual("Person", returnType);


            var paramSignature = MethodsGenerator.GetParameters(_mygen.Generator, fu)
                .Select(sn => sn.NormalizeWhitespace().ToFullString()).ToList();

            Assert.AreEqual("string name", paramSignature[0]);
            Assert.AreEqual("Action<string> onError", paramSignature[1]);
        }




        [TestMethod()]
        public void GetParameter_OneSimpleInput_WithoutNames()
        {
            var fu = FunctionUnitManager.CreateNew("foo");
            MainModelManager.AddNewInput(fu, "(string)");
            MainModelManager.AddNewOutput(fu, "(int)");



            var paramSignature = MethodsGenerator.GetParameters(_mygen.Generator, fu)
                .Select(sn => sn.NormalizeWhitespace().ToFullString()).ToList();

            Assert.AreEqual("string astring", paramSignature[0]);

        }

        [TestMethod()]
        public void GetParameter_OneSimpleInput()
        {
            var fu = FunctionUnitManager.CreateNew("foo");
            MainModelManager.AddNewInput(fu, "(name:string)");
            MainModelManager.AddNewOutput(fu, "(zahl:int)");



            var paramSignature = MethodsGenerator.GetParameters(_mygen.Generator, fu)
                .Select(sn => sn.NormalizeWhitespace().ToFullString()).ToList();

            Assert.AreEqual("string name", paramSignature[0]);

        }
    }
}