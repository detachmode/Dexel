using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using SharpFlowDesign;
using SharpFlowDesign.Model;

namespace Roslyn.Tests
{
    [TestClass()]
    public class MyGeneratorTests
    {
        private readonly MyGenerator _gen = new MyGenerator();
        [TestMethod()]
        public void GetReturnTypesTest()
        {

            CollectionAssert.AreEqual(new string[] { "string" }, _gen.GetReturnTypes("string"));
            CollectionAssert.AreEqual(new string[] { "string" }, _gen.GetReturnTypes("|string", pipePart:2));
            CollectionAssert.AreEqual(new string[] { "string" }, _gen.GetReturnTypes("int|string", pipePart: 2));
            CollectionAssert.AreEqual(new string[] { "string", "int" }, _gen.GetReturnTypes("int|string,int", pipePart: 2));

            // removes whitespace?
            CollectionAssert.AreEqual(new string[] { "string", "int" }, _gen.GetReturnTypes("int| string , int", pipePart: 2));

            // named parameter
            CollectionAssert.AreEqual(new string[] { "string", "int" }, _gen.GetReturnTypes("int| name:string , age:int", pipePart: 2));
        }

        [TestMethod()]
        public void MethodTest()
        {
            // string return value
            var node = SoftwareCellsManager.CreateNew("Random Name");
            Interactions.AddNewOutput(node, "string");

            var methode = _gen.Method(node);
            Assert.AreEqual("public string Random_Name()\r\n{\r\n}", methode.NormalizeWhitespace().ToFullString());

            // Empty output => return void
            node = SoftwareCellsManager.CreateNew("Random Name");
            Interactions.AddNewOutput(node, "");

            methode = _gen.Method(node);
            Assert.AreEqual("public void Random_Name()\r\n{\r\n}", methode.NormalizeWhitespace().ToFullString());


            var testModel = new MainModel();
            var newNameID = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            var newName = SoftwareCellsManager.GetFirst(newNameID, testModel);
            Interactions.AddNewInput(newNameID, "", testModel);

            var alterID = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            var alter = SoftwareCellsManager.GetFirst(alterID, testModel);
            MainModelManager.Connect(newNameID, alterID, "string | ", testModel);

            var personID = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            var person = SoftwareCellsManager.GetFirst(personID, testModel);
            MainModelManager.Connect(alterID, personID, "int | ... string", testModel);

            var definition = DataStreamManager.CreateNewDefinition("Person");
            person.OutputStreams.Add(definition);

            SyntaxNode[] members = testModel.SoftwareCells.Select(x => _gen.Method(x)).ToArray();
            var newNameMethod = members[0];
            Assert.AreEqual("public string Random_Name()\r\n{\r\n}", newNameMethod.NormalizeWhitespace().ToFullString());

            var alterMethod = members[1];
            Assert.AreEqual("public int Random_Age()\r\n{\r\n}", alterMethod.NormalizeWhitespace().ToFullString());

            var personMethod = members[2];

        }
    }
}