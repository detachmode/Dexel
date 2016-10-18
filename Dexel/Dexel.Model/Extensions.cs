using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Contracts.Model;

namespace Dexel.Model
{
    public static class Extensions
    {


        public static bool IsDefinitionIn(this IDataStream defintion, IEnumerable<IDataStreamDefinition> enumerable)
        {
            return enumerable.Any(x => x.IsEquals(defintion));
        }

        public static bool IsEquals(this IDataStreamDefinition def1, IDataStreamDefinition def2)
        {
            return def1.DataNames == def2.DataNames && def1.ActionName == def2.ActionName;
        }

        public static bool IsEquals(this IDataStreamDefinition def1, IDataStream def2)
        {
            return def1.DataNames == def2.DataNames && def1.ActionName == def2.ActionName;
        }

        public static void WhenProperty(this PropertyChangedEventArgs propertyChangedEventArgs, string propname, Action isPropertyAction)
        {
            if (propertyChangedEventArgs.PropertyName == propname)
            {
                isPropertyAction();
            }
        }
    }
}
