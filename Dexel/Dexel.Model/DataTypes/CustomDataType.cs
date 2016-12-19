using System.Collections.Generic;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [ImplementPropertyChanged]
    public class CustomDataType
    {
        public string Name { get; set; }
        public List<SubDataType> SubDataTypes { get; set; }
    }

    [ImplementPropertyChanged]
    public class SubDataType
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

}

   