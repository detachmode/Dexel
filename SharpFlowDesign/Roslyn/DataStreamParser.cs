using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Roslyn
{
    public class NameType
    {
        public string Name, Type;
        public bool IsArray;
        public bool IsList;
    }

    public  static class DataStreamParser
    {

        public static IEnumerable<NameType> GetOutputPart(string dataNames)
        {
            var datanames = GetPipePart(dataNames, 1);
            return ConvertToNameTypes(datanames);
        }

        public static IEnumerable<NameType> GetInputPart(string dataNames)
        {
            var datanames = GetPipePart(dataNames, 2);
            return ConvertToNameTypes(datanames);
        }


        private static IEnumerable<NameType> ConvertToNameTypes(string datanames)
        {
            return datanames.Split(',').Select(x =>
            {
                bool isArray = false, isList = false;
                var splitted = x.Split(':').Select(s =>
                {
                    if (s.Contains('*'))
                        isList = true;

                    if (s.Contains("[]"))
                        isArray = true;
                    string cleaned = Regex.Replace(s, "[@,\\.\";' \\[\\]\\\\]", string.Empty);
                    return cleaned.Trim();

                }).ToArray();
                return new NameType()
                {
                    Name = splitted.Length == 2 ? splitted[0] : null,
                    Type = splitted.Length == 2 ? splitted[1] : splitted[0],
                    IsArray = isArray,
                    IsList = isList
                };
            });
        }

        private static string GetPipePart(string dataNames, int pipePart)
        {
            if (!dataNames.Contains("|")) return dataNames;

            var matches = Regex.Matches(dataNames, @"(.*)\|(.*)");
            return matches[0].Groups[pipePart].Value;
        }
    }
}