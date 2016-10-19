using System;
using System.Linq;
using Dexel.Contracts.Model;

namespace Dexel.Model
{

    public class MainModelManager : IMainModelManager
    {
        private readonly ISoftwareCellsManager _softwareCellsManager;
        private readonly IDataStreamManager _dataStreamManager;

        public MainModelManager(ISoftwareCellsManager softwareCellsManager, IDataStreamManager dataStreamManager)
        {
            _softwareCellsManager = softwareCellsManager;
            _dataStreamManager = dataStreamManager;
        }


        public void RemoveConnection(IDataStream dataStream, IMainModel mainModel)
        {
            mainModel.Connections.RemoveAll(x => x.ID.Equals(dataStream.ID));
        }

        public void AddNewOutput(ISoftwareCell softwareCell, string datanames)
        {
            var definition = _dataStreamManager.CreateNewDefinition(softwareCell, datanames);
            softwareCell.OutputStreams.Add(definition);

        }

        public void AddNewInput(Guid softwareCellID, string datanames, IMainModel mainModel,
            string actionName = "")
        {
            _softwareCellsManager.GetAll(softwareCellID, mainModel).ToList()
                .ForEach(softwareCell =>
                {
                    var dataStream = _dataStreamManager.CreateNewDefinition(softwareCell, datanames, actionName);
                    softwareCell.InputStreams.Add(dataStream);
                });
        }


        public  void AddNewInput(ISoftwareCell softwareCell, string datanames, string actionName = null)
        {
            var definition = _dataStreamManager.CreateNewDefinition(softwareCell, datanames, actionName);
            softwareCell.InputStreams.Add(definition);

        }


        public  void RemoveConnection(Guid id, IMainModel mainModel)
        {
            var datastream = _dataStreamManager.GetFirst(id, mainModel);
            RemoveConnection(datastream, mainModel);


        }


        public ISoftwareCell AddNewSoftwareCell(string name, IMainModel mainModel)
        {
            var newcell = _softwareCellsManager.CreateNew(name);
            mainModel.SoftwareCells.Add(newcell);
            return newcell;
        }


        public Guid AddNewConnectionAndDSDs(ISoftwareCell source, ISoftwareCell destination,
            string datanames, string actionname, IMainModel mainModel)
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


        public void Connect(ISoftwareCell source, ISoftwareCell destination, IDataStreamDefinition defintion,
            IMainModel mainModel)
        {
            Connect(source, destination, defintion.DataNames, mainModel, actionName: defintion.ActionName);
        }


        public Guid Connect(ISoftwareCell source, ISoftwareCell destination, string datanames, IMainModel mainModel,
            string actionName = "")
        {


            source.OutputStreams.RemoveAll(x => x.DataNames == datanames && x.ActionName == actionName);
            destination.InputStreams.RemoveAll(x => x.DataNames == datanames && x.ActionName == actionName);

            return AddNewConnectionAndDSDs(source, destination, datanames, actionName, mainModel);

        }


        public void AddNewConnectionAndDSDs(IDataStreamDefinition defintion, ISoftwareCell source,
            ISoftwareCell destination, IMainModel mainModel)
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