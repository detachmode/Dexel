using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpFlowDesign.Model
{
    public static class FlowDesignManager
    {
        private static readonly List<SoftwareCell> SoftwareCells = new List<SoftwareCell>();
        private static readonly List<SoftwareCell.Stream> Streams = new List<SoftwareCell.Stream>();


        public static SoftwareCell Root { get; set; }


        public static SoftwareCell NewSoftwareCell(string name)
        {
            var softwareCell = new SoftwareCell { Name = name};
            SoftwareCells.Add(softwareCell);
            return softwareCell;
        }

        public static void Connect(SoftwareCell source, SoftwareCell destination, string datanames, string actionName = null, bool optional = false)
        {
            var tempStream = new SoftwareCell.Stream
            {
                ActionName = actionName, DataNames = datanames, Optional = optional
            };

            source.CheckForStreamWithSameName(destination, tempStream, 
                onFound: source.AddToExistingConnection, 
                onNotFound: source.AddNewConnection);

            Streams.Add(tempStream);
        }

    }
}
