using System;
using System.Linq;

namespace FlowDesignModel
{

    public static class MainModelManager
    {
        public static void RemoveConnection(DataStream dataStream, MainModel mainModel)
        {
            mainModel.Connections.RemoveAll(x => x.ID.Equals(dataStream.ID));
        }

        public static void AddNewOutput(SoftwareCell softwareCell, string datanames)
        {
            var definition = DataStreamManager.CreateNewDefinition(datanames);
            softwareCell.OutputStreams.Add(definition);

        }

        public static void AddNewInput(Guid softwareCellID, string datanames, MainModel mainModel,
            string actionName = "")
        {
            SoftwareCellsManager.GetAll(softwareCellID, mainModel).ToList()
                .ForEach(softwareCell =>
                {
                    var dataStream = DataStreamManager.CreateNewDefinition(datanames, actionName);
                    softwareCell.InputStreams.Add(dataStream);
                });
        }


        public static void AddNewInput(SoftwareCell softwareCell, string datanames)
        {
            var definition = DataStreamManager.CreateNewDefinition(datanames);
            softwareCell.InputStreams.Add(definition);

        }


        public static void RemoveConnection(Guid id, MainModel mainModel)
        {
            var datastream =  DataStreamManager.GetFirst(id, mainModel);
            MainModelManager.RemoveConnection(datastream, mainModel);


        }


        public static Guid AddNewSoftwareCell(string name, MainModel mainModel)
        {
            var newcell = SoftwareCellsManager.CreateNew(name);
            mainModel.SoftwareCells.Add(newcell);
            return newcell.ID;
        }


        public static Guid AddNewConnection(SoftwareCell source, SoftwareCell destination, 
            string datanames, string actionname, MainModel mainModel)
        {
            var def = DataStreamManager.CreateNewDefinition(datanames, actionname);
            def.Connected = true;
            source.OutputStreams.Add(def);

            def = DataStreamManager.CreateNewDefinition(datanames, actionname);
            def.Connected = true;
            destination.InputStreams.Add(def);

            var dataStream = DataStreamManager.CreateNew(datanames, actionname);
            dataStream.Destinations.Add(destination);
            dataStream.Sources.Add(source);

            mainModel.Connections.Add(dataStream);
            return dataStream.ID;
        }


        public static void Connect(Guid sourceID, Guid destinationID, DataStreamDefinition defintion, MainModel mainModel)
        {
            Connect(sourceID, destinationID, defintion.DataNames, mainModel, actionName: defintion.ActionName);
        }


        public static Guid Connect(Guid sourceID, Guid destinationID, string datanames, MainModel mainModel,
            string actionName = "")
        {
            var source = SoftwareCellsManager.GetFirst(sourceID, mainModel);
            var destination = SoftwareCellsManager.GetFirst(destinationID, mainModel);

            source.OutputStreams.RemoveAll(x => x.DataNames == datanames && x.ActionName == actionName);
            destination.InputStreams.RemoveAll(x => x.DataNames == datanames && x.ActionName == actionName);

            return AddNewConnection(source, destination, datanames, actionName, mainModel);

        }


        public static void AddNewConnection(DataStreamDefinition defintion, SoftwareCell source, SoftwareCell destination,
            MainModel mainModel)
        {
            var dataStream = DataStreamManager.CreateNew(defintion.DataNames, defintion.ActionName);
            dataStream.Destinations.Add(destination);
            dataStream.Sources.Add(source);
            mainModel.Connections.Add(dataStream);
        }
    }

}