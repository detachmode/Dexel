using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using SharpFlowDesign.Model;

namespace Roslyn
{

    public class MyGenerator
    {
        public Workspace _workspace = new AdhocWorkspace();
        public  SyntaxGenerator _generator;




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


        public SyntaxNode Method(string createName)
        {
            return _generator.MethodDeclaration(createName, null,
              null, null,
              Accessibility.Public,
              DeclarationModifiers.None,
              new SyntaxNode[] { });
        }

        public SyntaxNode Method(SoftwareCell softwareCell)
        {
            var methodName = softwareCell.Name.Replace(' ', '_');
            var outputDataNames =  softwareCell.OutputStreams.First().DataNames;
            
            //var inputDataNames = softwareCell.InputStreams.First().DataNames;

            var returntype = ConvertToTypeExpression(GetReturnTypes(outputDataNames,pipePart:1).First());

            return _generator.MethodDeclaration(methodName, null,
               null, returntype,
               Accessibility.Public,
               DeclarationModifiers.None,
               new SyntaxNode[] { } );
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


        public string[] GetReturnTypes(string dataNames, int pipePart = 1)
        {
            string inputs;
            if (dataNames.Contains("|"))
            {
                MatchCollection matches = Regex.Matches(dataNames, @"(.*)\|(.*)");
                inputs = matches[0].Groups[pipePart].Value;
            }
            else
            {
                inputs = dataNames;
            }
            
            return inputs.Split(',').Select(x =>
            {
                var splitted = x.Split(':');
                return splitted.Length == 2 ? splitted[1].Trim() : splitted[0].Trim();
            }).ToArray();

            //return _generator.TypeExpression(SpecialType.System_String);
        }
    }
}
