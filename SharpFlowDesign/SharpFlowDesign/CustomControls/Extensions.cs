using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpFlowDesign.CustomControls
{
    public static class  Extensions
    {
        public static void TryCast<T>(this object data, Action<T> onCastSuccess)
        {
            if (data.GetType() != typeof(T)) return;
            var casted = (T)data;
            onCastSuccess(casted);
        }
    }
}
