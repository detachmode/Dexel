using System.Collections.Generic;

namespace SharpFlowDesign.Model
{
    public class MainModel
    {
        private static MainModel self;

        public static MainModel Get()
        {
            return self ?? (self = new MainModel());
        }

        public List<Model.DataStream> Connections = new List<DataStream>();
        public List<Model.SoftwareCell> SoftwareCells = new List<SoftwareCell>();
    }
}