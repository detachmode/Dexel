using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Library;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;

namespace Roslyn.Analyser
{
    public static class FunctionUnitAnalyser
    {
        public static void GetAllActionOutputs(FunctionUnit functionUnit,
            Action<MethodSignaturePart> onConnected,
            Action<MethodSignaturePart> onUnconnected
        )
        {
            var sigs = OutputAnalyser.AnalyseOutputs(functionUnit);
            var asAction = sigs.Where(x => x.ImplementWith == DataFlowImplementationStyle.AsAction);
            asAction.ForEach(sig =>
            {
                if (!sig.DSD.Connected)
                    onUnconnected(sig);
                else
                {
                    onConnected(sig);
                }

            });

        }


        public static void GetDsdThatReturns(FunctionUnit functionUnit, Action<DataStreamDefinition> onReturn, Action onNoReturn = null)
        {
            var sigs = OutputAnalyser.AnalyseOutputs(functionUnit);
            var returndsd = sigs.FirstOrDefault(sig => sig.ImplementWith == DataFlowImplementationStyle.AsReturn)?.DSD;
            if (returndsd != null)
            {
                onReturn(returndsd);
            }
            else
            {
                onNoReturn?.Invoke();
            }
        }
    }
}
