using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Editor.ViewModels.UI_Sketches;
using Dexel.Model.DataTypes;
using Dexel.Model.FileIO;
using Dexel.Model.FileIO.UISketch;
using ICSharpCode.AvalonEdit.Editing;

namespace Dexel.Editor.FileIO
{
    class UISketches_SaveLoad
    {
        public static void SaveToFile(string fileName, Collection<SketchRectangle> rectangleCollection)
        {
            var saver = GetFileSaver(fileName);
            saver?.Invoke(fileName, rectangleCollection);
        }

        public static Collection<SketchRectangle> LoadFromFile(string fileName)
        {
            var loader = GetFileLoader(fileName);
            return loader?.Invoke(fileName);
        }

        public delegate Collection<SketchRectangle> LoaderDelegate(string path);

        public static LoaderDelegate GetFileLoader(string fileName)
        {
            LoaderDelegate func;
            switch (Path.GetExtension(fileName))
            {
                case ".yaml":
                    func = UiYAMLSaveLoad.LoadFromYaml<Collection<SketchRectangle>>;
                    return func;
                case ".json":
                    func = UiJsonSaveLoad.LoadFromJson;
                    return func;
                case ".xml":
                    func = UiXMLSaveLoad.LoadFromXml;
                    return func;
                default:
                    return null;

            }
        }

        public delegate void SaverDelegate(string path, Collection<SketchRectangle> rectangleCollection);

        public static SaverDelegate GetFileSaver(string fileName)
        {
            SaverDelegate func;
            switch (Path.GetExtension(fileName))
            {
                case ".yaml":
                    func = UiYAMLSaveLoad.SaveToYaml;
                    return func; 
                case ".json":
                    func = UiJsonSaveLoad.SaveToJson;
                    return func;
                case ".xml":
                    func = UiXMLSaveLoad.SaveToXML;
                    return func; 
                default:
                    return null;

            }
        }

    }
}
