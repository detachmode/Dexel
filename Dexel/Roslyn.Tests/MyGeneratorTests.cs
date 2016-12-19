using Roslyn;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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
        private readonly MyGenerator _mygen = new MyGenerator();


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

            var methode = MethodsGenerator.GenerateStaticMethod(_mygen.Generator, node);
            Assert.AreEqual("public static string RandomName()\r\n{\r\n}", methode.NormalizeWhitespace().ToFullString());

            // Empty output => return void
            node = SoftwareCellsManager.CreateNew("Random Name");
            MainModelManager.AddNewOutput(node, "");

            methode = MethodsGenerator.GenerateStaticMethod(_mygen.Generator, node);
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

            SyntaxNode[] members = testModel.SoftwareCells.Select(x => MethodsGenerator.GenerateStaticMethod(_mygen.Generator, x)).ToArray();
            var newNameMethod = members[0];
            Assert.AreEqual("public static string RandomName()\r\n{\r\n}", newNameMethod.NormalizeWhitespace().ToFullString());

            var alterMethod = members[1];
            Assert.AreEqual("public static int RandomAge()\r\n{\r\n}", alterMethod.NormalizeWhitespace().ToFullString());

            var personMethod = members[2];
            Assert.AreEqual("public static Person CreatePerson(int @int, string @string)\r\n{\r\n}", personMethod.NormalizeWhitespace().ToFullString());

            // Named Parameter
            person.InputStreams.Clear();
            SoftwareCellsManager.NewInputDef(person, "int | age:int, name:string", "");
            var personMethodNamedParams = MethodsGenerator.GenerateStaticMethod(_mygen.Generator, person);
            Assert.AreEqual("public static Person CreatePerson(int age, string name)\r\n{\r\n}",
                personMethodNamedParams.NormalizeWhitespace().ToFullString());







        }

        [TestMethod()]
        public void FindParametersTest()
        {

            var testModel = new MainModel();
            var main = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(main, "");
            MainModelManager.AddNewOutput(main, "");

            var alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.ConnectTwoCells(main, alter, "string", "", testModel);

            var person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.ConnectTwoCells(alter, person, "int", "int, string", testModel);
            MainModelManager.AddNewOutput(person, "person");

            var expectedList = new List<Parameter>()
            {
                new Parameter() {FoundFlag = Found.FoundInPreviousChild, Source = alter},
                new Parameter() {FoundFlag = Found.FoundInPreviousChild, Source = main}
            };
            var paramList = Integrations.FindParameters(person, testModel.Connections, main);
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


            paramList = Integrations.FindParameters(splitter, testModel.Connections, main);
            Assert.IsTrue(paramList.First().FoundFlag == Found.FromParent && paramList.First().Source == main);


            testModel = new MainModel();
            main = MainModelManager.AddNewSoftwareCell("convert roman number", testModel);
            MainModelManager.AddNewInput(main, "string");
            MainModelManager.AddNewOutput(main, "int");

            // should not find any           
            splitter = MainModelManager.AddNewSoftwareCell("split", testModel);
            MainModelManager.AddNewInput(splitter, "int");
            MainModelManager.AddNewOutput(splitter, "char*");

            main.Integration.Add(splitter);


            paramList = Integrations.FindParameters(splitter, testModel.Connections, main);
            Assert.IsFalse(paramList.First().FoundFlag == Found.FoundInPreviousChild && paramList.First().Source == null);

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
            MainModelManager.ConnectTwoCells(alter, person, "int", "int, string", testModel);

            var expected = new Parameter { FoundFlag = Found.FoundInPreviousChild, Source = alter };
            var lookingfor = new NameType { Name = null, Type = "int" };
            Assert.IsTrue(expected.Compare(Integrations.FindOneParameter(lookingfor, null, testModel.Connections, person, true)));

            expected = new Parameter { FoundFlag = Found.FoundInPreviousChild, Source = newName };
            lookingfor = new NameType { Name = null, Type = "string" };
            Assert.IsTrue(expected.Compare(Integrations.FindOneParameter(lookingfor, null, testModel.Connections, person, true)));

            testModel = new MainModel();
            newName = MainModelManager.AddNewSoftwareCell("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            alter = MainModelManager.AddNewSoftwareCell("Random Age", testModel);
            MainModelManager.ConnectTwoCells(newName, alter, "string", "", testModel);

            person = MainModelManager.AddNewSoftwareCell("Create Person", testModel);
            MainModelManager.ConnectTwoCells(alter, person, "int", "int, string", testModel);

            lookingfor = new NameType { Name = null, Type = "int" };
            Assert.IsTrue(Integrations.FindOneParameter(lookingfor, null, testModel.Connections, person, true).Source == alter);


        }



        [TestMethod()]
        public void GenerateDataTypesTest()
        {
            var testModel = new MainModel();
            var dt = new CustomDataType { Name = "Person" };
            var subdt = new SubDataType
            {
                Name = "Name",
                Type = "string"
            };
            dt.SubDataTypes = new List<SubDataType> { subdt };
            testModel.DataTypes.Add(dt);

            var gen = new MyGenerator();
            var res = gen.GenerateDataTypes(testModel);
            var str = gen.CompileToString(res.ToList());
            Assert.AreEqual("public class Person\r\n{\r\n    public string Name;\r\n}", str);

        }

        [TestMethod()]
        public void GenerateAllMethodsTest()
        {
            SingleMethod();
            SingleMethodWithStreamAsOutput();
            SingleMethodWithStreamAsOutputAndInputs();
        }

        [TestMethod()]
        public void SingleMethodWithStreamAsOutputAndInputs()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewSoftwareCell("X", testModel);
            MainModelManager.AddNewInput(x, "(int, string)");
            MainModelManager.AddNewOutput(x, "(Person)*");

            var formatted = _mygen.GenerateAllMethods(testModel).First().NormalizeWhitespace().ToFullString();
            Assert.IsTrue(Regex.IsMatch(formatted, @"^public static void X\(int \S*, string \S*, Action<Person> \S*\).*"));
        }


        [TestMethod()]
        public void SingleMethod()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewSoftwareCell("X", testModel);
            MainModelManager.AddNewInput(x, "(int)");
            MainModelManager.AddNewOutput(x, "(Person)");
            var formatted = _mygen.GenerateAllMethods(testModel).First().NormalizeWhitespace().ToFullString();
            Assert.IsTrue(Regex.IsMatch(formatted, @"^public static Person X\(int \S*\).*"));
        }

        [TestMethod()]
        public void SingleMethodWithStreamAsOutput()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewSoftwareCell("X", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "(Person)*");

            var res = _mygen.GenerateAllMethods(testModel);
            string formatted = res.First().NormalizeWhitespace().ToFullString();
            string expected =
                "public static void X(Action<Person> continueWith)\r\n{\r\n    throw  new NotImplementedException();\r\n}";

            Assert.IsTrue(Regex.IsMatch(formatted, "^public static void X.*"));
            Assert.IsTrue(Regex.IsMatch(formatted, @".*X\(Action<Person> \S*\).*"));
        }
    }
}