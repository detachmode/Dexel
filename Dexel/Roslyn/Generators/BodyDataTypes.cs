using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model.Manager;

namespace Roslyn.Generators
{
    public class ArgumentAssignment
    {
        public string ArgumentName;
    }

    public class Var
    {
        public string VariableName;
        public List<NameType> Type;
    }

    public class Call
    {
        public Var ReturnToVar;
        public string Methodname;
        public List<ArgumentAssignment> Parameter;
    }

    public class IntegrationBody
    {
        public List<object> Expressions;
    }

    public class LambdaExpression : ArgumentAssignment
    {
        public List<string> LambdaArguments;
        public IntegrationBody LambdaBody;
    }

    public class LocalVariableArgument : ArgumentAssignment
    {
        public string Name;
    }
}
