using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowDesignModel
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

        public static DataStreamDefinition CreateNewDefinition(string datanames, string actionsName = "", bool connected = false)
        {
            var dataStream = new DataStreamDefinition();
            dataStream.ID = Guid.NewGuid();
            dataStream.DataNames = datanames;
            dataStream.ActionName = actionsName;
            dataStream.Connected = connected;
            return dataStream;
        }


        public static DataStream GetFirst(Guid id, MainModel mainModel)
        {
            return mainModel.Connections.First(x => x.ID.Equals(id));
        }

        internal static DataStream CreateNew(DataStreamDefinition datastreamDefintion)
        {
           return CreateNew(datastreamDefintion.DataNames, datastreamDefintion.ActionName);
        }

        public static DataStreamDefinition CreateNewDefinition(DataStreamDefinition defintion, bool connected = false)
        {
            return CreateNewDefinition(defintion.DataNames, defintion.ActionName, connected: connected);
        }


        public static bool IsDefinitionIn(this DataStreamDefinition defintion, IEnumerable<DataStreamDefinition> enumerable)
        {
            return enumerable.Any( x => x.IsEquals(defintion));
        }


        public static bool IsEquals(this DataStreamDefinition def1, DataStreamDefinition def2)
        {
            return def1.DataNames == def2.DataNames && def1.ActionName == def2.ActionName;
        }


        public static void FindExistingDefinition(DataStreamDefinition defintion, IEnumerable<DataStreamDefinition> definitions, Action<DataStreamDefinition> onFound, Action onNotFound = null   )
        {
            var found = definitions.Where(x => x.IsEquals(defintion));
            if (found.Any())
                onFound(found.First());
            else
            {
                onNotFound?.Invoke();
            }
        }

        public static bool IsEquals(this DataStreamDefinition def1, DataStream dataStream)
        {
            return def1.DataNames == dataStream.DataNames && def1.ActionName == dataStream.ActionName;
        }


        public static void DeConnect(IEnumerable<DataStreamDefinition> definitions, DataStream dataStream)
        {
            definitions.Where( def => def.IsEquals(dataStream)).ToList()
                .ForEach( def => def.Connected = false);
        }


        public static Guid CheckForStreamWithSameName(SoftwareCell source, SoftwareCell destination,
            DataStream tempStream, MainModel mainModel,
            Action<DataStreamDefinition> onFound, Action onNotFound)
        {
            var found = source.OutputStreams.Where(x => x.DataNames.Equals(tempStream.DataNames)).ToList();
            if (found.Any())
            {
                onFound(found.First());
                return found.First().ID;
            }

            onNotFound();
            return tempStream.ID;
        }
    }

}