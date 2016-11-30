﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn
{

    public class MyGenerator
    {
        private readonly Workspace _workspace = new AdhocWorkspace();
        public SyntaxGenerator Generator;
        public MyGenerator()
        {
            Generator = SyntaxGenerator.GetGenerator(_workspace, LanguageNames.CSharp);
        }


        public void GenerateCodeAndPrint(MainModel model)
        {
            var methods = GenerateAllMethods(model);
            var datatypes = GenerateDataTypes(model);
            
            var interactionsClass = Class("Interactions", datatypes.Concat(methods).ToArray());
            var usingDirectives = Generator.NamespaceImportDeclaration("System");
            var namespaceDeclaration = Generator.NamespaceDeclaration("AutoGenerated", interactionsClass);

            CompileAndOutput(usingDirectives, namespaceDeclaration);
        }



        public IEnumerable<SyntaxNode> GenerateDataTypes(MainModel model)
        {
            return model.DataTypes.Select(dt =>
            {
                var body = DataTypesGenerator.GenerateFields(Generator, dt);
                return Class(Helper.FirstCharToUpper(dt.Name), body.ToArray());
            });
        }


        public string GenerateMethods(MainModel mainModel)
        {
            var methods = GenerateAllMethods(mainModel);
            return CompileToString(methods);
        }

        


        public List<SyntaxNode> GenerateAllMethods(MainModel model)
        {
            var operations = GeneratedOperations(model);
            return GenerateIntegrations(operations, model);
        }




        private List<SyntaxNode> GenerateIntegrations(List<SyntaxNode> operations, MainModel model)
        {
            model.SoftwareCells.Where(sc => sc.Integration.Count > 0).ToList().ForEach(isc =>
            {
                var body = Integrations.CreateIntegrationBody(Generator, model.Connections, isc);
                var main = MethodsGenerator.GenerateStaticMethod(Generator, isc, body);
                operations.Add(main);
            });
            return operations;
        }


        private List<SyntaxNode> GeneratedOperations(MainModel mainModel)
        {
            var operationBody = MethodsGenerator.GetNotImplementatedException(Generator);
            return mainModel.SoftwareCells
                .Where(softwareCell => softwareCell.Integration.Count == 0)
                .Select(softwareCell => MethodsGenerator.GenerateStaticMethod(Generator, softwareCell, operationBody))
                .ToList();

        }


        public string CompileToString(List<SyntaxNode> nodes)
        {
            var res = nodes.Select(n => n.NormalizeWhitespace().ToFullString()).Aggregate((f,s) => f + "\n\n" + s);
            return res;
        }


        private void CompileAndOutput(SyntaxNode usingDirectives, SyntaxNode namespaceDeclaration)
        {
            var newNode = Generator.CompilationUnit(usingDirectives, namespaceDeclaration).
                NormalizeWhitespace();

            try
            {
                File.WriteAllText(@"C:\Users\Dennis\Desktop\autogenerated.cs", newNode.ToFullString());
            }
            catch
            {
                // ignored
            }

            Console.Write(newNode.ToFullString());
        }


        public SyntaxNode Class(string name, SyntaxNode[] members)
        {
            // Generate the class
            var classDefinition = Generator.ClassDeclaration(
                name, null,
                Accessibility.Public,
                DeclarationModifiers.None,
                null,
                null,
                members
                );
            return classDefinition;
        }

    }


    public class GeneratedLocalVariable
    {
        public SoftwareCell Source;
        public string VariableName;
        public IEnumerable<NameType> NameTypes;
    }


    //internal class CreatedLocalMethod
    //{
    //    public string LocalName = string.Empty;
    //    public string LocalType = string.Empty;
    //    public ISoftwareCell SoftwareCell = null;
    //}


    public class MethodWithParameterDependencies
    {
        public SoftwareCell OfSoftwareCell;
        public List<Parameter> Parameters = new List<Parameter>();
    }


    public enum Found
    {
        NotFound,
        FromParent,
        FoundInPreviousChild
    }


    public class Parameter
    {
        public Found FoundFlag;
        public SoftwareCell Source;
        public NameType NeededNameType;
        public bool FromAction { get; set; }
    }




}