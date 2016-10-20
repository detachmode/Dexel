using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Editor.Views;
using Dexel.Contracts.Model;

namespace Dexel.Editor.DebuggingHelper
{

    public static class DebugPrinter
    {
        public static void PrintRecursive(ISoftwareCell cell)
        {
            //PrintOutputs(cell);
            //var destinations = cell.OutputStreams.SelectMany(stream => stream.Destinations).ToList();
            //destinations.ForEach(PrintRecursive);
        }


        public static void PrintOutputs(List<ISoftwareCell> cell)
        {
            //Console.WriteLine(@"// Outputs of " + cell.Name + ":");
            //cell.OutputStreams.ToList().ForEach(dataStream =>
            //{
            //    PrintStreamHeader(cell, dataStream);
            //    PrintDestinations(dataStream);
            //});
        }

        public static void PrintDestinations(IDataStream dataStream)
        {
            dataStream.Destinations.ForEach(x => Console.WriteLine($"\t ->{x.Parent.Name}"));
        }


        private static void PrintStreamHeader(SoftwareCell cell, IDataStream stream)
        {
            if (stream.ActionName != null)
                Console.WriteLine(cell.Name + @" - " + stream.ActionName + @"( " + stream.DataNames + @" ) ->");
            else
                Console.WriteLine(cell.Name + @" - ( " + stream.DataNames + @" ) -> ");
        }


        public static void PrintIntegration(ISoftwareCell cell)
        {
            if (cell.Integration == null) return;
            PrintIntegrationHeader(cell);
            PrintOutputs(cell.Integration);
        }


        private static void PrintIntegrationHeader(ISoftwareCell cell)
        {
            Console.WriteLine(@"// " + cell.Name + @" is integrating: " + cell.Integration.First().Name);
        }

        public static void PrintConnections(IMainModel mainModel)
        {
            Console.WriteLine(@"---------------------");
            Console.WriteLine(@"-- Connections ------");
            Console.WriteLine(@"---------------------");
            mainModel.Connections.ForEach( 
                x => 
                Console.WriteLine(@"{0} - {1} -> {2} ", x.Sources.First().Parent.Name, x.DataNames, x.Destinations.First().Parent.Name));
        }

        public static void PrintSoftwareCells(IMainModel mainModel)
        {
            Console.WriteLine(@"---------------------");
            Console.WriteLine(@"-- SoftwareCells ----");
            Console.WriteLine(@"---------------------");
            mainModel.SoftwareCells.ForEach(
                x =>
                {
                    Console.WriteLine("\nName: {0}", x.Name);
                    x.InputStreams.ForEach( i => Console.WriteLine("\t Input: {0}", i.DataNames));
                    x.OutputStreams.ForEach(o => Console.WriteLine("\t Output: {0}", o.DataNames));

                });
        }
}
}
