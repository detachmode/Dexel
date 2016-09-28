using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpFlowDesign.Model
{

    public class SoftwareCell
    {
        public SoftwareCell()
        {
            InputStream = new List<Stream>();
            OutputStream = new List<Stream>();
        }

        public string Name { get; set; }
        public FlowAttribute Attribute { get; set; } // Pyramide or Barrel


        public void Connect(SoftwareCell destination, string datanames, string actionName = null, bool optional = false)
        {
            var tempStream = new Stream {ActionName = actionName, DataNames = datanames, Optional = optional};
            CheckForStreamWithSameName(destination, tempStream,
                AddToExistingConnection,
                AddNewConnection);
        }


        public static void PrintRecursive(SoftwareCell cell)
        {
            cell.PrintOutputs();
            var destinations = cell.OutputStream.SelectMany(stream => stream.Destinations).ToList();
            destinations.ForEach(PrintRecursive);
        }


        public void PrintOutputs()
        {
            Console.WriteLine(@"// Outputs of " + this.Name + ":");
            OutputStream.ForEach(stream =>
            {
                PrintStreamHeader(stream);
                stream.PrintDestinations();
            });
        }

        private void PrintStreamHeader(Stream stream)
        {
            if (stream.ActionName != null)
                Console.WriteLine(this.Name + @" - " +stream.ActionName +@"( " + stream.DataNames +@" ) ->");
            else
                Console.WriteLine(this.Name + @" - ( " + stream.DataNames +@" ) -> ");
        }

        public void PrintIntegration()
        {
            if (Integration == null) return;
            PrintIntegrationHeader();
            Integration.PrintOutputs();
        }


        private void PrintIntegrationHeader()
        {
            Console.WriteLine(@"// " + this.Name + @" is integrating: " + Integration.Name);
        }


        public void AddOutput(string datanames, string actionName = null, bool optional = false)
        {
            var stream = new Stream {ActionName =  actionName, DataNames = datanames, Optional = optional };
            AddOutput(stream);
        }


        public void AddInput(string datanames, string actionName = null, bool optional = false)
        {
            var stream = new Stream { ActionName = actionName, DataNames = datanames, Optional = optional };
            AddInput(stream);
        }

        private SoftwareCell Integration { get; set; } // ZOOM into this Node. Start node of the integrated flow


        public void SetIntegration(SoftwareCell integration)
        {
            var foundMatchingInput = InputStream.Any(x =>
                integration.InputStream.Any(y => x.DataNames.Equals(y.DataNames)));
            if (!foundMatchingInput)
                throw new Exception("Integration inputs are not matching with inputs of integrated Flow!");

            Integration = integration;
        }


        private List<Stream> InputStream { get; }
        private List<Stream> OutputStream { get; }

        private void CheckForStreamWithSameName(SoftwareCell destination, Stream tempStream,
            Action<Stream, SoftwareCell> onFound, Action<SoftwareCell, Stream> onNotFound)
        {
            var found = OutputStream.Where(x => x.DataNames.Equals(tempStream.DataNames) && x.Optional.Equals(tempStream.Optional)).ToList();
            if (found.Any())
                onFound(found.First(), destination);
            else
                onNotFound(destination, tempStream);
        }


        private void AddToExistingConnection(Stream stream, SoftwareCell destination)
        {
            stream.AddSource(this);
            stream.AddDestination(destination);
        }


        private void AddNewConnection(SoftwareCell destination, Stream stream)
        {
            AddOutput(stream);
            destination.AddInput(stream);
        }


        private void AddOutput(Stream stream)
        {
            stream.AddSource(this);
            OutputStream.Add(stream);
            return;
        }





        private void AddInput(Stream stream)
        {
            stream.AddDestination(this);
            InputStream.Add(stream);
            return;
        }




        private void RemoveStream(Stream stream)
        {
            InputStream.RemoveAll(x => x == stream);
            OutputStream.RemoveAll(x => x == stream);
        }


        public class Stream
        {
            public Stream()
            {
                Sources = new List<SoftwareCell>();
                Destinations = new List<SoftwareCell>();
                DataNames = "";
            }

            public string ActionName { get; set; }
            public string DataNames { get; set; }
            public bool Optional { get; set; }
            public List<SoftwareCell> Sources { get; }
            public List<SoftwareCell> Destinations { get; }


            public void Delete()
            {
                Sources.ForEach(x => x.RemoveStream(this));
                Sources.Clear();

                Destinations.ForEach(x => x.RemoveStream(this));
                Destinations.Clear();
            }


            public void AddSource(SoftwareCell flow)
            {
                Sources.Add(flow);
            }


            public void AddDestination(SoftwareCell flow)
            {
                Destinations.Add(flow);
            }


            public void PrintDestinations()
            {

                Destinations.ForEach(x => Console.WriteLine("\t -> " + x.Name));
            }
        }



    }


    public enum FlowAttributeType
    {
        Provider,
        State
    }


    public class FlowAttribute
    {
        public string Name { get; set; }
        public FlowAttributeType Type { get; set; }
    }

}