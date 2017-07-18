using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model.DataTypes;
using Newtonsoft.Json;

namespace Dexel.Model.FileIO.UISketch
{
    public static class UiJsonSaveLoad
    {
        public static void SaveToJson(string path, Collection<SketchRectangle> rectanglesCollection)
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            var serializer = JsonSerializer.Create(settings);
            serializer.Formatting = Formatting.Indented;


            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, rectanglesCollection);
                File.WriteAllText(path, writer.ToString());
            }
        }


        public static Collection<SketchRectangle> LoadFromJson(string path)
        {
            //var settings = new JsonSerializerSettings
            //{
            //    PreserveReferencesHandling = PreserveReferencesHandling.Objects
            //};
            var ret = JsonConvert.DeserializeObject<Collection<SketchRectangle>>(File.ReadAllText(path));
            return ret;
        }
    }
}
