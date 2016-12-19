using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dexel.Model.DataTypes;
using PropertyChanged;


namespace Dexel.Model
{

    [ImplementPropertyChanged]
    public static class DataStreamManager
    {
        public static DataStream NewDataStream(string datanames)
        {
            var dataStream = new DataStream();
            dataStream.ID = Guid.NewGuid();
            dataStream.DataNames = datanames;
            return dataStream;
        }

        public static bool IsInput(this DataStreamDefinition dsd)
        {
            return dsd.Parent.InputStreams.Contains(dsd);
        }


        public static void Check(this DataStreamDefinition dsd, Action isInput, Action isOutput)
        {
            if (dsd.IsInput())
                isInput();
            else if (dsd.IsOutput())
                isOutput();
        }

        public static void GetFirstConnected(this List<DataStreamDefinition> dsds, Action<DataStreamDefinition> foundConnected, Action noConnected)
        {
            var firstconnected = dsds.FirstOrDefault(x => x.Connected);
            if (firstconnected != null)
                foundConnected(firstconnected);
            else
                noConnected();

        }

        

        public static bool IsOutput(this DataStreamDefinition dsd)
        {
            return dsd.Parent.OutputStreams.Contains(dsd);
        }


        public static DataStreamDefinition NewDefinition(SoftwareCell parent, string datanames, string actionsName = "",
            bool connected = false)
        {
            var dataStream = new DataStreamDefinition();
            dataStream.ID = Guid.NewGuid();
            dataStream.DataNames = datanames;
            dataStream.Parent = parent;
            dataStream.ActionName = actionsName;
            dataStream.Connected = connected;
            return dataStream;
        }


        public static DataStream GetFirst(Guid id, MainModel mainModel)
        {
            return mainModel.Connections.First(x => x.ID.Equals(id));
        }


        public static DataStream NewDataStream(DataStreamDefinition datastreamDefintion)
        {
            return NewDataStream(datastreamDefintion.DataNames);
        }


        public static DataStreamDefinition NewDefinition(SoftwareCell parent, DataStreamDefinition defintion,
            bool connected = false)
        {
            return NewDefinition(parent, defintion.DataNames, defintion.ActionName, connected);
        }


        public static void SetConnectedState(DataStream dataStream, bool state)
        {
            dataStream.Sources.ForEach(dsd => dsd.Connected = state);
            dataStream.Destinations.ForEach(dsd => dsd.Connected = state);
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


        public static string MergeDataNames(DataStreamDefinition sourceDSD, DataStreamDefinition destinationDSD)
        {
            if (destinationDSD == null)
                return sourceDSD.DataNames + " | ";

            if (sourceDSD.DataNames == destinationDSD.DataNames)
                return sourceDSD.DataNames;

            return sourceDSD.DataNames + " | " + destinationDSD.DataNames;
        }


        public static void ChangeDatanames(DataStream datastream, string newDatanames)
        {
            // update datanames of connection itself
            datastream.DataNames = newDatanames;

            // update datanames of DSDs
            TrySolveWithPipeNotation(newDatanames,
                onSuccess: (outputPart, inputPart) =>
                {
                    // TODO: doesn't support mutliple sources yet
                    datastream.Sources.First().DataNames = outputPart.Trim();
                    datastream.Destinations.First().DataNames = inputPart.Trim();
                },
                onNoSuccess: () =>
                {
                    // TODO: doesn't support mutliple destinations yet
                    datastream.Sources.First().DataNames = newDatanames.Trim();
                    datastream.Destinations.First().DataNames = newDatanames.Trim();
                });
        }



        private static string[] SolveNoParenthesis(string datanames, string[] strings)
        {
            if (strings != null)
                return strings;

            var withoutparenthesis =
                new Regex(
                    @"^\s*(?'output'.*)(?'pipe'\|)\s*(?'dots'\.{3})?(?'input'.*)");
            var matches = withoutparenthesis.Matches(datanames);
            if (matches.Count == 1)
            {
                // ReSharper disable once RedundantAssignment
                string input = "", output = "";
                var grps = matches[0].Groups;

                output = datanames.Split('|')[0];
                input = datanames.Split('|')[1];


                if (!grps["dots"].Success)
                    return new[] {output, input};

                input = grps["output"].Value.Trim() + ", " + grps["input"].Value.Trim();
                return new[] {output, input};
            }
            return null;
        }


        public static void TrySolveWithPipeNotation(string datanames, Action<string,string> onSuccess, Action onNoSuccess)
        {
            var withparenthesis =
                new Regex(
                    @"^\s*(?'open1'\()(?'output'.*)(?'close1'\)\*?)\s*(?'pipe'\|)\s*(?'open2'\()\s*(?'dots'\.{3})?(?'input'.*)(?'close2'\)\*?)\s*");
            var matches = withparenthesis.Matches(datanames);
            if (matches.Count == 1)
            {
                string input = "", output = "";
                var grps = matches[0].Groups;

                output = datanames.Split('|')[0];
                input = datanames.Split('|')[1];


                if (!grps["dots"].Success)
                {
                    onSuccess(output, input);
                }
                else
                {
                    input = grps["open2"].Value + grps["output"].Value.Trim() + ", " + grps["input"].Value.Trim() +
                       grps["close2"].Value;

                    onSuccess(output, input);
                }
            }
            else
            {
                onNoSuccess();
            }
        }


        public static DataStreamDefinition GetDSDFromModel(Guid id, List<SoftwareCell> softwareCells)
        {
            var inputDSDs = softwareCells.Select(sc => sc.InputStreams.FirstOrDefault(dsd => dsd.ID == id))
                .Where(x => x != null).ToList();
            if (inputDSDs.Any())
            {
                return inputDSDs.First();
            }

            var outputDSDs = softwareCells.Select(sc => sc.OutputStreams.FirstOrDefault(dsd => dsd.ID == id))
                .Where(x => x != null).ToList();
            if (outputDSDs.Any())
            {
                return outputDSDs.First();
            }
            return null;
        }


        public static void IsInSameCollection(DataStreamDefinition dsd1, DataStreamDefinition dsd2, Action<List<DataStreamDefinition>> onTrue, Action onFalse = null
            )
        {
            if (dsd1.Parent.InputStreams.Contains(dsd2) && dsd1.Parent.InputStreams.Contains(dsd1))
            {
                onTrue(dsd1.Parent.InputStreams);
            }
            else if (dsd1.Parent.OutputStreams.Contains(dsd2) && dsd1.Parent.OutputStreams.Contains(dsd1))
            {
                onTrue(dsd1.Parent.OutputStreams);
            }
            else
            {
                onFalse?.Invoke();
            }

        }


        public static void SwapDataStreamDefinitons(DataStreamDefinition dsd1, DataStreamDefinition dsd2, List<DataStreamDefinition> list)
        {
            var dsd1Index = list.IndexOf(dsd1);
            var dsd2Index = list.IndexOf(dsd2);


            list[dsd1Index] = dsd2;
            list[dsd2Index] = dsd1;
        }
    }

}