using System.Diagnostics;

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
