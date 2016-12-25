using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Library;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Roslyn.Generators;

namespace Roslyn.Tests
{
    [TestClass()]
    public class IntegrationGeneratorTests
    {

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


            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams.First(), print.InputStreams.First(), testModel);


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
            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams[1], printerror.InputStreams.First(), testModel);
            main.IsIntegrating.AddUnique(printerror);

            List<IntegrationGenerator.LambdaBody> lambdaBodies = new List<IntegrationGenerator.LambdaBody>();
            IntegrationGenerator.TravelIntegration(main, testModel,
                onInLambdaBody: lambdaBodies.Add);

            Assert.IsTrue(lambdaBodies.Any(c => c.FunctionUnit == printerror && c.InsideLambdaOf == onerror));
            Assert.IsTrue(lambdaBodies.Any(c => c.FunctionUnit == print && c.InsideLambdaOf == outperson));
            Assert.IsTrue(lambdaBodies.Any(c => c.FunctionUnit == addage && c.InsideLambdaOf == outperson));

            //MyGenerator mygen = new MyGenerator();
            //var body = IntegrationGenerator.CreateIntegrationBody(mygen.Generator, testModel, main);
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
            MainModelManager.ConnectTwoDefintions(createPersons.OutputStreams[1], printerror.InputStreams.First(), testModel);
            x.IsIntegrating.AddUnique(printerror);

            List<IntegrationGenerator.LambdaBody> callinbodies = new List<IntegrationGenerator.LambdaBody>();
            IntegrationGenerator.TravelIntegration(x, testModel,
                onInLambdaBody: callinbodies.Add);

            Assert.IsTrue(callinbodies.Any(c => c.FunctionUnit == printerror && c.InsideLambdaOf == onerror));
            Assert.IsTrue(callinbodies.Any(c => c.FunctionUnit == print && c.InsideLambdaOf == null));
            Assert.IsTrue(callinbodies.Any(c => c.FunctionUnit == createPersons && c.InsideLambdaOf == null));
            Assert.IsTrue(callinbodies.Any(c => c.FunctionUnit == addage && c.InsideLambdaOf == null));


            //MyGenerator mygen = new MyGenerator();
            //var body = IntegrationGenerator.CreateIntegrationBody(mygen.Generator, testModel, x);
            //var mainnode = MethodsGenerator.MethodDeclaration(mygen.Generator, body, "test", null, null);

            //var code = mainnode.NormalizeWhitespace().ToFullString();

        }

        [TestMethod()]
        public void AssignmentParameter_IncludingLambdaBodiesRecursiveTest()
        {
            Assert.Fail();
        }
    }
}