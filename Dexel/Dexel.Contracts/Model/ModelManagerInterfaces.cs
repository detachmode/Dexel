using System;
using System.Collections.Generic;

namespace Dexel.Contracts.Model
{

    public interface ISoftwareCellsManager
    {
        IEnumerable<ISoftwareCell> GetAll(Guid softwareCellID, IMainModel mainModel);
        ISoftwareCell GetFirst(Guid destinationID, IMainModel mainModel);
        ISoftwareCell GetFristByID(Guid destinationID, IMainModel mainModel);
        ISoftwareCell CreateNew(string name = "");


        void RemoveDefinitionsFromSourceAndDestination(IDataStreamDefinition defintion, ISoftwareCell source,
            ISoftwareCell destination);
    }


    public interface IDataStreamManager
    {
        IDataStream CreateNew(string datanames, string actionsName = "");


        IDataStreamDefinition CreateNewDefinition(ISoftwareCell parent, string datanames, string actionsName = "",
            bool connected = false);


        IDataStream GetFirst(Guid id, IMainModel mainModel);
        IDataStream CreateNew(IDataStreamDefinition datastreamDefintion);


        IDataStreamDefinition CreateNewDefinition(ISoftwareCell parent, IDataStreamDefinition defintion,
            bool connected = false);


        IDataStreamDefinition FindExistingDefinition(IDataStreamDefinition defintion,
            IEnumerable<IDataStreamDefinition> definitions, Action<IDataStreamDefinition> onFound,
            Action onNotFound = null);


        void DeConnect(IEnumerable<IDataStreamDefinition> definitions, IDataStream dataStream);


        Guid CheckForStreamWithSameName(ISoftwareCell source, ISoftwareCell destination,
            IDataStream tempStream, IMainModel mainModel,
            Action<IDataStreamDefinition> onFound, Action onNotFound);
    }


    public interface IMainModelManager
    {
        void RemoveConnection(IDataStream dataStream, IMainModel mainModel);
        void AddNewOutput(ISoftwareCell softwareCell, string datanames);


        void AddNewInput(Guid softwareCellID, string datanames, IMainModel mainModel,
            string actionName = "");


        void AddNewInput(ISoftwareCell softwareCell, string datanames, string actionName = null);
        void RemoveConnection(Guid id, IMainModel mainModel);
        ISoftwareCell AddNewSoftwareCell(string name, IMainModel mainModel);


        Guid AddNewConnectionAndDSDs(ISoftwareCell source, ISoftwareCell destination,
            string datanames, string actionname, IMainModel mainModel);


        void Connect(ISoftwareCell source, ISoftwareCell destination, IDataStreamDefinition defintion,
            IMainModel mainModel);


        Guid Connect(ISoftwareCell source, ISoftwareCell destination, string datanames, IMainModel mainModel,
            string actionName = "");


        void AddNewConnectionAndDSDs(IDataStreamDefinition defintion, ISoftwareCell source, ISoftwareCell destination,
            IMainModel mainModel);
    }

}