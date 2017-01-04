using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Roslyn.Analyser;

namespace Roslyn.Generators
{
    public static class Names
    {
        public static string NewAction(DataStreamDefinition dsd)
        {
            string @return = null;
            OutputAnalyser.IsActionNameDefined(dsd, 
                onDefined: () => @return = dsd.ActionName.Replace(".", String.Empty),
                onUndefined: () => @return = GenerateNameOfActionByTypes(dsd.DataNames));

            return @return;
        }


        private static string GenerateNameOfActionByTypes(string rawdatanames)
        {
            var nametypes = DataStreamParser.GetOutputPart(rawdatanames);
            if (nametypes.Count == 1)
            {
                var nt = nametypes.First();
                if (String.IsNullOrWhiteSpace(nt.Name))
                    return  $"on{Helper.FirstCharToUpper(nt.Type)}";
                return $"on{Helper.FirstCharToUpper(nt.Name)}";
            }
            return  "continueWith";

        }


        public static string NewLocalVariable(List<NameType> output)
        {
            if (output.Count == 0)
            {
                return null;
            }
            if (output.Count > 1)
            {
                return "tupel";
            }
            return output.First().Name ?? "a" + output.First().Type;
        }


        public static string MethodName(FunctionUnit functionUnit)
        {
            if (String.IsNullOrEmpty(functionUnit.Name))
                throw new Exception("FunctionUnit has no name");

            return
                functionUnit.Name.Split(' ')
                    .Where(s => !String.IsNullOrEmpty(s))
                    .Select(Helper.FirstCharToUpper)
                    .Aggregate((s, s2) => s + s2);
        }


        public static string ParameterName(NameType nametype)
        {
            var lower = nametype.Type.ToLower();
            if (nametype.IsArray || nametype.IsList)
                lower += "s";
            return nametype.Name ?? $"a{lower}";
        }
    }
}
