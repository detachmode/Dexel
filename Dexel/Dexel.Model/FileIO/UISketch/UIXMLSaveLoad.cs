using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;

namespace Dexel.Model.FileIO.UISketch
{
    public static class UiXMLSaveLoad
    {
        public static void SaveToXML(string path, Collection<SketchRectangle> rectanglesCollection)
        {
            var xmlSerializer = new XmlSerializer(typeof(Collection<SketchRectangle>));
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                xmlSerializer.Serialize(sw, rectanglesCollection);
            }
        }


        public static Collection<SketchRectangle> LoadFromXml(string path)
        {
            var xsSubmit = new XmlSerializer(typeof(Collection<SketchRectangle>));
            using (var reader = new FileStream(path, FileMode.Open))
            {
                var loadedRectanglesCollection = (Collection<SketchRectangle>)xsSubmit.Deserialize(reader);
                return loadedRectanglesCollection;
            }
        }
    }
}
