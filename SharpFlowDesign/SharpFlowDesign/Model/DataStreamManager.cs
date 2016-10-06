using System;

namespace SharpFlowDesign.Model
{

    public static class DataStreamManager
    {
        public static DataStream CreateNew(string datanames)
        {
            var dataStream = new DataStream();
            dataStream.ID = Guid.NewGuid();
            dataStream.DataNames = datanames;
            return dataStream;
        }
    }

}