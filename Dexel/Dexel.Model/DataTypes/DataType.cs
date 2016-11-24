using System.Collections.Generic;

namespace Dexel.Model.DataTypes
{
    public class DataType
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<DataType> DataTypes { get; set; }
    }
}

   