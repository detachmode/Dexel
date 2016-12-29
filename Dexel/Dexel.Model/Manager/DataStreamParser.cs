using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dexel.Model.DataTypes;

namespace Dexel.Model.Manager
{
    public class NameType
    {
        public string Name, Type;
        public bool IsArray;
        public bool IsList;
        public bool IsInsideStream;
    }

    public enum DataFlowImplementationStyle
    {
        AsAction,
        AsReturn
    }

    public class MethodSignaturePart
    {
        public DataFlowImplementationStyle ImplementWith;
        public DataStreamDefinition DSD;
    }

    public static class DataStreamParser
    {
        public static IEnumerable<NameType> GetInputAndOutput(string rawdatanames)
        {
            return GetInputPart(rawdatanames).Concat(GetOutputPart(rawdatanames));
        }

        public static List<NameType> GetOutputPart(string rawdatanames)
        {
            var result = new List<NameType>();


            var firstdatanames = GetPipePart(rawdatanames, 1);
            var isInsideStream = IsStream(firstdatanames);
            var insideParenthesis = GetInsideParenthesis(firstdatanames);

            CommaSeparator(insideParenthesis, onEach: s
                => ConvertToNameType(s, isInsideStream, onNewNameType: nametype
                    => result.Add(nametype)));

            return result;
        }


        public static bool IsStream(string rawdatanames)
        {
            return Regex.IsMatch(rawdatanames, @"^\s*\(.*\)\*\s*$");
        }

        public static void CheckIsStream(string rawdatanames, Action isStream = null, Action isNotStream = null)
        {
            if (Regex.IsMatch(rawdatanames, @"^\(.*\)\*$"))
                isStream?.Invoke();
            else
            {
                isNotStream?.Invoke();
            }
        }


        public static IEnumerable<NameType> GetInputPart(string rawdatanames)
        {
            var result = new List<NameType>();

            HasThreeDotSyntax(rawdatanames,
                () => result = GetOutputPart(rawdatanames).ToList());

            AddSecondPartDataNames(rawdatanames, result);

            return result;
        }


        private static void AddSecondPartDataNames(string rawdatanames, ICollection<NameType> result)
        {

            var isInsideStream = IsStream(rawdatanames);
            var insideParenthesis = GetInsideParenthesis(rawdatanames);
            var seconddatanames = GetPipePart(insideParenthesis, 2);
            CommaSeparator(seconddatanames, onEach: s
                => ConvertToNameType(s, isInsideStream, onNewNameType: result.Add));
        }


        private static string GetInsideParenthesis(string rawdatanames)
        {
            string result = rawdatanames;
            var matches = Regex.Matches(rawdatanames, @"^\s*\((.*)\)\*?\s*$");
            if (matches.Count == 1)
            {
                result = matches[0].Groups[1].Value;
            }


            return result;
        }


        private static void HasThreeDotSyntax(string rawdatanames, Action onTrue)
        {
            var getAlsoLastOutput = Regex.IsMatch(rawdatanames, @"\|\s*\.{3}\s+");
            if (getAlsoLastOutput)
            {
                onTrue();
            }
        }





        private static void CommaSeparator(string datanames, Action<string> onEach)
        {
            if (String.IsNullOrEmpty(datanames.Trim()))
                return;
            datanames.Split(',').ToList().ForEach(onEach);
        }


        public static void ConvertToNameType(string singledataname, bool isInsideStream, Action<NameType> onNewNameType)
        {
            bool isArray = false, isList = false;


            var splitted = singledataname.Split(':').Select(s =>
            {
                if (s.Contains('*'))
                    isList = true;

                if (s.Contains("[]"))
                    isArray = true;

                var cleaned = Regex.Replace(s, "[@,\\.\";'*() \\[\\]\\\\]", String.Empty);
                return cleaned.Trim();
            }).ToArray();

            onNewNameType(new NameType()
            {
                Name = splitted.Length == 2 ? splitted[0] : null,
                Type = splitted.Length == 2 ? splitted[1] : splitted[0],
                IsArray = isArray,
                IsInsideStream = isInsideStream,
                IsList = isList
            });
        }


        private static string GetPipePart(string dataNames, int pipePart)
        {
            if (!dataNames.Contains("|")) return dataNames;

            var matches = Regex.Matches(dataNames, @"(.*)\|(.*)");
            return matches[0].Groups[pipePart].Value;
        }


        public static void OutputByReturn(FunctionUnit functionUnit, Action<DataStreamDefinition> onFound)
        {
            var noActionsnames = functionUnit.OutputStreams.Where(dsd => string.IsNullOrWhiteSpace(dsd.ActionName)).ToList();
            if (noActionsnames.Count == 1)
            {
                CheckIsStream(noActionsnames.First().DataNames,
                    isNotStream: () =>
                    {
                        onFound(noActionsnames.First());
                    },
                    isStream: () =>
                    {
                        CheckIsStream(functionUnit.InputStreams.First().DataNames,
                            isStream: () => onFound(noActionsnames.First()));
                    });
            }
        }
    }
}