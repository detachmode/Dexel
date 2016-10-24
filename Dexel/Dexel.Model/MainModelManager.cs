using System;
using System.Linq;
using Dexel.Model.DataTypes;

namespace Dexel.Model
{

    public class MainModelManager
    {
        private readonly SoftwareCellsManager _softwareCellsManager;
        private readonly DataStreamManager _dataStreamManager;

        public MainModelManager(SoftwareCellsManager softwareCellsManager, DataStreamManager dataStreamManager)
        {
            _softwareCellsManager = softwareCellsManager;
            _dataStreamManager = dataStreamManager;
        }


        public void RemoveConnection(DataStream dataStream, MainModel mainModel)
        {
            mainModel.Connections.RemoveAll(x => x.ID.Equals(dataStream.ID));
        }

        public void AddNewOutput(SoftwareCell softwareCell, string datanames)
        {
            var definition = _dataStreamManager.CreateNewDefinition(softwareCell, datanames);
            softwareCell.OutputStreams.Add(definition);

        }

        public void AddNewInput(Guid softwareCellID, string datanames, MainModel mainModel,
            string actionName = "")
        {
            _softwareCellsManager.GetAll(softwareCellID, mainModel).ToList()
                .ForEach(softwareCell =>
                {
                    var dataStream = _dataStreamManager.CreateNewDefinition(softwareCell, datanames, actionName);
                    softwareCell.InputStreams.Add(dataStream);
                });
        }


        public  void AddNewInput(SoftwareCell softwareCell, string datanames, string actionName = null)
        {
            var definition = _dataStreamManager.CreateNewDefinition(softwareCell, datanames, actionName);
            softwareCell.InputStreams.Add(definition);

        }


        public  void RemoveConnection(Guid id, MainModel mainModel)
        {
            var datastream = _dataStreamManager.GetFirst(id, mainModel);
            RemoveConnection(datastream, mainModel);


        }


        public SoftwareCell AddNewSoftwareCell(string name, MainModel mainModel)
        {
            var newcell = _softwareCellsManager.CreateNew(name);
            mainModel.SoftwareCells.Add(newcell);
            return newcell;
        }


        public Guid AddNewConnectionAndDSDs(SoftwareCell source, SoftwareCell destination,
            string datanames, string actionname, MainModel mainModel)
        {
            var sourceDef = _dataStreamManager.CreateNewDefinition(source, datanames, actionname);
            sourceDef.Connected = true;
            source.OutputStreams.Add(sourceDef);

            var destinationDef = _dataStreamManager.CreateNewDefinition(destination, datanames, actionname);
            destinationDef.Connected = true;
            destination.InputStreams.Add(destinationDef);

            var dataStream = _dataStreamManager.CreateNew(datanames, actionname);
            dataStream.Destinations.Add(destinationDef);
            dataStream.Sources.Add(sourceDef);

            mainModel.Connections.Add(dataStream);
            return dataStream.ID;
        }


        public void Connect(SoftwareCell source, SoftwareCell destination, DataStreamDefinition defintion,
            MainModel mainModel)
        {
            Connect(source, destination, defintion.DataNames, mainModel, actionName: defintion.ActionName);
        }


        public Guid Connect(SoftwareCell source, SoftwareCell destination, string datanames, MainModel mainModel,
            string actionName = "")
        {


            source.OutputStreams.RemoveAll(x => x.DataNames == datanames && x.ActionName == actionName);
            destination.InputStreams.RemoveAll(x => x.DataNames == datanames && x.ActionName == actionName);

            return AddNewConnectionAndDSDs(source, destination, datanames, actionName, mainModel);

        }


        public void AddNewConnectionAndDSDs(DataStreamDefinition defintion, SoftwareCell source,
            SoftwareCell destination, MainModel mainModel)
        {
            _softwareCellsManager.RemoveDefinitionsFromSourceAndDestination(defintion, source, destination);
            var dataStream = _dataStreamManager.CreateNew(defintion.DataNames, defintion.ActionName);

            var outputDefinition = _dataStreamManager.CreateNewDefinition(source, defintion);
            outputDefinition.Connected = true;
            source.OutputStreams.Add(outputDefinition);
            dataStream.Sources.Add(outputDefinition);

            var inputDefinition = _dataStreamManager.CreateNewDefinition(destination, "");
            inputDefinition.Connected = true;
            destination.InputStreams.Add(inputDefinition);
            dataStream.Destinations.Add(inputDefinition);



            mainModel.Connections.Add(dataStream);
        }
    }

}