using System;
using System.Linq;
using System.Windows;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using SharpFlowDesign;
using SharpFlowDesign.Model;

namespace Roslyn
{

    internal class Program
    {
        private static void Main(string[] args)
        {
            // Get a workspace
            var workspace = new AdhocWorkspace();


            // Get the SyntaxGenerator for the specified language
            var generator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.CSharp);
            // Create using/Imports directives
            var usingDirectives = generator.NamespaceImportDeclaration("System");



            // Generate two private fields
            //var lastNameField = generator.FieldDeclaration("_lastName",
            //    generator.TypeExpression(SpecialType.System_String),
            //    Accessibility.Private);
            //var firstNameField = generator.FieldDeclaration("_firstName",
            //    generator.TypeExpression(SpecialType.System_String),
            //    Accessibility.Private);


            // Generate two properties with explicit get/set
            //var lastNameProperty = generator.PropertyDeclaration("LastName",
            //    generator.TypeExpression(SpecialType.System_String), Accessibility.Public,
            //    getAccessorStatements: new[]
            //    {generator.ReturnStatement(generator.IdentifierName("_lastName"))},
            //    setAccessorStatements: new[]
            //    {
            //        generator.AssignmentStatement(generator.IdentifierName("_lastName"),
            //            generator.IdentifierName("value"))
            //    });
            //var firstNameProperty = generator.PropertyDeclaration("FirstName",
            //    generator.TypeExpression(SpecialType.System_String),
            //    Accessibility.Public,
            //    getAccessorStatements: new[]
            //    {generator.ReturnStatement(generator.IdentifierName("_firstName"))},
            //    setAccessorStatements: new[]
            //    {
            //        generator.AssignmentStatement(generator.IdentifierName("_firstName"),
            //            generator.IdentifierName("value"))
            //    });
            // Generate the method body for the Clone method
            var cloneMethodBody = generator.ReturnStatement(generator.
              InvocationExpression(generator.IdentifierName("MemberwiseClone")));

            // Generate the Clone method declaration
            var cloneMethoDeclaration = generator.MethodDeclaration("Clone", null,
              null, null,
              Accessibility.Public,
              DeclarationModifiers.Virtual,
              new SyntaxNode[] { cloneMethodBody });

            // Generate a SyntaxNode for the interface's name you want to implement
            var ICloneableInterfaceType = generator.IdentifierName("ICloneable");

            // Explicit ICloneable.Clone implemenation
            var cloneMethodWithInterfaceType = generator.
              AsPublicInterfaceImplementation(cloneMethoDeclaration,
              ICloneableInterfaceType);
            // Generate parameters for the class' constructor
            var constructorParameters = new SyntaxNode[] {
  generator.ParameterDeclaration("LastName",
  generator.TypeExpression(SpecialType.System_String)),
  generator.ParameterDeclaration("FirstName",
  generator.TypeExpression(SpecialType.System_String)) };

            // Generate the constructor's method body
            var constructorBody = new SyntaxNode[] {
  generator.AssignmentStatement(generator.IdentifierName("_lastName"),
  generator.IdentifierName("LastName")),
  generator.AssignmentStatement(generator.IdentifierName("_firstName"),
  generator.IdentifierName("FirstName"))};

            // Generate the class' constructor
            var constructor = generator.ConstructorDeclaration("Person",
              constructorParameters, Accessibility.Public,
              statements: constructorBody);

            

            // An array of SyntaxNode as the class members
            //          var members = new SyntaxNode[] { lastNameField,
            //firstNameField, lastNameProperty, firstNameProperty,
            //cloneMethodWithInterfaceType, constructor };


            // Generate the class
            //var classDefinition = generator.ClassDeclaration(
            //  "Person", typeParameters: null,
            //  accessibility: Accessibility.Public,
            //  modifiers: DeclarationModifiers.Abstract,
            //  baseType: null,
            //  interfaceTypes: new SyntaxNode[] { ICloneableInterfaceType },
            //  members: members);

            // Declare a namespace








            //var nameField = generator.FieldDeclaration("Name",
            //generator.TypeExpression(SpecialType.System_String),
            //Accessibility.Public);

            //var ageField = generator.FieldDeclaration("Age",
            //  generator.TypeExpression(SpecialType.System_Int32),
            //  Accessibility.Public);

            //var personMembers = new SyntaxNode[] { nameField, ageField };
            //var personClass = myGenerator.Class("Person", personMembers);


            //var returtype =  generator.IdentifierName("Person");
            //var bla = generator.MethodDeclaration("bla", null,
            //   null, returtype,
            //   Accessibility.Public,
            //   DeclarationModifiers.None,
            //   new SyntaxNode[] { });
            //var myGenerator = new MyGenerator();

            
            //SyntaxNode[] members = testModel.SoftwareCells.Select(x => myGenerator.Method(x)).ToArray();


            //var interactionsClass = myGenerator.Class("Interactions", members);
            //var namespaceDeclaration = generator.NamespaceDeclaration("MyTypes", interactionsClass);

            // Get a CompilationUnit (code file) for the generated code
            //var newNode = generator.CompilationUnit(usingDirectives, namespaceDeclaration).
            //  NormalizeWhitespace();

           
            //Console.Write(interactionsClass.NormalizeWhitespace().ToFullString());
            //Console.Read();




        }
    }

}