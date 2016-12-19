using System;
using System.Collections.Generic;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [Serializable]
    [ImplementPropertyChanged]
    public class MainModel
    {
        public List<FunctionUnit> FunctionUnits { get; set; }
        public List<DataStream> Connections { get;  set; }
        public List<CustomDataType> DataTypes { get; set; } 
        
        public MainModel()
        {
            Connections = new List<DataStream>();
            FunctionUnits = new List<FunctionUnit>();
            DataTypes = new List<CustomDataType>();
        }

    }
}