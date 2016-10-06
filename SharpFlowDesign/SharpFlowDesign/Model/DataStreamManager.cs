using System;
using System.Linq;

namespace SharpFlowDesign.Model
{

    public static class DataStreamManager
    {
        public static DataStream CreateNew(string datanames, string actionsName = "")
        {
            var dataStream = new DataStream();
            dataStream.ID = Guid.NewGuid();
            dataStream.DataNames = datanames;
            dataStream.ActionName = actionsName;
            return dataStream;
        }


        public static DataStream GetFirst(Guid id, MainModel mainModel)
        {
            return mainModel.Connections.First(x => x.ID.Equals(id));
        }
    }

}