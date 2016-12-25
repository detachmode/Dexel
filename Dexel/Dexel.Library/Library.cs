using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dexel.Library
{
    public static class Library
    {
        public static T DeepClone<T>(this T a)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        public static void TryCast<T>(this object data, Action<T> onCastSuccess)
        {
            if (data.GetType() != typeof(T)) return;
            var casted = (T)data;
            onCastSuccess(casted);
        }
    }
}
