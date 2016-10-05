using System;
using SharpFlowDesign.Model;

namespace SharpFlowDesign
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