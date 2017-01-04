using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dexel.Model.DataTypes;

namespace Dexel.Model.Common
{

    public static class Extensions
    {
        public static bool IsDefinitionIn(this DataStream defintion, IEnumerable<DataStreamDefinition> enumerable)
        {
            return enumerable.Any(x => x.IsEquals(defintion));
        }


        public static bool IsEquals(this DataStreamDefinition def1, DataStreamDefinition def2)
        {
            return def1.DataNames == def2.DataNames && def1.ActionName == def2.ActionName;
        }


        public static bool IsEquals(this DataStreamDefinition def1, DataStream def2)
        {
            return def1.DataNames == def2.DataNames;
        }


        public static void WhenProperty(this PropertyChangedEventArgs propertyChangedEventArgs, string propname,
            Action isPropertyAction)
        {
            if (propertyChangedEventArgs.PropertyName == propname)
            {
                isPropertyAction();
            }
        }
    }

}