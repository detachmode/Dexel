using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model.DataTypes;

namespace Dexel.Editor.Common
{

    public static class DebugPrinter
    {
        public static void PrintRecursive(FunctionUnit fu)
        {
            //PrintOutputs(fu);
            //var destinations = fu.OutputStreams.SelectMany(stream => stream.Destinations).ToList();
            //destinations.ForEach(PrintRecursive);
        }


        public static void PrintOutputs(List<FunctionUnit> fu)
        {
            //Console.WriteLine(@"// Outputs of " + fu.Name + ":");
            //fu.OutputStreams.ToList().ForEach(dataStream =>
            //{
            //    PrintStreamHeader(fu, dataStream);
            //    PrintDestinations(dataStream);
            //});
        }

        public static void PrintDestinations(DataStream dataStream)
        {
            dataStream.Destinations.ForEach(x => Console.WriteLine($"\t ->{x.Parent.Name}"));
        }


        public static void PrintIntegration(FunctionUnit fu)
        {
            if (fu.Integration == null) return;
            PrintIntegrationHeader(fu);
            PrintOutputs(fu.Integration);
        }


        private static void PrintIntegrationHeader(FunctionUnit fu)
        {
            Console.WriteLine(@"// " + fu.Name + @" is integrating: " + fu.Integration.First().Name);
        }

        public static void PrintConnections(MainModel mainModel)
        {
            Console.WriteLine(@"---------------------");
            Console.WriteLine(@"-- Connections ------");
            Console.WriteLine(@"---------------------");
            mainModel.Connections.ForEach( 
                x => 
                Console.WriteLine(@"{0} - {1} -> {2} ", x.Sources.First().Parent.Name, x.DataNames, x.Destinations.First().Parent.Name));
        }

        public static void PrintFunctionUnits(MainModel mainModel)
        {
            Console.WriteLine(@"---------------------");
            Console.WriteLine(@"-- FunctionUnits ----");
            Console.WriteLine(@"---------------------");
            mainModel.FunctionUnits.ForEach(
                x =>
                {
                    Console.WriteLine("\nName: {0}", x.Name);
                    x.InputStreams.ForEach( i => Console.WriteLine("\t Input: {0}", i.DataNames));
                    x.OutputStreams.ForEach(o => Console.WriteLine("\t Output: {0}", o.DataNames));

                });
        }
}
}
