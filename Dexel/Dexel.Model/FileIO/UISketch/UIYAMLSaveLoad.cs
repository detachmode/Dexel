using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Model.DataTypes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

namespace Dexel.Model.FileIO.UISketch
{
    public static class UiYAMLSaveLoad
    {
        public static void SaveToYaml(string path, Collection<SketchRectangle> rectanglesCollection)
        {
            var yamlser = new Serializer();

            using (var writer = new StringWriter())
            {
                yamlser.Serialize(writer, rectanglesCollection);
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
