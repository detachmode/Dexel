using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;

namespace Roslyn.Analyser
{
    public static class OutputAnalyser
    {
        public static void IsActionNameDefined(DataStreamDefinition dsd, Action onDefined, Action onUndefined)
        {
            if (string.IsNullOrWhiteSpace(dsd.ActionName))
                onUndefined();
            else
                onDefined();
        }


        public static List<MethodSignaturePart> AnalyseOutputs(FunctionUnit functionUnit)
        {
            var result = new List<MethodSignaturePart>();

            var copyOfOutputs = functionUnit.OutputStreams.ToList();
            DataStreamParser.OutputByReturn(functionUnit, dsdByReturn =>
            {
                result.Add(new MethodSignaturePart
                {
                    DSD = dsdByReturn,
                    ImplementWith = DataFlowImplementationStyle.AsReturn
                });
                copyOfOutputs.Remove(dsdByReturn);
            });

            copyOfOutputs.ForEach(dsd =>
            {
                result.Add(new MethodSignaturePart
                {
                    DSD = dsd,
                    ImplementWith = DataFlowImplementationStyle.AsAction
                });
            });

            return result;
        }
    }
}
