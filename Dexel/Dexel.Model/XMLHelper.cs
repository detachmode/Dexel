using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Dexel.Model
{
    public static class XMLHelper
    {

        public static void SaveToXML<T>(this T obj, string path)
        {


            var xmlSerializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            using (var writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented })
            {
                xmlSerializer.Serialize(writer, obj);
                File.WriteAllText(path, stringWriter.ToString());
            }
        }

        public static T FromXML<T>(string path)
        {
            var xsSubmit = new XmlSerializer(typeof(T));
            using (var reader = new FileStream(path, FileMode.Open))
            {
                return (T)xsSubmit.Deserialize(reader);
            }
        }
    }
}
