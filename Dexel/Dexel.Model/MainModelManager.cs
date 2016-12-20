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


        public static void AddNewOutput(FunctionUnit functionUnit, string datanames, string actionName = null)
        {
            FunctionUnitManager.NewOutputDef(functionUnit, datanames, actionName);
        }


        public static void AddNewInput(FunctionUnit functionUnit, string datanames, string actionName = null)
        {
            FunctionUnitManager.NewInputDef(functionUnit, datanames, actionName);
        }


        public static void RemoveConnection(Guid id, MainModel mainModel)
        {
            var datastream = DataStreamManager.GetFirst(id, mainModel);
            RemoveConnection(datastream, mainModel);
        }


        public static FunctionUnit AddNewFunctionUnit(string name, MainModel mainModel)
        {
            var newcell = FunctionUnitManager.CreateNew(name);
            mainModel.FunctionUnits.Add(newcell);
            return newcell;
        }


        public static void ConnectTwoFunctionUnits(FunctionUnit source, FunctionUnit destination, DataStreamDefinition defintion,
            MainModel mainModel)
        {
            ConnectTwoFunctionUnits(source, destination, defintion.DataNames, "", mainModel, defintion.ActionName);
        }


        public static DataStream ConnectTwoFunctionUnits(FunctionUnit source, FunctionUnit destination, string outputs,
            string inputs,
            MainModel mainModel, string actionName = "")
        {
            source.OutputStreams.RemoveAll(x => x.DataNames == outputs && x.ActionName == actionName);
            destination.InputStreams.RemoveAll(x => x.DataNames == inputs && x.ActionName == actionName);

            var sourceDef = FunctionUnitManager.NewOutputDef(source, outputs, actionName);
            var destinationDef = FunctionUnitManager.NewInputDef(destination, inputs, null);

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
            loadedMainModel.FunctionUnits.ForEach(sc =>
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
                    c.Sources.Select(dsd => DataStreamManager.GetDSDFromModel(dsd.ID, loadedMainModel.FunctionUnits))
                        .ToList();
                if (found.Any())
                {
                    c.Sources = found;
                }

                found =
                    c.Destinations.Select(
                        dsd => DataStreamManager.GetDSDFromModel(dsd.ID, loadedMainModel.FunctionUnits)).ToList();
                if (found.Any())
                {
                    c.Destinations = found;
                }
            });
        }


        public static void SolveIntegrationReferences(MainModel loadedMainModel)
        {
            var lookupID = loadedMainModel.FunctionUnits.ToLookup(sc => sc.ID);
            loadedMainModel.FunctionUnits.Where(sc => sc.Integration.Count != 0).ToList().ForEach(sc =>
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


        public static void RemoveAllConnectedFromIntegration(FunctionUnit functionUnit, MainModel mainModel)
        {
            FindIntegration(functionUnit, foundIntegration =>
            {
                foundIntegration.Integration.Remove(functionUnit);
                TraverseChildren(functionUnit, child => foundIntegration.Integration.Remove(child), mainModel);
                TraverseChildrenBackwards(functionUnit, child => foundIntegration.Integration.Remove(child), mainModel);
            }, mainModel);
        }


        public static void RemoveFromIntegrationsIncludingChildren(FunctionUnit functionUnit, MainModel mainModel)
        {
            FindIntegration(functionUnit, foundIntegration =>
            {
                foundIntegration.Integration.Remove(functionUnit);
                TraverseChildren(functionUnit, child => foundIntegration.Integration.Remove(child), mainModel);
            }, mainModel);
        }


        public static void TraverseChildren(FunctionUnit parent, Action<FunctionUnit> onEach, MainModel mainModel)
        {
            var foundConnections = mainModel.Connections.Where(c => c.Sources.Any(dsd => dsd.Parent == parent));
            foreach (var connection in foundConnections)
            {
                connection.Destinations.ForEach(dsd =>
                {
                    onEach(dsd.Parent);
                    TraverseChildren(dsd.Parent, onEach, mainModel);
                });
            }
        }


        public static void TraverseChildrenBackwards(FunctionUnit children, Action<FunctionUnit> onEach,
            MainModel mainModel)
        {
            var foundConnections = mainModel.Connections.Where(c => c.Destinations.Any(dsd => dsd.Parent == children));
            foreach (var connection in foundConnections)
            {
                connection.Sources.ForEach(dsd =>
                {
                    onEach(dsd.Parent);
                    TraverseChildrenBackwards(dsd.Parent, onEach, mainModel);
                });
            }
        }

        public static void TraverseChildrenBackwards(FunctionUnit children, Action<FunctionUnit, DataStream> onEach,
            List<DataStream> allconnections)
        {
            var foundConnections = allconnections.Where(c => c.Destinations.Any(dsd => dsd.Parent == children));
            foreach (var connection in foundConnections)
            {
                connection.Sources.ForEach(dsd =>
                {
                    onEach(dsd.Parent, connection);
                    TraverseChildrenBackwards(dsd.Parent, onEach, allconnections);
                });
            }
        }


        private static void FindIntegration(FunctionUnit functionUnit, Action<FunctionUnit> onFound, MainModel mainModel)
        {
            mainModel.FunctionUnits.Where(sc => sc.Integration.Contains(functionUnit))
                .ForEach(onFound);
        }


        public static void DeleteFunctionUnit(FunctionUnit functionUnit, MainModel mainModel)
        {
            // solve Integration logic
            AtleastOneInputConnected(functionUnit,
                () => RemoveFromIntegrationsIncludingChildren(functionUnit, mainModel),
                inputsNotConnected: () =>
                    RemoveFromIntegrations(functionUnit, mainModel));
                
            // remove functionunit itself
            RemoveAllConnectionsToFunctionUnit(functionUnit, mainModel);
            mainModel.FunctionUnits.Remove(functionUnit);
        }


        private static void RemoveFromIntegrations(FunctionUnit functionUnit, MainModel mainModel)
        {
            FindIntegration(functionUnit, integratedByCell => integratedByCell.Integration.Remove(functionUnit),
                mainModel);
        }


        private static void RemoveAllConnectionsToFunctionUnit(FunctionUnit functionUnit, MainModel mainModel)
        {
            var sources =
                mainModel.Connections.Where(c => c.Sources.Any(sc => functionUnit == sc.Parent));
            var destinations =
                mainModel.Connections.Where(c => c.Destinations.Any(sc => functionUnit == sc.Parent));
            var todelete = destinations.Concat(sources).ToList();
            todelete.ForEach(c => RemoveConnection(c, mainModel));
        }


        private static void AtleastOneInputConnected(FunctionUnit functionUnit, Action doAction,
            Action inputsNotConnected = null)
        {
            if (functionUnit.InputStreams.Any(dsd => dsd.Connected))
            {
                doAction();
            }
            else
            {
                inputsNotConnected?.Invoke();
            }
        }


        private static void SetIntegrationOfCopiedFunctionUnits(List<CopiedFunctionUnits> copiedList, MainModel mainModel)
        {
            copiedList.ForEach(cc =>
            {
                var orginal = mainModel.FunctionUnits.First(sc => sc.ID == cc.OriginGuid);
                orginal.Integration.ForEach(isc =>
                {
                    var incopied = copiedList.FirstOrDefault(x => x.OriginGuid == isc.ID);
                    if (incopied != null)
                    {
                        cc.NewFunctionUnit.Integration.Add(incopied.NewFunctionUnit);
                    }
                });
            });
        }


        private static void ReConnetCopiedFunctionUnits(List<CopiedFunctionUnits> copiedList, List<FunctionUnit> original,
            MainModel mainModel)
        {
            var connectionsOfSelectedFunctionUnits = mainModel.Connections.Where(c =>
                c.Sources.Any(y => original.Any(x => x == y.Parent))
                &&
                c.Destinations.Any(y => original.Any(x => x == y.Parent))
                ).ToList();


            connectionsOfSelectedFunctionUnits.ForEach(c =>
            {
                var datastream = c;
                var destination = datastream.Destinations.Select(y => y.Parent).First();
                var source = datastream.Sources.Select(y => y.Parent).First();

                var sourcedsd = copiedList.First(y => y.OriginGuid == source.ID).NewFunctionUnit.OutputStreams.First(
                    dsd => dsd.DataNames == datastream.Sources.First().DataNames);
                var destinationdsd = copiedList.First(y => y.OriginGuid == destination.ID).NewFunctionUnit.InputStreams.First(
                    dsd => dsd.DataNames == datastream.Destinations.First().DataNames);

                ConnectTwoDefintions(sourcedsd, destinationdsd, mainModel);
            });
        }


        private static FunctionUnit Duplicate(FunctionUnit functionUnit)
        {
            var newmodel = FunctionUnitManager.CreateNew(functionUnit.Name);
            newmodel.Position = functionUnit.Position;
            functionUnit.InputStreams.ForEach(dsd =>
            {
                var newdsd = DataStreamManager.NewDefinition(newmodel, dsd);
                newmodel.InputStreams.Add(newdsd);
            });
            functionUnit.OutputStreams.ForEach(dsd =>
            {
                var newdsd = DataStreamManager.NewDefinition(newmodel, dsd);
                newmodel.OutputStreams.Add(newdsd);
            });

            return newmodel;
        }


        private static List<CopiedFunctionUnits> DuplicateMany(List<FunctionUnit> softwareCells, MainModel mainModel)
        {
            var copiedList = new List<CopiedFunctionUnits>();
            softwareCells.ForEach(sc =>
            {
                var newCell = Duplicate(sc);
                var copiedCell = new CopiedFunctionUnits {OriginGuid = sc.ID, NewFunctionUnit = newCell};
                copiedList.Add(copiedCell);
                mainModel.FunctionUnits.Add(newCell);
            });
            return copiedList;
        }


        public static void MakeIntegrationIncludingChildren(FunctionUnit parentCell, FunctionUnit subCell,
            MainModel mainModel)
        {
            parentCell.Integration.AddUnique(subCell);
            TraverseChildren(subCell, child => parentCell.Integration.AddUnique(child), mainModel);
        }


        public static void MoveFunctionUnitsToClickedPosition(Point positionClicked, List<CopiedFunctionUnits> copiedList)
        {
            var delta = positionClicked - copiedList.First().NewFunctionUnit.Position;
            copiedList.ForEach(x => x.NewFunctionUnit.Position += delta);
        }


        public static List<CopiedFunctionUnits> Duplicate(List<FunctionUnit> softwareCells, MainModel mainModel)
        {
            var copiedList = DuplicateMany(softwareCells, mainModel);
            ReConnetCopiedFunctionUnits(copiedList, softwareCells, mainModel);
            SetIntegrationOfCopiedFunctionUnits(copiedList, mainModel);
            return copiedList;
        }


        public static void MakeIntegrationIncludingAllConnected(FunctionUnit parentCell, FunctionUnit subCell,
            MainModel mainModel)
        {
            parentCell.Integration.AddUnique(subCell);
            TraverseChildren(subCell, child => parentCell.Integration.AddUnique(child), mainModel);
            TraverseChildrenBackwards(subCell, child => parentCell.Integration.AddUnique(child), mainModel);
        }


        public static List<FunctionUnit> GetChildrenAndIntegrated(FunctionUnit functionUnit, List<FunctionUnit> found,
            MainModel mainModel)
        {
            found.AddUnique(functionUnit);
            functionUnit.Integration.ForEach(isc => GetChildrenAndIntegrated(isc, found, mainModel));

            TraverseChildren(functionUnit, child =>
            {
                found.AddUnique(child);
                child.Integration.ForEach(isc => GetChildrenAndIntegrated(isc, found, mainModel));
            }, mainModel);

           
            return found;
        }


        public class CopiedFunctionUnits
        {
            public FunctionUnit NewFunctionUnit;
            public Guid OriginGuid;
        }


        public static DataStream FindDataStream(DataStreamDefinition connectedDsd, MainModel mainModel)
        {
           return mainModel.Connections.FirstOrDefault(c => c.Sources.Contains(connectedDsd) || c.Destinations.Contains(connectedDsd));
        }


        public static object GetBeginningOfFlow(FunctionUnit startByFunctionUnit, MainModel mainModel)
        {
            object @return;

            if (startByFunctionUnit.InputStreams.Any())
                @return = startByFunctionUnit.InputStreams.First();
            else
                @return = startByFunctionUnit;

            TraverseChildrenBackwards(startByFunctionUnit, fu =>
            {
                if (fu.InputStreams.Any())
                    @return = fu.InputStreams.First();
                else
                    @return = fu;

            }, mainModel);

            return @return;
        }

        public static object GetEndOfFlow(FunctionUnit startByFunctionUnit, MainModel mainModel)
        {
            object @return;

            if (startByFunctionUnit.OutputStreams.Any())
                @return = startByFunctionUnit.OutputStreams.First();
            else
                @return = startByFunctionUnit;

            TraverseChildren(startByFunctionUnit, fu =>
            {
                if (fu.OutputStreams.Any())
                    @return = fu.OutputStreams.First();
                else
                    @return = fu;

            }, mainModel);

            return @return;
        }
    }

}