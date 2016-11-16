using System.IO;
using System.Linq;
using Dexel.Model.DataTypes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Dexel.Model
{

    public static class YAMLSaveLoader
    {
        public static void SaveToYaml(string path, MainModel mainModel)
        {
            var yamlser = new Serializer();

            using (var writer = new StringWriter())
            {
                yamlser.Serialize(writer, mainModel);
                File.WriteAllText(path, writer.ToString());
            }
        }


        public static T LoadFromYaml<T>(string path)
        {
            var input = new StringReader(File.ReadAllText(path));
            var deserializer = new Deserializer();

            var objectDeserializer = deserializer.NodeDeserializers
                .Select((d, i) => new
                {
                    Deserializer = d as ObjectNodeDeserializer,
                    Index = i
                })
                .First(d => d.Deserializer != null);


            return deserializer.Deserialize<T>(input);
        }
    }

}