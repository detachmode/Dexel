using System.Collections.Generic;
using System.Linq;
using Dexel.Model;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Common;

namespace Roslyn.Tests
{
    [TestClass]
    public class MyGeneratorTests
    {
        private readonly MyGenerator _gen = new MyGenerator();
        private static readonly DataStreamManager DataStreamManager = new DataStreamManager();
        private static readonly SoftwareCellsManager SoftwareCellsManager = new SoftwareCellsManager();
        private static readonly MainModelManager MainModelManager = new MainModelManager(SoftwareCellsManager, DataStreamManager);

        [TestMethod]
        public void GetReturnTypesTest()
        {

            CollectionAssert.AreEqual(new[] { "string" }, DataStreamParser.GetInputPart("string").Select(x => x.Type).ToArray());
            CollectionAssert.AreEqual(new[] { "string" }, DataStreamParser.GetOutputPart("string").Select(x => x.Type).ToArray());
            CollectionAssert.AreEqual(new[] { "string" }, DataStreamParser.GetInputPart("|string").Select(x => x.Type).ToArray());
            CollectionAssert.AreEqual(new[] { "string" }, DataStreamParser.GetInputPart("int|string").Select(x => x.Type).ToArray());
            CollectionAssert.AreEqual(new[] { "string", "int" }, DataStreamParser.GetInputPart("int|string,int").Select(x => x.Type).ToArray());

            // removes whitespace?
            CollectionAssert.AreEqual(new[] { "string", "int" }, DataStreamParser.GetInputPart("int| string , int").Select(x => x.Type).ToArray());

            // named parameter
            CollectionAssert.AreEqual(new[] { "string", "int" }, DataStreamParser.GetInputPart("int| name:string , age:int").Select(x => x.Type).ToArray());
            CollectionAssert.AreEqual(new[] { "name", "age" }, DataStreamParser.GetInputPart("int| name:string , age:int").Select(x => x.Name).ToArray());

        }

        [TestMethod]
        public void MethodTest()
        {
            // string return value
            var node = SoftwareCellsManager.CreateNew("Random Name");
            MainModelManager.AddNewOutput(node, "string");

            var methode = MethodsGenerator.GenerateMethod(_gen.Generator, node);
            Assert.AreEqual("public string Random_Name()\r\n{\r\n}", methode.NormalizeWhitespace().ToFullString());

            // Empty output => return void
            node = SoftwareCellsManager.CreateNew("Random Name");
            MainModelManager.AddNewOutput(node, "");

            methode = MethodsGenerator.GenerateMethod(_gen.Generator, node);
            Assert.AreEqual("public void Random_Name()\r\n{\r\n}", methode.NormalizeWhitespace().ToFullString());


            var testModel = new MainModel();
            var newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.Connect(newName, alter, "string | ", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.Connect(alter, person, "int | int, string", testModel);

            var definition = DataStreamManager.CreateNewDefinition(person, "Person");
            person.OutputStreams.Add(definition);

            SyntaxNode[] members = testModel.SoftwareCells.Select(x => MethodsGenerator.GenerateMethod(_gen.Generator, x)).ToArray();
            var newNameMethod = members[0];
            Assert.AreEqual("public string Random_Name()\r\n{\r\n}", newNameMethod.NormalizeWhitespace().ToFullString());

            var alterMethod = members[1];
            Assert.AreEqual("public int Random_Age()\r\n{\r\n}", alterMethod.NormalizeWhitespace().ToFullString());

            var personMethod = members[2];
            Assert.AreEqual("public Person Create_Person(int param1, string param2)\r\n{\r\n}", personMethod.NormalizeWhitespace().ToFullString());

            // Named Parameter
            person.InputStreams.Clear();
            person.InputStreams.Add(new DataStreamDefinition() { DataNames = "int | age:int, name:string" });
            var personMethodNamedParams = MethodsGenerator.GenerateMethod(_gen.Generator, person);
            Assert.AreEqual("public Person Create_Person(int age, string name)\r\n{\r\n}",
                personMethodNamedParams.NormalizeWhitespace().ToFullString());







        }

        [TestMethod()]
        public void FindParametersTest()
        {

            var testModel = new MainModel();
            var newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.Connect(newName, alter, "string | ", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.Connect(alter, person, "int | int, string", testModel);


            var expectedList = new List<Parameter>()
            {
                new Parameter() {FoundFlag = true, Source = alter},
                new Parameter() {FoundFlag = true, Source = newName}
            };
            var paramList = Integrations.FindParameters(testModel.Connections, person);
            Assert.IsTrue(expectedList[0].Compare(paramList[0]));
            Assert.IsTrue(expectedList[1].Compare(paramList[1]));
        }

        [TestMethod()]
        public void FindOneParameterTest()
        {
            var testModel = new MainModel();
            var newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.Connect(newName, alter, "string | ", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.Connect(alter, person, "int | int, string", testModel);

            var expected = new Parameter { FoundFlag = true, Source = alter };
            var lookingfor = new NameType { Name = null, Type = "int" };
            Assert.IsTrue(expected.Compare(Integrations.FindOneParameter(lookingfor, testModel.Connections, person)));

            expected = new Parameter { FoundFlag = true, Source = newName };
            lookingfor = new NameType { Name = null, Type = "string" };
            Assert.IsTrue(expected.Compare(Integrations.FindOneParameter(lookingfor, testModel.Connections, person)));

            testModel = new MainModel();
            newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.Connect(newName, alter, "string | ", testModel);

            person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.Connect(alter, person, "int | ... string", testModel);

            lookingfor = new NameType { Name = null, Type = "int" };
            Assert.IsTrue(Integrations.FindOneParameter(lookingfor, testModel.Connections, person).Source == alter);


        }

      

        [TestMethod()]
        public void LocalMethodCallTest()
        {
            // void method call -> no local variable needed
            var testModel = new MainModel();
            var foo = MainModelManager.AddNewSoftwareCell("foo", testModel);
            MainModelManager.AddNewInput(foo, "");
            MainModelManager.AddNewOutput(foo, "");
           var nodes = Integrations.LocalMethodCall(_gen.Generator, foo, null, new List<GeneratedLocalVariable>());
           var fullstring =nodes.NormalizeWhitespace().ToFullString();
            Assert.AreEqual("foo()", fullstring);
        }
    }
}