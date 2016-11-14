using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Library;
using Dexel.Model.DataTypes;

namespace Dexel.Model
{

    public static class MainModelManager
    {
        public static void RemoveConnection(DataStream dataStream, MainModel mainModel)
        {

            dataStream.Sources.ForEach(dsds => dsds.Connected = false);
            dataStream.Destinations.ForEach(dsds => dsds.Connected = false);

           
            RemoveFromIntegrationIncludingChildren(dataStream, mainModel);
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
            ConnectTwoCells(source, destination, defintion.DataNames, "", mainModel, defintion.ActionName);
        }


        public static DataStream ConnectTwoCells(SoftwareCell source, SoftwareCell destination, string outputs,
            string inputs,
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

            AddToIntegrationIncludingChildren(sourceDSD, destinationDSD, mainModel);

            return datastream;
        }


        private static void AddToIntegrationIncludingChildren(DataStreamDefinition sourceDSD,
            DataStreamDefinition destinationDSD,
            MainModel mainModel)
        {
            FindIntegration(sourceDSD.Parent, foundIntegration =>
            {
                foundIntegration.Integration.AddUnique(destinationDSD.Parent);
                TraverseChildren(destinationDSD.Parent, child => foundIntegration.Integration.AddUnique(child),
                    mainModel);
            }, mainModel);

            FindIntegration(destinationDSD.Parent, foundIntegration =>
            {
                foundIntegration.Integration.AddUnique(sourceDSD.Parent);
                TraverseChildrenBackwards(sourceDSD.Parent, child => foundIntegration.Integration.AddUnique(child),
                    mainModel);
            }, mainModel);
        }


        public static void SetParents(MainModel loadedMainModel)
        {
            loadedMainModel.SoftwareCells.ForEach(sc =>
            {
                sc.InputStreams.ForEach(dsd => dsd.Parent = sc);
                sc.OutputStreams.ForEach(dsd => dsd.Parent = sc);
            });
        }


        public static void SolveConnectionReferences(MainModel loadedMainModel)
        {
            loadedMainModel.Connections.ForEach(c =>
            {
                var found =
                    c.Sources.Select(dsd => DataStreamManager.GetDSDFromModel(dsd.ID, loadedMainModel.SoftwareCells))
                        .ToList();
                if (found.Any())
                {
                    c.Sources = found;
                }

                found =
                    c.Destinations.Select(
                        dsd => DataStreamManager.GetDSDFromModel(dsd.ID, loadedMainModel.SoftwareCells)).ToList();
                if (found.Any())
                {
                    c.Destinations = found;
                }
            });
        }


        public static void SolveIntegrationReferences(MainModel loadedMainModel)
        {
            var lookupID = loadedMainModel.SoftwareCells.ToLookup(sc => sc.ID);
            loadedMainModel.SoftwareCells.Where(sc => sc.Integration.Count != 0).ToList().ForEach(sc =>
            {
                var references = sc.Integration.SelectMany(iSc => lookupID[iSc.ID]).ToList();
                sc.Integration = references;
            });
        }


        public static void RemoveFromIntegrationIncludingChildren(DataStream dataStream, MainModel mainModel)
        {
            
            FindIntegration(dataStream.Destinations.First().Parent, foundIntegration =>
            {
                //foundIntegration.Integration.RemoveAll(iSc => dataStream.Sources.Any(dsd => dsd.Parent.ID == iSc.ID));
                foundIntegration.Integration.RemoveAll(iSc => dataStream.Destinations.Any(dsd => dsd.Parent.ID == iSc.ID));

                dataStream.Destinations.ForEach(
                    dsd =>
                    {
                        foundIntegration.Integration.Remove(dsd.Parent);
                        TraverseChildren(dsd.Parent,
                            child => foundIntegration.Integration.RemoveAll(iSc => iSc.ID == child.ID),
                            mainModel);
                    });
            }, mainModel);
        }


        public static void TraverseChildren(SoftwareCell parent, Action<SoftwareCell> func, MainModel mainModel)
        {
            var foundConnections = mainModel.Connections.Where(c => c.Sources.Any(dsd => dsd.Parent == parent));
            foreach (var connection in foundConnections)
            {
                connection.Destinations.ForEach(dsd =>
                {
                    func(dsd.Parent);
                    TraverseChildren(dsd.Parent, func, mainModel);
                });
            }
        }


        public static void TraverseChildrenBackwards(SoftwareCell children, Action<SoftwareCell> func,
            MainModel mainModel)
        {
            var foundConnections = mainModel.Connections.Where(c => c.Destinations.Any(dsd => dsd.Parent == children));
            foreach (var connection in foundConnections)
            {
                connection.Sources.ForEach(dsd =>
                {
                    func(dsd.Parent);
                    TraverseChildren(dsd.Parent, func, mainModel);
                });
            }
        }


        private static void FindIntegration(SoftwareCell softwareCell, Action<SoftwareCell> onFound, MainModel mainModel)
        {
            var softwareCells = mainModel.SoftwareCells.Where(sc => sc.Integration.Any(iSc => iSc.ID == softwareCell.ID));
            var integration = softwareCells as IList<SoftwareCell> ?? softwareCells.ToList();
            if (integration.Any())
            {
                onFound(integration.First());
            }
        }
    }

}