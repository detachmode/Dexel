using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;
using PropertyChanged;
using YamlDotNet.Serialization;

namespace Dexel.Model.DataTypes
{
    [Serializable]
    [ImplementPropertyChanged]
    public class MainModel
    {
        public List<FunctionUnit> FunctionUnits { get; set; }
        public List<DataStream> Connections { get;  set; }
        public List<CustomDataType> DataTypes { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        [YamlIgnore]
        public RuntimeInfoModel Runtime { get; } = new RuntimeInfoModel();
        
        public MainModel()
        {
            Connections = new List<DataStream>();
            FunctionUnits = new List<FunctionUnit>();
            DataTypes = new List<CustomDataType>();
        }

    }

    public class RuntimeInfoModel
    {
        public bool IsLoading { get; set; }
        public int MissingDataTypes { get; set; }
    }
}