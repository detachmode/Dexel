using System;
using System.Linq;

namespace Roslyn
{
    public static class Helper
    {
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                return "";
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(string input)
        {
            if (String.IsNullOrEmpty(input))
                return "";
            return input.First().ToString().ToLower() + input.Substring(1);
        }



        public static T TryCatch<T>(Func<T> func, string errormsg)
        {
            try
            {
                return func();
            }
            catch
            {
                throw new Exception(errormsg);
            }
        }
    }
}