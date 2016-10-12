﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Roslyn.Common;
using SharpFlowDesign.Model;

namespace Roslyn
{
    public class MyGenerator
    {
        public SyntaxGenerator Generator;
        public Workspace _workspace = new AdhocWorkspace();


        public MyGenerator()
        {
            Generator = SyntaxGenerator.GetGenerator(_workspace, LanguageNames.CSharp);
        }


        public SyntaxNode Class(string name, SyntaxNode[] members)
        {
            // Generate the class
            var classDefinition = Generator.ClassDeclaration(
                name, typeParameters: null,
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.None,
                baseType: null,
                interfaceTypes: null,
                members: members
                );
            return classDefinition;
        }


        public SyntaxNode[] CreateIntegrationBody(SyntaxGenerator generator, List<DataStream> connections, List<SoftwareCell> integratedSoftwareCells)
        {
            var generated = new List<GeneratedLocalVariable>();

            var methodWithParameterDependencieses = FindParameterDependencies(integratedSoftwareCells, connections);
            return CreateAllDependenciesAvailable(generator, methodWithParameterDependencieses, generated).ToArray();
        }

        private List<SyntaxNode> CreateAllDependenciesAvailable(SyntaxGenerator generator,
            List<MethodWithParameterDependencies> parameterDependencieses, List<GeneratedLocalVariable> generated)
        {
            var result = GenerateNoParameterMethods(generator, parameterDependencieses, generated);
            result = CreateAllDependenciesAvailable(generator, parameterDependencieses, generated, result).ToList();

            return result;
        }

        private List<SyntaxNode> GenerateNoParameterMethods(SyntaxGenerator generator,
            List<MethodWithParameterDependencies> parameterDependencies, List<GeneratedLocalVariable> generated)
        {
            var noParams = parameterDependencies.Where(x => x.Parameters.Count == 0).ToList();
            parameterDependencies.RemoveAll(x => x.Parameters.Count == 0);
            return noParams.Select(x => LocalMethodCall(generator, x.OfSoftwareCell, null, generated)).ToList();
        }

        private List<SyntaxNode> CreateAllDependenciesAvailable(SyntaxGenerator generator,
            List<MethodWithParameterDependencies> methods, List<GeneratedLocalVariable> generated,
            IEnumerable<SyntaxNode> result)
        {
            if (!methods.Any())
                return result.ToList();

            var nodes = methods.Where(
                methodWithParameterDependencies => CanBeGenerated(generated, methodWithParameterDependencies))
                .Select(
                    methodWithParameterDependencies =>
                        GenerateLocalMethodCall(generator, generated, methodWithParameterDependencies)).ToList();
            var newresult = result.ToList();
             newresult.AddRange(nodes);
            return newresult;
        }

        private static bool CanBeGenerated(List<GeneratedLocalVariable> generated,
            MethodWithParameterDependencies methodWithParameterDependencies)
        {
            return
                methodWithParameterDependencies.Parameters.TrueForAll(
                    param => generated.Any(c => c.Source == param.Source));
        }

        private SyntaxNode GenerateLocalMethodCall(SyntaxGenerator generator, List<GeneratedLocalVariable> generated,
            MethodWithParameterDependencies x)
        {
            return LocalMethodCall(generator, x.OfSoftwareCell,
                CreateParameterAssignment(generator, generated, x.Parameters),
                generated);
        }

        private static SyntaxNode[] CreateParameterAssignment(SyntaxGenerator generator, List<GeneratedLocalVariable> generated, List<Parameter> parameters)
        {
            return parameters.Select(p =>
            {
                var variablename = generated.First(x => x.Source == p.Source).VariableName;
                return generator.IdentifierName(variablename);
            }).ToArray();
        }



        private List<MethodWithParameterDependencies> FindParameterDependencies(List<SoftwareCell> softwareCells,
            List<DataStream> connections)
        {
            return softwareCells.Select(sc => new MethodWithParameterDependencies
            {
                OfSoftwareCell = sc,
                Parameters = FindParameters(connections, sc)
            }).ToList();
        }

        public List<Parameter> FindParameters(List<DataStream> connections, SoftwareCell ofSoftwareCell)
        {
            var nameTypes = DataStreamParser.ParseDataNames(ofSoftwareCell.InputStreams.First().DataNames, pipePart: 2).ToList();
            return nameTypes.Where(nt => nt.Type != "").Select(nt => FindOneParameter(nt, connections, ofSoftwareCell)).ToList();
        }

        public Parameter FindOneParameter(NameType lookingForNameType, List<DataStream> connections, SoftwareCell ofSoftwareCell)
        {
            var dataStream = GetInputConnection(connections, ofSoftwareCell);
            if (dataStream == null)
            {
                return new Parameter
                {
                    FoundFlag = false,
                    Source = null
                };
            }

            var outputNametypes = DataStreamParser.ParseDataNames(dataStream.DataNames, pipePart: 1).ToList();

            var found = outputNametypes.Where(nt => nt.Type == lookingForNameType.Type && nt.Name == lookingForNameType.Name).ToList();
            if (found.Any())
            {
                return new Parameter
                {
                    FoundFlag = true,
                    Source = dataStream.Sources.First()
                };
            }

            return FindOneParameter(lookingForNameType, connections, dataStream.Sources.First());

        }

        private DataStream GetInputConnection(List<DataStream> connections, SoftwareCell ofSoftwareCell)
        {
            var found = connections.Where(c => c.Destinations.Contains(ofSoftwareCell)).ToList();
            return found.Any() ? found.First() : null;
        }


        private SyntaxNode LocalMethodCall(SyntaxGenerator generator, SoftwareCell softwareCell, SyntaxNode[] parameter,
            List<GeneratedLocalVariable> generated)
        {
            var firstouttype = DataStreamParser.ParseDataNames(softwareCell.OutputStreams.First().DataNames).First();

            var localType = DataTypeParser.ConvertToTypeExpression(generator, firstouttype.Type);
            var localName = firstouttype.Name ?? "a" + firstouttype.Type;


            generated.Add(new GeneratedLocalVariable
            {
                VariableName = localName,
                Source = softwareCell
            });

            return Generator.LocalDeclarationStatement(localType, localName,
                Generator.InvocationExpression(Generator.IdentifierName(Operations.GetMethodName(softwareCell)),
                    parameter ?? new SyntaxNode[] { }));
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


    public class NameType
    {
        public string Name, Type;
    }
}