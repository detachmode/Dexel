using System.Collections.Generic;

namespace FlowDesignModel
{
    public class MainModel
    {
        private static MainModel self;

        public static MainModel Get()
        {
            return self ?? (self = new MainModel());
        }

        public List<DataStream> Connections = new List<DataStream>();
        public List<SoftwareCell> SoftwareCells = new List<SoftwareCell>();
    }
}