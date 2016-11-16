using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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


        public static void AddToIntegrationIncludingChildren(DataStreamDefinition sourceDSD,
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


        public static void RemoveFromIntegrationsIncludingChildren(DataStream dataStream, MainModel mainModel)
        {
            FindIntegration(dataStream.Destinations.First().Parent, foundIntegration =>
            {
                foundIntegration.Integration.RemoveAll(
                    iSc => dataStream.Destinations.Any(dsd => dsd.Parent.ID == iSc.ID));

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


        public static void RemoveAllConnectedFromIntegration(SoftwareCell softwareCell, MainModel mainModel)
        {
            FindIntegration(softwareCell, foundIntegration =>
            {
                foundIntegration.Integration.Remove(softwareCell);
                TraverseChildren(softwareCell, child => foundIntegration.Integration.Remove(child), mainModel);
                TraverseChildrenBackwards(softwareCell, child => foundIntegration.Integration.Remove(child), mainModel);
            }, mainModel);
        }


        public static void RemoveFromIntegrationsIncludingChildren(SoftwareCell softwareCell, MainModel mainModel)
        {
            FindIntegration(softwareCell, foundIntegration =>
            {
                foundIntegration.Integration.Remove(softwareCell);
                TraverseChildren(softwareCell, child => foundIntegration.Integration.Remove(child), mainModel);
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
            mainModel.SoftwareCells.Where(sc => sc.Integration.Contains(softwareCell))
                .ForEach(onFound);
        }


        public static void DeleteCell(SoftwareCell softwareCell, MainModel mainModel)
        {
            // solve Integration logic
            AtleastOneInputConnected(softwareCell,
                () => RemoveFromIntegrationsIncludingChildren(softwareCell, mainModel),
                inputsNotConnected: () =>
                    RemoveFromIntegrations(softwareCell, mainModel));

            RemoveAllConnectionsToCell(softwareCell, mainModel);

            mainModel.SoftwareCells.Remove(softwareCell);
        }


        private static void RemoveFromIntegrations(SoftwareCell softwareCell, MainModel mainModel)
        {
            FindIntegration(softwareCell, integratedByCell => integratedByCell.Integration.Remove(softwareCell),
                mainModel);
        }


        private static void RemoveAllConnectionsToCell(SoftwareCell softwareCell, MainModel mainModel)
        {
            var sources =
                mainModel.Connections.Where(c => c.Sources.Any(sc => softwareCell == sc.Parent));
            var destinations =
                mainModel.Connections.Where(c => c.Destinations.Any(sc => softwareCell == sc.Parent));
            var todelete = destinations.Concat(sources).ToList();
            todelete.ForEach(c => RemoveConnection(c, mainModel));
        }


        private static void AtleastOneInputConnected(SoftwareCell softwareCell, Action doAction,
            Action inputsNotConnected = null)
        {
            if (softwareCell.InputStreams.Any(dsd => dsd.Connected))
            {
                doAction();
            }
            else
            {
                inputsNotConnected?.Invoke();
            }
        }

        public class CopiedCells
        {
            public Guid originGuid;
            public SoftwareCell newCell;
        }

        private static void SetIntegrationOfCopiedCells(List<CopiedCells> copiedList, MainModel mainModel)
        {
            var allnew = copiedList.Select(x => x.newCell).ToList();
            copiedList.ForEach(cc =>
            {
                var orginal = mainModel.SoftwareCells.First(sc => sc.ID == cc.originGuid);
                orginal.Integration.ForEach(isc =>
                {
                    var incopied = copiedList.FirstOrDefault(x => x.originGuid == isc.ID);
                    if (incopied != null)
                    {
                        cc.newCell.Integration.Add(incopied.newCell);
                    }

                });
            });
        }


        private static void ReConnetCopiedCells(List<CopiedCells> copiedList, List<SoftwareCell> original, MainModel mainModel)
        {

            var connectionsOfSelectedCells = mainModel.Connections.Where(c =>
               c.Sources.Any(y => original.Any(x => x == y.Parent))
               &&
               c.Destinations.Any(y => original.Any(x => x == y.Parent))
               ).ToList();


            connectionsOfSelectedCells.ForEach(c =>
            {
                var datastream = c;
                SoftwareCell destination = datastream.Destinations.Select(y => y.Parent).First();
                SoftwareCell source = datastream.Sources.Select(y => y.Parent).First();

                var sourcedsd = copiedList.First(y => y.originGuid == source.ID).newCell.OutputStreams.First(
                    dsd => dsd.DataNames == datastream.Sources.First().DataNames);
                var destinationdsd = copiedList.First(y => y.originGuid == destination.ID).newCell.InputStreams.First(
                   dsd => dsd.DataNames == datastream.Destinations.First().DataNames);

                var newConnection = ConnectTwoDefintions(sourcedsd, destinationdsd, mainModel);
            });
        }

        private static SoftwareCell Duplicate(SoftwareCell softwareCell, MainModel mainModel)
        {
            var newmodel = SoftwareCellsManager.CreateNew(softwareCell.Name);
            newmodel.Position = softwareCell.Position;
            softwareCell.InputStreams.ForEach(dsd =>
            {
                var newdsd = DataStreamManager.NewDefinition(newmodel, dsd);
                newmodel.InputStreams.Add(newdsd);
            });
            softwareCell.OutputStreams.ForEach(dsd =>
            {
                var newdsd = DataStreamManager.NewDefinition(newmodel, dsd);
                newmodel.OutputStreams.Add(newdsd);
            });

            return newmodel;
        }


        private static List<CopiedCells> DuplicateMany(List<SoftwareCell> softwareCells, MainModel mainModel)
        {
            var copiedList = new List<CopiedCells>();
            softwareCells.ForEach(sc =>
            {
                var newCell = Duplicate(sc, mainModel);
                var copiedCell = new CopiedCells { originGuid = sc.ID, newCell = newCell };
                copiedList.Add(copiedCell);
                mainModel.SoftwareCells.Add(newCell);
            });
            return copiedList;
        }


        public static void MakeIntegrationIncludingChildren(SoftwareCell parentCell, SoftwareCell subCell, MainModel mainModel)
        {
            parentCell.Integration.AddUnique(subCell);
            TraverseChildren(subCell, child => parentCell.Integration.AddUnique(child), mainModel);
        }


        public static void MoveCellsToClickedPosition(Point positionClicked, List<CopiedCells> copiedList)
        {
            var delta = positionClicked - copiedList.First().newCell.Position;
            copiedList.ForEach(x => x.newCell.Position += delta);
        }


        public static List<MainModelManager.CopiedCells> Duplicate( List<SoftwareCell> softwareCells, MainModel mainModel)
        {
            var copiedList = MainModelManager.DuplicateMany(softwareCells, mainModel);
            MainModelManager.ReConnetCopiedCells(copiedList, softwareCells, mainModel);
            MainModelManager.SetIntegrationOfCopiedCells(copiedList, mainModel);
            return copiedList;
        }


        public static void MakeIntegrationIncludingAllConnected(SoftwareCell parentCell, SoftwareCell subCell, MainModel mainModel)
        {
            parentCell.Integration.AddUnique(subCell);
            TraverseChildren(subCell, child => parentCell.Integration.AddUnique(child), mainModel);
            TraverseChildrenBackwards(subCell, child => parentCell.Integration.AddUnique(child), mainModel);
        }
    }

}