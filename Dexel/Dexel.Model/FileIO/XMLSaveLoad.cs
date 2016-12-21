using System.IO;
using System.Text;
using System.Xml.Serialization;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;

namespace Dexel.Model.FileIO
{

    public static class XMLSaveLoad
    {
        public static void SaveToXML(string path, MainModel mainModel)
        {
            var xmlSerializer = new XmlSerializer(typeof(MainModel));
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                xmlSerializer.Serialize(sw, mainModel);
            }
        }


        public static MainModel LoadFromXml(string path)
        {
            var xsSubmit = new XmlSerializer(typeof(MainModel));
            using (var reader = new FileStream(path, FileMode.Open))
            {
                var loadedMainModel = (MainModel) xsSubmit.Deserialize(reader);
                MainModelManager.SetParents(loadedMainModel);
                MainModelManager.SolveConnectionReferences(loadedMainModel);
                MainModelManager.SolveIntegrationReferences(loadedMainModel);
                return loadedMainModel;
            }
        }
    }

}