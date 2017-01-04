using System;
using System.IO;

namespace Roslyn
{
    public class Outputs
    {
        public static void WriteToDesktopFile(string generatedCode)
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                File.WriteAllText(Path.Combine(desktop, @"generatedFlowDesign.cs"), generatedCode);
            }
            catch
            {
                Console.WriteLine("Couldn't generate or write file");
            }
        }


        public static void ClearPrintToConsole(string generatedCode)
        {
            Console.Clear();
            Console.Write(generatedCode);
        }
    }
}