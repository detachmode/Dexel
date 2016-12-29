using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dexel.Library;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Analyser;
using static Roslyn.IntegrationGenerator;

namespace Roslyn.Tests.Generators
{
    [TestClass()]
    public class IntegrationGeneratorTests
    {
        private readonly MyGenerator _mygen = new MyGenerator();


        [TestMethod()]
        public void Integration_SingleChildren()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "(Person)");

            var createPersons = MainModelManager.AddNewFunctionUnit("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            MainModelManager.AddNewOutput(createPersons, "(Person)");


            //var body = IntegrationGenerator.TravelOutputs(x, testModel);

            //Assert.AreEqual(1, body.Expressions.Count);
            //body.Expressions.First().TryCast<Call>(call => Assert.AreEqual("Person", call.ReturnToVar.Type));
            //body.Expressions.First().TryCast<Call>(call => Assert.AreEqual("aPerson", call.ReturnToVar.VariableName));
        }


        [TestMethod]
        public void FindParametersTest()
        {
            // unnamed syntax test
            var testModel = new MainModel();
            var newName = MainModelManager.AddNewFunctionUnit("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            var alter = MainModelManager.AddNewFunctionUnit("Random Age", testModel);
            MainModelManager.ConnectTwoFunctionUnits(newName, alter, "string", "", testModel);

            var person = MainModelManager.AddNewFunctionUnit("Create Person", testModel);
            MainModelManager.ConnectTwoFunctionUnits(alter, person, "int", "int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            var dependecies = FlowAnalyser.FindParameters(person, testModel.Connections, newName);
            Assert.IsTrue(dependecies.Any(x => x.Source.Parent == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source.Parent == newName));

            // ...  syntax test
            testModel = new MainModel();
            newName = MainModelManager.AddNewFunctionUnit("Random Name", testModel);
            MainModelManager.AddNewInput(newName, "");

            alter = MainModelManager.AddNewFunctionUnit("Random Age", testModel);
            MainModelManager.ConnectTwoFunctionUnits(newName, alter, "string", "", testModel);

            person = MainModelManager.AddNewFunctionUnit("Create Person", testModel);
            MainModelManager.ConnectTwoFunctionUnits(alter, person, "int", "int, string", testModel);
            MainModelManager.AddNewOutput(person, "Person");

            dependecies = FlowAnalyser.FindParameters(person, testModel.Connections, newName);
            Assert.IsTrue(dependecies.Any(x => x.Source.Parent == alter));
            Assert.IsTrue(dependecies.Any(x => x.Source.Parent == newName));
        }


        [TestMethod]
        public void FindParameters_From_Action_DSD()
        {
            // unnamed syntax test
            var testModel = new MainModel();

            var person = MainModelManager.AddNewFunctionUnit("Create Person", testModel);
            MainModelManager.AddNewInput(person, "()");
            var outperson = MainModelManager.AddNewOutput(person, "(Person)", actionName: "onPerson");


            var addage = MainModelManager.AddNewFunctionUnit("Add Age", testModel);
            var inAge = MainModelManager.AddNewInput(addage, "(Person)");
            MainModelManager.AddNewOutput(addage, "(Person)");

            MainModelManager.ConnectTwoDefintions(outperson, inAge, testModel);

            var dependecies = FlowAnalyser.FindParameters(addage, testModel.Connections, null);
            var firstdep = dependecies.First();
            Assert.AreEqual(outperson, firstdep.Source);
        }


        [TestMethod]
        public void FindParameters_From_Parent()
        {
            // unnamed syntax test
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            var countinParent = MainModelManager.AddNewInput(main, "(count:int)");
            MainModelManager.AddNewOutput(main, "()");

            var person = MainModelManager.AddNewFunctionUnit("Create Person", testModel);
            MainModelManager.AddNewInput(person, "(count:int)");
            //var outperson = MainModelManager.AddNewOutput(person, "(Person)", actionName:"onPerson");
            main.IsIntegrating.Add(person);

            var dependecies = FlowAnalyser.FindParameters(person, testModel.Connections, main);
            var firstdep = dependecies.First();
            Assert.AreEqual(countinParent, firstdep.Source);
        }


        [TestMethod]
        public void FindParameters_NoDependecies()
        {
            // unnamed syntax test
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            var countinParent = MainModelManager.AddNewInput(main, "(count:int)");
            MainModelManager.AddNewOutput(main, "()");

            var person = MainModelManager.AddNewFunctionUnit("Create Person", testModel);
            MainModelManager.AddNewInput(person, "()");
            var outperson = MainModelManager.AddNewOutput(person, "(Person)", actionName: "onPerson");
            main.IsIntegrating.Add(person);

            var dependecies = FlowAnalyser.FindParameters(person, testModel.Connections, main);
            Assert.AreEqual(0, dependecies.Count);
        }


        [TestMethod]
        public void TwoOptionalOutputs()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewFunctionUnit("X", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "()");

            var createPersons = MainModelManager.AddNewFunctionUnit("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            MainModelManager.AddNewOutput(createPersons, "(Person)*", actionName: "onPerson");

            var checkage = MainModelManager.AddNewFunctionUnit("Check Age", testModel);
            MainModelManager.AddNewInput(checkage, "(Person)*");
            var onadult = MainModelManager.AddNewOutput(checkage, "(Person)*", actionName: "onAdult");
            var onchild = MainModelManager.AddNewOutput(checkage, "(Person)*", actionName: "onChild");

            var print = MainModelManager.AddNewFunctionUnit("Print", testModel);
            MainModelManager.AddNewInput(print, "(Person)*");
            var printout =  MainModelManager.AddNewOutput(print, "()");

            var checkout = MainModelManager.AddNewFunctionUnit("CheckOut", testModel);
            var checkoutIn =  MainModelManager.AddNewInput(checkout, "(Person)*");
            MainModelManager.AddNewOutput(checkout, "()");

            var printchild = MainModelManager.AddNewFunctionUnit("Print", testModel);
            MainModelManager.AddNewInput(printchild, "(Person)*");
            MainModelManager.AddNewOutput(printchild, "()");


            MainModelManager.ConnectTwoDefintions(printout, checkoutIn, testModel);

            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams.First(),
                checkage.InputStreams.First(), testModel);

            MainModelManager.ConnectTwoDefintions(onadult,
                print.InputStreams.First(), testModel);

            MainModelManager.ConnectTwoDefintions(onchild,
                printchild.InputStreams.First(), testModel);

            x.IsIntegrating.AddUnique(createPersons);
            x.IsIntegrating.AddUnique(checkage);
            x.IsIntegrating.AddUnique(checkout);
            x.IsIntegrating.AddUnique(print);
            x.IsIntegrating.AddUnique(printchild);

            var res = GenerateIntegrationBody(_mygen.Generator, testModel, x);
            var formatted = _mygen.CompileToString(res.ToList());

            Assert.AreEqual(
                "CreatePersons(onPerson: person => {\r\n    CheckAge(person, onAdult: person2 => {\r\n        Print(person2);\r\n        CheckOut(person2);\r\n    }, onChild: person3 => {\r\n        Print(person3);\r\n    });\r\n})",
                formatted);
        }


        [TestMethod]
        public void MakeIntegration_passActionFromIntegrationToOperation()
        {
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(main, "()");
            var intOut = MainModelManager.AddNewOutput(main, "(string)", actionName: "onError");

            var trySomething = MainModelManager.AddNewFunctionUnit("try something", testModel);
            MainModelManager.AddNewInput(trySomething, "()");
            var OperationOut = MainModelManager.AddNewOutput(trySomething, "(string)", actionName: "onError");

            main.IsIntegrating.AddUnique(trySomething);


            IntegrationBody body = new IntegrationBody();
            body.Integration = main;

            IntegrationAnalyser.AnalyseMatchingOutputOfIntegration(body, testModel);

            Assert.AreEqual(1, body.OutputOfIntegration.Count);
            Assert.AreEqual(intOut, body.OutputOfIntegration.First().IntegrationOutput);
            Assert.AreEqual(OperationOut, body.OutputOfIntegration.First().SubFunctionUnitOutput);


            var res = GenerateIntegrationBody(_mygen.Generator, testModel, main);
            var formatted = _mygen.CompileToStrings(res.ToList());


            Assert.AreEqual("TrySomething(onError)", formatted[0]);
        }


        [TestMethod]
        public void MakeIntegration_ReturnRightVariable()
        {
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(main, "()");
            var intOut = MainModelManager.AddNewOutput(main, "(string)");

            var trySomething = MainModelManager.AddNewFunctionUnit("create string", testModel);
            MainModelManager.AddNewInput(trySomething, "()");
            var OperationOut = MainModelManager.AddNewOutput(trySomething, "(string)");

            main.IsIntegrating.AddUnique(trySomething);


            IntegrationBody body = new IntegrationBody();
            body.Integration = main;


            var res = GenerateIntegrationBody(_mygen.Generator, testModel, main);
            var formatted = _mygen.CompileToStrings(res.ToList());

            Assert.AreEqual("return CreateString();", formatted[0]);
            Assert.AreEqual(1, formatted.Length);
        }




        [TestMethod]
        public void MakeIntegration_CreateAtReturnLocalVariable()
        {




            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(main, "()");
            var intOut = MainModelManager.AddNewOutput(main, "(string)");

            var first = MainModelManager.AddNewFunctionUnit("first", testModel);
            MainModelManager.AddNewInput(first, "()");
            var optionalOut = MainModelManager.AddNewOutput(first, "()", actionName: "onOptional");


            var second = MainModelManager.AddNewFunctionUnit("create string", testModel);
            var secondIn =MainModelManager.AddNewInput(second, "()");
            MainModelManager.AddNewOutput(second, "(string)");

            MainModelManager.ConnectTwoDefintions(optionalOut, secondIn, testModel);

                main.IsIntegrating.AddUnique(first);
                main.IsIntegrating.AddUnique(second);

                //var generator = _mygen.Generator;
                //    //QualifiedName(generator.IdentifierName("b") ,generator.IdentifierName("c"));


                //var testresult = test.NormalizeWhitespace().ToFullString();


            // analyse data flow 
            var integrationBody = IntegrationAnalyser.CreateNewIntegrationBody(testModel.Connections, main);
            AddIntegrationInputParameterToLocalScope(integrationBody, main);
            IntegrationAnalyser.AnalyseParameterDependencies(integrationBody);
            IntegrationAnalyser.AnalyseLambdaBodies(integrationBody, testModel);
            IntegrationAnalyser.AnalyseMatchingOutputOfIntegration(integrationBody, testModel);
            IntegrationAnalyser.AnalyseReturnToLocalReturnVariable(integrationBody, testModel);

            Assert.AreEqual(1, integrationBody.ReturnToLocalReturnVariable.Count);



            var res = GenerateIntegrationBody(_mygen.Generator, testModel, main);
            var formatted = _mygen.CompileToStrings(res.ToList());

            Assert.AreEqual("string @return = null;", formatted[0]);
            Assert.AreEqual(3, formatted.Length);
            Assert.AreEqual("First(onOptional: () => {\r\n    @return = CreateString();\r\n})", formatted[1]);
            Assert.AreEqual("return @return;", formatted[2]);
        }


        [TestMethod]
        public void InnerStreamOnly()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewFunctionUnit("X", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "(int)");

            var createPersons = MainModelManager.AddNewFunctionUnit("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            MainModelManager.AddNewOutput(createPersons, "(Person)*");

            var addAge = MainModelManager.AddNewFunctionUnit("Add Age", testModel);
            MainModelManager.AddNewInput(addAge, "(Person)*");
            MainModelManager.AddNewOutput(addAge, "()");

            var addName = MainModelManager.AddNewFunctionUnit("Add Name", testModel);
            MainModelManager.AddNewInput(addName, "(Person)*");
            MainModelManager.AddNewOutput(addName, "()");

            var sumAges = MainModelManager.AddNewFunctionUnit("Sum Ages", testModel);
            MainModelManager.AddNewInput(sumAges, "(Person)*");
            MainModelManager.AddNewOutput(sumAges, "(int)");


            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams.First(),
                addAge.InputStreams.First(), testModel);

            MainModelManager.ConnectTwoDefintions(addAge.OutputStreams.First(),
                addName.InputStreams.First(), testModel);

            MainModelManager.ConnectTwoDefintions(addName.OutputStreams.First(),
                sumAges.InputStreams.First(), testModel);

            x.IsIntegrating.AddUnique(createPersons);
            x.IsIntegrating.AddUnique(addAge);
            x.IsIntegrating.AddUnique(addName);
            x.IsIntegrating.AddUnique(sumAges);

            var res = GenerateIntegrationBody(_mygen.Generator, testModel, x);
            var formatted = _mygen.CompileToString(res.ToList());

            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons.*AddAge.*", RegexOptions.Singleline));

            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons\(person =>.*\S* AddAge\(person\);.*AddName\(person\);.*",
                    RegexOptions.Singleline));

            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons\(person =>.*\S* AddAge\(person\);.*AddName\(person\);.*SumAges\(person\);.*",
                    RegexOptions.Singleline));
        }


        [TestMethod]
        public void StreamOutput()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewFunctionUnit("X", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "(Person)*");

            var createPersons = MainModelManager.AddNewFunctionUnit("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            MainModelManager.AddNewOutput(createPersons, "(Person)*");

            var addAge = MainModelManager.AddNewFunctionUnit("Add Age", testModel);
            MainModelManager.AddNewInput(addAge, "(Person)*");
            MainModelManager.AddNewOutput(addAge, "(Person)*");


            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams.First(),
                addAge.InputStreams.First(), testModel);

            x.IsIntegrating.Add(createPersons);
            x.IsIntegrating.Add(addAge);


            var res = GenerateIntegrationBody(_mygen.Generator, testModel, x);
            var formatted = _mygen.CompileToString(res.ToList());

            Assert.IsTrue(
                Regex.IsMatch(
                    formatted,
                    @".*CreatePersons.*AddAge.*", RegexOptions.Singleline));


            Assert.AreEqual("CreatePersons(person => {\r\n    onPerson(AddAge(person));\r\n})", formatted);
        }


        [TestMethod()]
        public void GenerateBodyTest()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "()");

            var createPersons = MainModelManager.AddNewFunctionUnit("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            MainModelManager.AddNewOutput(createPersons, "(Person)*");

            var print = MainModelManager.AddNewFunctionUnit("Print", testModel);
            MainModelManager.AddNewInput(print, "(Person)*");
            MainModelManager.AddNewOutput(print, "()");


            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams.First(), print.InputStreams.First(),
                testModel);


            //x.IsIntegrating.AddUnique(createPersons);
            //x.IsIntegrating.AddUnique(print);

            //var body = IntegrationGenerator.TravelOutputs(x, testModel);

            //Call firstcall = null;
            //body.Expressions[0].TryCast<Call>(call => firstcall = call);

            //Assert.AreEqual(null, firstcall.ReturnToVar);
            //Assert.AreEqual("CreatePersons", firstcall.Methodname);

            //firstcall.Parameter.First().TryCast<LambdaExpression>(
            //    expression => Assert.AreEqual("person", expression.LambdaArguments.First()));

            //IntegrationBody lambdaBody = null;
            //body.Expressions[0].TryCast<Call>(call => call.Parameter.First().TryCast<LambdaExpression>(
            //    expression => lambdaBody = expression.LambdaBody));


            //lambdaBody.Expressions.First().TryCast<Call>(
            //    call => Assert.AreEqual("Print", call.Methodname));

            //lambdaBody.Expressions.First().TryCast<Call>(
            //    call => Assert.AreEqual("person", call.Parameter.First()));
        }


        [TestMethod()]
        public void TravelOutputsTest_TwoCallsInsideLambda()
        {
            var testModel = new MainModel();
            var main = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(main, "()");
            MainModelManager.AddNewOutput(main, "()");

            var createPersons = MainModelManager.AddNewFunctionUnit("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            var outperson = MainModelManager.AddNewOutput(createPersons, "(Person)*");
            var onerror = MainModelManager.AddNewOutput(createPersons, "(string)", actionName: "onError");
            main.IsIntegrating.AddUnique(createPersons);


            var addage = MainModelManager.AddNewFunctionUnit("Add Age", testModel);
            var addagein = MainModelManager.AddNewInput(addage, "(Person)*");
            var addageout = MainModelManager.AddNewOutput(addage, "(Person)*");
            MainModelManager.ConnectTwoDefintions(outperson, addagein, testModel);
            main.IsIntegrating.AddUnique(addage);


            var print = MainModelManager.AddNewFunctionUnit("Print", testModel);
            MainModelManager.AddNewInput(print, "(Person)*");
            MainModelManager.AddNewOutput(print, "()");
            MainModelManager.ConnectTwoDefintions(addageout, print.InputStreams.First(), testModel);
            main.IsIntegrating.AddUnique(print);


            var printerror = MainModelManager.AddNewFunctionUnit("PrintError", testModel);
            MainModelManager.AddNewInput(printerror, "(string)");
            MainModelManager.AddNewOutput(printerror, "()");
            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams[1], printerror.InputStreams.First(),
                testModel);
            main.IsIntegrating.AddUnique(printerror);

            List<LambdaBody> lambdaBodies = new List<LambdaBody>();
            FlowAnalyser.TravelIntegration(main, testModel,
                onInLambdaBody: lambdaBodies.Add);

            Assert.IsTrue(lambdaBodies.Any(c => c.FunctionUnit == printerror && c.InsideLambdaOf == onerror));
            Assert.IsTrue(lambdaBodies.Any(c => c.FunctionUnit == print && c.InsideLambdaOf == outperson));
            Assert.IsTrue(lambdaBodies.Any(c => c.FunctionUnit == addage && c.InsideLambdaOf == outperson));

            //MyGenerator mygen = new MyGenerator();
            //var body = IntegrationGenerator.GenerateIntegrationBody(mygen.Generator, testModel, main);
            //var mainnode = MethodsGenerator.GenerateStaticMethod(mygen.Generator, main, body);

            //var code = mainnode.NormalizeWhitespace().ToFullString();
        }


        [TestMethod()]
        public void TravelOutputsTest()
        {
            var testModel = new MainModel();
            var x = MainModelManager.AddNewFunctionUnit("main", testModel);
            MainModelManager.AddNewInput(x, "()");
            MainModelManager.AddNewOutput(x, "()");

            var createPersons = MainModelManager.AddNewFunctionUnit("Create Persons", testModel);
            MainModelManager.AddNewInput(createPersons, "()");
            var outperson = MainModelManager.AddNewOutput(createPersons, "(Person)");
            var onerror = MainModelManager.AddNewOutput(createPersons, "(string)", actionName: "onError");
            x.IsIntegrating.AddUnique(createPersons);


            var addage = MainModelManager.AddNewFunctionUnit("Add Age", testModel);
            var addagein = MainModelManager.AddNewInput(addage, "(Person)");
            var addageout = MainModelManager.AddNewOutput(addage, "(Person)");
            MainModelManager.ConnectTwoDefintions(outperson, addagein, testModel);
            x.IsIntegrating.AddUnique(addage);


            var print = MainModelManager.AddNewFunctionUnit("Print", testModel);
            MainModelManager.AddNewInput(print, "(Person)");
            MainModelManager.AddNewOutput(print, "()");
            MainModelManager.ConnectTwoDefintions(addageout, print.InputStreams.First(), testModel);
            x.IsIntegrating.AddUnique(print);


            var printerror = MainModelManager.AddNewFunctionUnit("PrintError", testModel);
            MainModelManager.AddNewInput(printerror, "(string)");
            MainModelManager.AddNewOutput(printerror, "()");
            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams[1], printerror.InputStreams.First(),
                testModel);
            x.IsIntegrating.AddUnique(printerror);

            List<LambdaBody> callinbodies = new List<LambdaBody>();
            FlowAnalyser.TravelIntegration(x, testModel,
                onInLambdaBody: callinbodies.Add);

            Assert.IsTrue(callinbodies.Any(c => c.FunctionUnit == printerror && c.InsideLambdaOf == onerror));
            Assert.IsTrue(callinbodies.Any(c => c.FunctionUnit == print && c.InsideLambdaOf == null));
            Assert.IsTrue(callinbodies.Any(c => c.FunctionUnit == createPersons && c.InsideLambdaOf == null));
            Assert.IsTrue(callinbodies.Any(c => c.FunctionUnit == addage && c.InsideLambdaOf == null));


            //MyGenerator mygen = new MyGenerator();
            //var body = IntegrationGenerator.GenerateIntegrationBody(mygen.Generator, testModel, x);
            //var mainnode = MethodsGenerator.MethodDeclaration(mygen.Generator, body, "test", null, null);

            //var code = mainnode.NormalizeWhitespace().ToFullString();
        }


        //[TestMethod()]
        //public void GenereteArgumentsFromInputTest()
        //{
        //    var testModel = new MainModel();

        //    var main = MainModelManager.AddNewFunctionUnit("main", testModel);
        //    MainModelManager.AddNewInput(main, "()");
        //    MainModelManager.AddNewOutput(main, "()");


        //    var x = MainModelManager.AddNewFunctionUnit("x", testModel);
        //    MainModelManager.AddNewInput(x, "()");
        //    var optionout = MainModelManager.AddNewOutput(x, "(int,string)", actionName:"onOutput");


        //    var y = MainModelManager.AddNewFunctionUnit("y", testModel);
        //    var input = MainModelManager.AddNewInput(y, "(int,string)");
        //    MainModelManager.AddNewOutput(y, "()");

        //    main.IsIntegrating.Add(x);


        //    main.IsIntegrating.Add(y);

        //    MainModelManager.ConnectTwoDefintions(optionout, input, testModel);

        //    var mygen = new MyGenerator();
        //    IntegrationGenerator.GenerateIntegrationBody(mygen.Generator, testModel, main);

        //    var body = IntegrationGenerator.CreateNewIntegrationBody(testModel.Connections, main );
        //    IntegrationGenerator.AddIntegrationParameterToLocalScope(body, main);
        //    IntegrationGenerator.AnalyseParameterDependencies(body);

        //    var calldep = body.CallDependecies.First(cd => cd.OfFunctionUnit == y);


        //}
    }
}