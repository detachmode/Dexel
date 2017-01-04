using System.IO;
using Dexel.Model.DataTypes;
using Dexel.Model.FileIO;

namespace Dexel.Editor.FileIO
{
    public static class FileSaveLoad
    {


        public delegate MainModel LoaderDelegate(string path);

        public static LoaderDelegate GetFileLoader(string fileName)
        {
            LoaderDelegate func;
            switch (Path.GetExtension(fileName))
            {
                case ".yaml":
                    func = YAMLSaveLoader.LoadFromYaml<MainModel>;
                    return func;
                case ".json":
                    func = JsonSaveLoad.LoadFromJson;
                    return func;
                case ".xml":
                    func = XMLSaveLoad.LoadFromXml;
                    return func;
                default:
                    return null;

            }
        }

        public delegate void SaverDelegate(string path, MainModel mainModel);
        
        public static SaverDelegate GetFileSaver(string fileName)
        {
            SaverDelegate func;
            switch (Path.GetExtension(fileName))
            {
                case ".yaml":
                    func = YAMLSaveLoader.SaveToYaml;
                    return func;
                case ".json":
                    func = JsonSaveLoad.SaveToJson;
                    return func;
                case ".xml":
                    func = XMLSaveLoad.SaveToXML;
                    return func;
                default:
                    return null;

            }
        }
    }
}