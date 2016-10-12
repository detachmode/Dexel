using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowDesignModel
{
    public static class Extensions
    {
        public static void WhenProperty(this PropertyChangedEventArgs propertyChangedEventArgs, string propname, Action isPropertyAction)
        {
            if (propertyChangedEventArgs.PropertyName == propname)
            {
                isPropertyAction();
            }
        }
    }
}
