using System;
using System.Linq;

namespace Roslyn
{
    public static class Helper
    {
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new Exception("Couldn't convert to camel case");
            return input.First().ToString().ToUpper() + input.Substring(1);
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