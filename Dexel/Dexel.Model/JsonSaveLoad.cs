using System.IO;
using System.Linq;
using Dexel.Model.DataTypes;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Dexel.Model
{
    public static class JsonSaveLoad
    {
        public static void SaveToJson(string path, MainModel mainModel)
        {
            var settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            var serializer = JsonSerializer.Create(settings);
            serializer.Formatting = Formatting.Indented;


            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, mainModel);
                File.WriteAllText(path, writer.ToString());
            }
        }

        public static MainModel LoadFromJson(string path)
        {
            //var settings = new JsonSerializerSettings
            //{
            //    PreserveReferencesHandling = PreserveReferencesHandling.Objects
            //};
            var ret = JsonConvert.DeserializeObject<MainModel>(File.ReadAllText(path));
            return ret;
        }
    }
}