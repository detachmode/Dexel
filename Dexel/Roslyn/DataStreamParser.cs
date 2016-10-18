using System;
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
        public bool IsInsideStream;
    }

    public  static class DataStreamParser
    {

        public static IEnumerable<NameType> GetOutputPart(string rawdatanames)
        {
            var result = new List<NameType>();
            var isInsideStream = IsStream(rawdatanames);
            var insideParenthesis = GetInsideParenthesis(rawdatanames);

            var firstdatanames = GetPipePart(insideParenthesis, 1);
          
            CommaSeparator(firstdatanames, onEach: s
                => ConvertToNameType(s, isInsideStream, onNewNameType: nametype
                    => result.Add(nametype)));

            return result;
        }


        private static bool IsStream(string rawdatanames)
        {
           return Regex.IsMatch(rawdatanames, @"^\(.*\)\*$");
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
            var matches = Regex.Matches(rawdatanames, @"^\((.*)\)\*?$");
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

                var cleaned = Regex.Replace(s, "[@,\\.\";'*() \\[\\]\\\\]", string.Empty);
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
    }
}