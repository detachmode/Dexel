﻿using System.Collections.Generic;
using Roslyn;
using System.Linq;
using FlowDesignModel;
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

            CollectionAssert.AreEqual(new[] { "string" }, DataStreamParser.ParseDataNames("string").Select(x => x.Type).ToArray());
            CollectionAssert.AreEqual(new[] { "string" }, DataStreamParser.ParseDataNames("|string", pipePart: 2).Select(x => x.Type).ToArray());
            CollectionAssert.AreEqual(new[] { "string" }, DataStreamParser.ParseDataNames("int|string", pipePart: 2).Select(x => x.Type).ToArray());
            CollectionAssert.AreEqual(new[] { "string", "int" }, DataStreamParser.ParseDataNames("int|string,int", pipePart: 2).Select(x => x.Type).ToArray());

            // removes whitespace?
            CollectionAssert.AreEqual(new[] { "string", "int" }, DataStreamParser.ParseDataNames("int| string , int", pipePart: 2).Select(x => x.Type).ToArray());

            // named parameter
            CollectionAssert.AreEqual(new[] { "string", "int" }, DataStreamParser.ParseDataNames("int| name:string , age:int", pipePart: 2).Select(x => x.Type).ToArray());
            CollectionAssert.AreEqual(new[] { "name", "age" }, DataStreamParser.ParseDataNames("int| name:string , age:int", pipePart: 2).Select(x => x.Name).ToArray());

        }

        [TestMethod]
        public void MethodTest()
        {
            // string return value
            var node = SoftwareCellsManager.CreateNew("Random Name");
            MainModelManager.AddNewOutput(node, "string");

            var methode = Operations.GenerateOperationMethod(_gen.Generator, node);
            Assert.AreEqual("public string Random_Name()\r\n{\r\n}", methode.NormalizeWhitespace().ToFullString());

            // Empty output => return void
            node = SoftwareCellsManager.CreateNew("Random Name");
            MainModelManager.AddNewOutput(node, "");

            methode = Operations.GenerateOperationMethod(_gen.Generator, node);
            Assert.AreEqual("public void Random_Name()\r\n{\r\n}", methode.NormalizeWhitespace().ToFullString());


            var testModel = new MainModel();
            var newNameID = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            var newName = SoftwareCellsManager.GetFirst(newNameID, testModel);
            MainModelManager.AddNewInput(newNameID, "", testModel);

            var alterID = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            var alter = SoftwareCellsManager.GetFirst(alterID, testModel);
            MainModelManager.Connect(newNameID, alterID, "string | ", testModel);

            var personID = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            var person = SoftwareCellsManager.GetFirst(personID, testModel);
            MainModelManager.Connect(alterID, personID, "int | int, string", testModel);

            var definition = DataStreamManager.CreateNewDefinition(person, "Person");
            person.OutputStreams.Add(definition);

            SyntaxNode[] members = testModel.SoftwareCells.Select(x => Operations.GenerateOperationMethod(_gen.Generator, x)).ToArray();
            var newNameMethod = members[0];
            Assert.AreEqual("public string Random_Name()\r\n{\r\n}", newNameMethod.NormalizeWhitespace().ToFullString());

            var alterMethod = members[1];
            Assert.AreEqual("public int Random_Age()\r\n{\r\n}", alterMethod.NormalizeWhitespace().ToFullString());

            var personMethod = members[2];
            Assert.AreEqual("public Person Create_Person(int param1, string param2)\r\n{\r\n}", personMethod.NormalizeWhitespace().ToFullString());

            // Named Parameter
            person.InputStreams.Clear();
            person.InputStreams.Add(new DataStreamDefinition() { DataNames = "int | age:int, name:string" });
            var personMethodNamedParams = Operations.GenerateOperationMethod(_gen.Generator, person);
            Assert.AreEqual("public Person Create_Person(int age, string name)\r\n{\r\n}",
                personMethodNamedParams.NormalizeWhitespace().ToFullString());







        }

        [TestMethod()]
        public void FindParametersTest()
        {

            var testModel = new MainModel();
            var newNameID = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            var newName = SoftwareCellsManager.GetFirst(newNameID, testModel);
            MainModelManager.AddNewInput(newNameID, "", testModel);

            var alterID = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            var alter = SoftwareCellsManager.GetFirst(alterID, testModel);
            MainModelManager.Connect(newNameID, alterID, "string | ", testModel);

            var personID = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            var person = SoftwareCellsManager.GetFirst(personID, testModel);
            MainModelManager.Connect(alterID, personID, "int | int, string", testModel);


            var expectedList = new List<Parameter>()
            {
                new Parameter() {FoundFlag = true, Source = alter},
                new Parameter() {FoundFlag = true, Source = newName}
            };
            var paramList = _gen.FindParameters(testModel.Connections, person);
            Assert.IsTrue(expectedList[0].Compare(paramList[0]));
            Assert.IsTrue(expectedList[1].Compare(paramList[1]));
        }

        [TestMethod()]
        public void FindOneParameterTest()
        {
            var testModel = new MainModel();
            var newNameID = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            var newName = SoftwareCellsManager.GetFirst(newNameID, testModel);
            MainModelManager.AddNewInput(newNameID, "", testModel);

            var alterID = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            var alter = SoftwareCellsManager.GetFirst(alterID, testModel);
            MainModelManager.Connect(newNameID, alterID, "string | ", testModel);

            var personID = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            var person = SoftwareCellsManager.GetFirst(personID, testModel);
            MainModelManager.Connect(alterID, personID, "int | int, string", testModel);

            var expected = new Parameter { FoundFlag = true, Source = alter };
            var lookingfor = new NameType { Name = null, Type = "int" };
            Assert.IsTrue(expected.Compare(_gen.FindOneParameter(lookingfor, testModel.Connections, person)));

            expected = new Parameter { FoundFlag = true, Source = newName };
            lookingfor = new NameType { Name = null, Type = "string" };
            Assert.IsTrue(expected.Compare(_gen.FindOneParameter(lookingfor, testModel.Connections, person)));

        }

        [TestMethod()]
        public void CreateIntegrationBodyTest()
        {
            var testModel = new MainModel();
            var newNameID = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            var newName = SoftwareCellsManager.GetFirst(newNameID, testModel);
            MainModelManager.AddNewInput(newNameID, "", testModel);

            var alterID = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            var alter = SoftwareCellsManager.GetFirst(alterID, testModel);
            MainModelManager.Connect(newNameID, alterID, "string | ", testModel);

            var personID = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            var person = SoftwareCellsManager.GetFirst(personID, testModel);
            MainModelManager.Connect(alterID, personID, "int | int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            var nodes = _gen.CreateIntegrationBody(_gen.Generator, testModel.Connections, testModel.SoftwareCells);
        }

        [TestMethod()]
        public void LocalMethodCallTest()
        {
            // void method call -> no local variable needed
            var testModel = new MainModel();
            var fooID = MainModelManager.AddNewSoftwareCell("foo", testModel);
            var foo = SoftwareCellsManager.GetFirst(fooID, testModel);
            MainModelManager.AddNewInput(foo, "");
            MainModelManager.AddNewOutput(foo, "");
           var nodes = _gen.LocalMethodCall(_gen.Generator, foo, null, new List<GeneratedLocalVariable>());
           var fullstring =nodes.NormalizeWhitespace().ToFullString();
            Assert.AreEqual("foo()", fullstring);
        }
    }
}