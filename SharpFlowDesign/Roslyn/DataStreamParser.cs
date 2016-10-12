using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Roslyn
{
    public  static class DataStreamParser
    {

        public static IEnumerable<NameType> ParseDataNames(string dataNames, int pipePart = 1)
        {
            var datanames = GetPipePart(dataNames, pipePart);
            return ConvertToNameTypes(datanames);
        }

        private static IEnumerable<NameType> ConvertToNameTypes(string datanames)
        {
            return datanames.Split(',').Select(x =>
            {
                var splitted = x.Split(':');
                return new NameType()
                {
                    Name = splitted.Length == 2 ? splitted[0].Trim() : null,
                    Type = splitted.Length == 2 ? splitted[1].Trim() : splitted[0].Trim()
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