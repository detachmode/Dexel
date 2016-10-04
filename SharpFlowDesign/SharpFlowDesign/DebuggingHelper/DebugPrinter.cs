using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFlowDesign.Model;

namespace SharpFlowDesign.DebuggingHelper
{

    public static class DebugPrinter
    {
        public static void PrintRecursive(SoftwareCell cell)
        {
            PrintOutputs(cell);
            var destinations = cell.OutputConnections.SelectMany(stream => stream.Destinations).ToList();
            destinations.ForEach(PrintRecursive);
        }


        public static void PrintOutputs(SoftwareCell cell)
        {
            Console.WriteLine(@"// Outputs of " + cell.Name + ":");
            cell.OutputConnections.ToList().ForEach(stream =>
            {
                PrintStreamHeader(cell, stream);
                stream.PrintDestinations();
            });
        }


        private static void PrintStreamHeader(SoftwareCell cell, SoftwareCell.Stream stream)
        {
            if (stream.ActionName != null)
                Console.WriteLine(cell.Name + @" - " + stream.ActionName + @"( " + stream.DataNames + @" ) ->");
            else
                Console.WriteLine(cell.Name + @" - ( " + stream.DataNames + @" ) -> ");
        }


        public static void PrintIntegration(SoftwareCell cell)
        {
            if (cell.Integration == null) return;
            PrintIntegrationHeader(cell);
            PrintOutputs(cell.Integration);
        }


        private static void PrintIntegrationHeader(SoftwareCell cell)
        {
            Console.WriteLine(@"// " + cell.Name + @" is integrating: " + cell.Integration.Name);
        }
    }
}
