using System;

namespace Dexel.Editor.CustomControls
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
