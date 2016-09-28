using System;
using System.Collections.Generic;

namespace SharpFlowDesign.Model
{
    public partial class SoftwareCell
    {
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
                Destinations.ForEach(x => Console.WriteLine($"\t ->{x.Name}"));
            }
        }
    }
}
