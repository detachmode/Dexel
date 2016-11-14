using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dexel.Library
{
    public static class LINQExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static void AddUnique<T>(this List<T> enumeration, T element)
        {
            if (enumeration.Contains(element))
            {
                return;
            }
            enumeration.Add(element);
           
        }
    }
}
