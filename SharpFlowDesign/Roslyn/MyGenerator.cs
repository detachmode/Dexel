﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlowDesignModel;
using Microsoft.CodeAnalysis;
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
            var interactionsClass = Class("Interactions", methods.ToArray());
            var usingDirectives = Generator.NamespaceImportDeclaration("System");
            var namespaceDeclaration = Generator.NamespaceDeclaration("AutoGenerated", interactionsClass);

            CompileAndOutput(usingDirectives, namespaceDeclaration);
        }


        private List<SyntaxNode> GenerateAllMethods(MainModel model)
        {
            var methods = model.SoftwareCells
                .Select(softwareCell => MethodsGenerator.GenerateMethod(Generator, softwareCell))
                .ToList();

            // One main Integration is implicitly integrating all operations
            var body = Integrations.CreateIntegrationBody(Generator, model.Connections, model.SoftwareCells);
            var main = MethodsGenerator.GenerateMethod(Generator, "main", body);
            methods.Add(main);
            return methods;
        }


        private void CompileAndOutput(SyntaxNode usingDirectives, SyntaxNode namespaceDeclaration)
        {
            var newNode = Generator.CompilationUnit(usingDirectives, namespaceDeclaration).
                NormalizeWhitespace();

            try
            {
                File.WriteAllText(@"C:\Users\Dennis\Desktop\autogenerated.cs", newNode.ToFullString());
            }
            catch (Exception)
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
    }


    internal class CreatedLocalMethod
    {
        public string LocalName;
        public string LocalType;
        public SoftwareCell SoftwareCell;
    }


    public class MethodWithParameterDependencies
    {
        public SoftwareCell OfSoftwareCell;
        public List<Parameter> Parameters = new List<Parameter>();
    }


    public class Parameter
    {
        public bool FoundFlag;
        public SoftwareCell Source;
    }




}