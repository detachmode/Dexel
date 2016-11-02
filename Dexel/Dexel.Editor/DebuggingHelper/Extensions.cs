using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dexel.Editor.DebuggingHelper
{
    public static class MyDebug
    {
        private static string lastline = "";
        public static void WriteLineIfDifferent(string line)
        {
            if (lastline != line)
            {
                Debug.WriteLine(line);
                lastline = line;
            }
            
        }
    }

}
