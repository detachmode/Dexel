using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Dexel.Editor.ViewModels.UI_Sketches;

namespace Dexel.Editor.Common
{
    public static class MyDebug
    {
        private static string _lastline = "";
        public static void WriteLineIfDifferent(string line)
        {
            if (_lastline == line) return;
            Debug.WriteLine(line);
            _lastline = line;
        }
    }
}
