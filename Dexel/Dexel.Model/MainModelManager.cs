using System;
using Dexel.Model.DataTypes;

namespace Dexel.Model
{

    public static class MainModelManager
    {
        public static void RemoveConnection(DataStream dataStream, MainModel mainModel)
        {
            mainModel.Connections.RemoveAll(x => x.ID.Equals(dataStream.ID));
        }


        public static void AddNewOutput(SoftwareCell softwareCell, string datanames, string actionName = null)
        {
            SoftwareCellsManager.NewOutputDef(softwareCell, datanames, actionName);
        }


        public static void AddNewInput(SoftwareCell softwareCell, string datanames, string actionName = null)
        {
            SoftwareCellsManager.NewInputDef(softwareCell, datanames, actionName);
        }


        public static void RemoveConnection(Guid id, MainModel mainModel)
        {
            var datastream = DataStreamManager.GetFirst(id, mainModel);
            RemoveConnection(datastream, mainModel);
        }


        public static SoftwareCell AddNewSoftwareCell(string name, MainModel mainModel)
        {
            var newcell = SoftwareCellsManager.CreateNew(name);
            mainModel.SoftwareCells.Add(newcell);
            return newcell;
        }


        public static void ConnectTwoCells(SoftwareCell source, SoftwareCell destination, DataStreamDefinition defintion,
            MainModel mainModel)
        {
            ConnectTwoCells(source, destination, defintion.DataNames, "" ,  mainModel, defintion.ActionName);
        }


        public static DataStream ConnectTwoCells(SoftwareCell source, SoftwareCell destination, string outputs, string inputs,
            MainModel mainModel, string actionName = "")
        {

            source.OutputStreams.RemoveAll(x => x.DataNames == outputs && x.ActionName == actionName);
            destination.InputStreams.RemoveAll(x => x.DataNames == inputs && x.ActionName == actionName);

            var sourceDef = SoftwareCellsManager.NewOutputDef(source, outputs, actionName);
            var destinationDef = SoftwareCellsManager.NewInputDef(destination, inputs, null);

            return ConnectTwoDefintions(sourceDef, destinationDef, mainModel);
        }


        public static DataStream ConnectTwoDefintions(DataStreamDefinition sourceDSD,
            DataStreamDefinition destinationDSD, MainModel mainModel)
        {
            // new Connection
            var datastream = DataStreamManager.NewDataStream(DataStreamManager.MergeDataNames(sourceDSD, destinationDSD));
            datastream.Sources.Add(sourceDSD);
            datastream.Destinations.Add(destinationDSD);
            mainModel.Connections.Add(datastream);

            // update definition state
            destinationDSD.Connected = true;
            sourceDSD.Connected = true;

            return datastream;
        }

    }

}