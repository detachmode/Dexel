using System.Collections.Generic;
using System.Linq;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Common;

namespace Roslyn.Tests
{
    [TestClass]
    public class MyGeneratorTests
    {
        private readonly MyGenerator _gen = new MyGenerator();


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

            var methode = MethodsGenerator.GenerateStaticMethod(_gen.Generator, node);
            Assert.AreEqual("public static string RandomName()\r\n{\r\n}", methode.NormalizeWhitespace().ToFullString());

            // Empty output => return void
            node = SoftwareCellsManager.CreateNew("Random Name");
            MainModelManager.AddNewOutput(node, "");

            methode = MethodsGenerator.GenerateStaticMethod(_gen.Generator, node);
            Assert.AreEqual("public static void RandomName()\r\n{\r\n}", methode.NormalizeWhitespace().ToFullString());


            var testModel = new MainModel();
            var newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.ConnectTwoCells(newName, alter, "string", "", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.ConnectTwoCells(alter, person, "int", "int, string", testModel);

            var definition = DataStreamManager.NewDefinition(person, "Person");
            person.OutputStreams.Add(definition);

            SyntaxNode[] members = testModel.SoftwareCells.Select(x => MethodsGenerator.GenerateStaticMethod(_gen.Generator, x)).ToArray();
            var newNameMethod = members[0];
            Assert.AreEqual("public static string RandomName()\r\n{\r\n}", newNameMethod.NormalizeWhitespace().ToFullString());

            var alterMethod = members[1];
            Assert.AreEqual("public static int RandomAge()\r\n{\r\n}", alterMethod.NormalizeWhitespace().ToFullString());

            var personMethod = members[2];
            Assert.AreEqual("public static Person CreatePerson(int @int, string @string)\r\n{\r\n}", personMethod.NormalizeWhitespace().ToFullString());

            // Named Parameter
            person.InputStreams.Clear();
            SoftwareCellsManager.NewInputDef(person, "int | age:int, name:string", "");
            var personMethodNamedParams = MethodsGenerator.GenerateStaticMethod(_gen.Generator, person);
            Assert.AreEqual("public static Person CreatePerson(int age, string name)\r\n{\r\n}",
                personMethodNamedParams.NormalizeWhitespace().ToFullString());







        }

        [TestMethod()]
        public void FindParametersTest()
        {

            var testModel = new MainModel();
            var main = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(main, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.ConnectTwoCells(main, alter, "string", "", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.ConnectTwoCells(alter, person, "int","int, string", testModel);


             var expectedList = new List<Parameter>()
            {
                new Parameter() {FoundFlag = true, Source = alter},
                new Parameter() {FoundFlag = true, Source = main}
            };
            var paramList = Integrations.FindParameters(testModel.Connections, main, person);
            Assert.IsTrue(expectedList[0].Compare(paramList[0]));
            Assert.IsTrue(expectedList[1].Compare(paramList[1]));

            // test find parameter from parent method arguments
            testModel = new MainModel();
            main = MainModelManager.AddNewSoftwareCell("convert roman number", testModel);
            MainModelManager.AddNewInput(main, "string");
            MainModelManager.AddNewOutput(main, "int");



            var splitter = MainModelManager.AddNewSoftwareCell("split", testModel);
            MainModelManager.AddNewInput(splitter, "string");
            MainModelManager.AddNewOutput(splitter, "char*");

            main.Integration.Add(splitter);


            paramList = Integrations.FindParameters(testModel.Connections, main, splitter);
            Assert.IsTrue(paramList.Count == 0);

        }

        [TestMethod()]
        public void FindOneParameterTest()
        {
            var testModel = new MainModel();
            var newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.ConnectTwoCells(newName, alter, "string", "", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.ConnectTwoCells(alter, person, "int","int, string", testModel);

            var expected = new Parameter { FoundFlag = true, Source = alter };
            var lookingfor = new NameType { Name = null, Type = "int" };
            Assert.IsTrue(expected.Compare(Integrations.FindOneParameter(lookingfor, null,testModel.Connections, person)));

            expected = new Parameter { FoundFlag = true, Source = newName };
            lookingfor = new NameType { Name = null, Type = "string" };
            Assert.IsTrue(expected.Compare(Integrations.FindOneParameter(lookingfor,null ,testModel.Connections, person)));

            testModel = new MainModel();
            newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.ConnectTwoCells(newName, alter, "string","", testModel);

            person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.ConnectTwoCells(alter, person, "int","int, string", testModel);

            lookingfor = new NameType { Name = null, Type = "int" };
            Assert.IsTrue(Integrations.FindOneParameter(lookingfor,null, testModel.Connections, person).Source == alter);


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
            Assert.AreEqual("Foo()", fullstring);
        }
    }
}