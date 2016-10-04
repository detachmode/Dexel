using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Roslyn
{

    public class MyGenerator
    {
        private readonly Workspace _workspace = new AdhocWorkspace();
        private readonly SyntaxGenerator _generator;


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


    }
}
