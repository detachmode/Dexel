using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SharpFlowDesign.XML
{

    public class XmlConverter
    {
        public string ConvertObject<T>(T objectData)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (StringWriter textWriter = new Utf8StringWriter())
            {
                xmlSerializer.Serialize(textWriter, objectData);
                return textWriter.ToString();
            }
        }


        public T ConvertObjectBack<T>(string objectDataString)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(objectDataString);
            MemoryStream stream = new MemoryStream(byteArray);
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            return (T) serializer.Deserialize(stream);
        }


        public string[] SimpleConvertBackToString(string dataString)
        {
            dataString.Replace("</", "<");
            char[] delimiters = new char[] {'<', '>'};
            string[] splittedData = dataString.Split(delimiters);
            return splittedData;
        }


        /// <summary>
        /// Ändert das Encoding von dem Serializer von UTF-16 auf UTF-8
        /// </summary>
        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }

}