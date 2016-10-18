using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Contracts.Model;

namespace Dexel.Model
{

  


    public class DataStreamManager : IDataStreamManager
    {
        public  IDataStream CreateNew(string datanames, string actionsName = "")
        {
            var dataStream = new DataStream();
            dataStream.ID = Guid.NewGuid();
            dataStream.DataNames = datanames;
            dataStream.ActionName = actionsName;
            return dataStream;
        }

        public  IDataStreamDefinition CreateNewDefinition(ISoftwareCell parent, string datanames, string actionsName = "", bool connected = false)
        {
            var dataStream = new DataStreamDefinition();
            dataStream.ID = Guid.NewGuid(); 
            dataStream.DataNames = datanames;
            dataStream.Parent = parent;
            dataStream.ActionName = actionsName;
            dataStream.Connected = connected;
            return dataStream;
        }


        public  IDataStream GetFirst(Guid id, IMainModel mainModel)
        {
            return mainModel.Connections.First(x => x.ID.Equals(id));
        }

        public  IDataStream CreateNew(IDataStreamDefinition datastreamDefintion)
        {
           return CreateNew(datastreamDefintion.DataNames, datastreamDefintion.ActionName);
        }

        public  IDataStreamDefinition CreateNewDefinition(ISoftwareCell parent, IDataStreamDefinition defintion, bool connected = false)
        {
            return CreateNewDefinition(parent, defintion.DataNames, defintion.ActionName, connected: connected);
        }


        public  IDataStreamDefinition FindExistingDefinition(IDataStreamDefinition defintion, IEnumerable<IDataStreamDefinition> definitions, Action<IDataStreamDefinition> onFound, Action onNotFound = null   )
        {
            var found = definitions.Where(x => x.IsEquals(defintion));
            if (found.Any())
            {
                onFound(found.First());
                return found.First();
            }
            else
            {
                onNotFound?.Invoke();
            }
            return null;
        }

        

        public  void DeConnect(IEnumerable<IDataStreamDefinition> definitions, IDataStream dataStream)
        {
            definitions.Where( def => def.IsEquals(dataStream)).ToList()
                .ForEach( def => def.Connected = false);
        }


        public  Guid CheckForStreamWithSameName(ISoftwareCell source, ISoftwareCell destination,
            IDataStream tempStream, IMainModel mainModel,
            Action<IDataStreamDefinition> onFound, Action onNotFound)
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