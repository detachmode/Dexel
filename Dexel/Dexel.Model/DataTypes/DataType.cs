using System.Collections.Generic;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [ImplementPropertyChanged]
    public class DataType
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<DataType> DataTypes { get; set; }
    }
}

   