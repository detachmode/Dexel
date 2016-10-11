using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using SharpFlowDesign.Model;

namespace Roslyn
{
    public class MyGenerator
    {
        public SyntaxGenerator _generator;
        public Workspace _workspace = new AdhocWorkspace();


        public MyGenerator()
        {
            _generator = SyntaxGenerator.GetGenerator(_workspace, LanguageNames.CSharp);
        }

        public SyntaxNode Class(string name, SyntaxNode[] members)
        {
            // Generate the class
            var classDefinition = _generator.ClassDeclaration(
                name, typeParameters: null,
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.None,
                baseType: null,
                interfaceTypes: null,
                members: members
                );
            return classDefinition;
        }


        public SyntaxNode Method(string createName, SyntaxNode[] body = null)
        {
            return _generator.MethodDeclaration(createName, null,
                null, null,
                Accessibility.Public,
                DeclarationModifiers.None,
                body ?? new SyntaxNode[] {});
        }

        public SyntaxNode Method(SoftwareCell softwareCell)
        {
            var methodName = GetMethodName(softwareCell);
            var returntype = GetReturnPart(softwareCell);
            var parameters = GetParameters(softwareCell);

            ////var constructorParameters = new SyntaxNode[] {
            //      _generator.ParameterDeclaration("LastName",
            //      _generator.TypeExpression(SpecialType.System_String)),


            return _generator.MethodDeclaration(methodName, parameters,
                null, returntype,
                Accessibility.Public,
                DeclarationModifiers.None,
                new SyntaxNode[] {});
        }

        private IEnumerable<SyntaxNode> GetParameters(SoftwareCell softwareCell)
        {
            if (!softwareCell.InputStreams.Any())
                return null;

            var inputDataNames = softwareCell.InputStreams.First().DataNames;
            var i = 0;
            return ParseDataNames(inputDataNames, pipePart: 2)
                    .Where(nameType => ConvertToTypeExpression(nameType.Type) != null)
                    .Select(nametype =>
                    {
                        ++i;
                        var name = nametype.Name ?? "param" + i;                    
                        return _generator.ParameterDeclaration(name, ConvertToTypeExpression(nametype.Type));
                    }).ToArray();
        }

        private static string GetMethodName(SoftwareCell softwareCell)
        {
            return softwareCell.Name.Replace(' ', '_');
        }

        private SyntaxNode GetReturnPart(SoftwareCell softwareCell)
        {
            var outputDataNames = softwareCell.OutputStreams.First().DataNames;
            return ParseDataNames(outputDataNames, pipePart: 1)
                .Select(nameType => ConvertToTypeExpression(nameType.Type))
                .First();
        }


        public SyntaxNode ConvertToTypeExpression(string type)
        {
            switch (type)
            {
                case "":
                    return null;
                case "string":
                    return _generator.TypeExpression(SpecialType.System_String);
                case "int":
                    return _generator.TypeExpression(SpecialType.System_Int32);
                case "double":
                    return _generator.TypeExpression(SpecialType.System_Double);
                default:
                    return _generator.IdentifierName(type);
            }
        }


        public IEnumerable<NameType> ParseDataNames(string dataNames, int pipePart = 1)
        {
            var datanames = GetPipePart(dataNames, pipePart);
            return ConvertToNameTypes(datanames);
        }

        private static IEnumerable<NameType> ConvertToNameTypes(string datanames)
        {
            return datanames.Split(',').Select(x =>
            {
                var splitted = x.Split(':');
                return new NameType()
                {
                    Name = splitted.Length == 2 ? splitted[0].Trim() : null,
                    Type = splitted.Length == 2 ? splitted[1].Trim() : splitted[0].Trim()
                };
            });
        }

        private static string GetPipePart(string dataNames, int pipePart)
        {
            if (!dataNames.Contains("|")) return dataNames;

            var matches = Regex.Matches(dataNames, @"(.*)\|(.*)");
            return matches[0].Groups[pipePart].Value;
        }

        public SyntaxNode[] CreateIntegrationBody(List<DataStream> connections, SoftwareCell startSoftwareCell)
        {
            List<Tuple<NameType,string>> existingLocals = new List<Tuple<NameType, string>>();
           
            var callfirst = LocalMethodCall(startSoftwareCell,connections, existingLocals);

            var nextStreams = connections.Where(conn => conn.Sources.Any(y => y.ID == startSoftwareCell.ID));
            var nextCalls = nextStreams.Select(x => LocalMethodCall(x.Destinations.First())).ToArray();


            var result = new[]
            {
                callfirst,
                //_generator.LocalDeclarationStatement(
                //    _generator.TypeExpression(SpecialType.System_String),
                //    "name",
                //    _generator.LiteralExpression("hello")
                //    ),

                //_generator.LocalDeclarationStatement(
                //    _generator.TypeExpression(SpecialType.System_String),
                //    "name", _generator.InvocationExpression(_generator.IdentifierName("MemberwiseClone")))
            }.ToList();

            result.AddRange(nextCalls);



            return result.ToArray();

            //_generator.VariableDeclaration(
            //   type: _generator.IdentifierName(_generator.Token(SyntaxKind.VarKeyword)),
            //   variables: _generator.SeparatedList(
            //       _generator.VariableDeclarator(
            //           identifier: _generator.Identifier(name)))));

        }

        private SyntaxNode[] GetArguments(SoftwareCell softwareCell, List<DataStream> connections, List<CreatedLocalMethod> existingLocals)
        {
            var end = connections.Where(x => x.Destinations.Contains(softwareCell));
            var nodebefore = end.First().Sources.First();
            existingLocals.Last(x => x.SoftwareCell == nodebefore).LocalName
                            throw new NotImplementedException();
        }

        private SyntaxNode LocalMethodCall(SoftwareCell softwareCell,List<DataStream>connections, List<CreatedLocalMethod> existingLocals)
        {
            SyntaxNode[] arguments  = GetArguments(softwareCell,connections, existingLocals);


            var firstouttype = ParseDataNames(softwareCell.OutputStreams.First().DataNames).First();

            var localType = ConvertToTypeExpression(firstouttype.Type);
            var localName = firstouttype.Name ?? "a" + firstouttype.Type;
            var created = new CreatedLocalMethod
            {
                LocalName = localName,
                LocalType = firstouttype.Type,
                SoftwareCell = softwareCell
            };
            existingLocals.Add(created);

            return _generator.LocalDeclarationStatement(localType,localName,
                _generator.InvocationExpression(_generator.IdentifierName(GetMethodName(softwareCell)),
                arguments ?? new SyntaxNode[] {}));
        }
    }

    internal class CreatedLocalMethod
    {
        public string LocalName, LocalType;
        public SoftwareCell SoftwareCell;

    }


    public class NameType
    {
        public string Name, Type;
    }
}