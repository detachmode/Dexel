using System.Collections.Generic;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [ImplementPropertyChanged]
    public class MainModel
    {
        public List<DataStream> Connections { get; set; }
        public List<SoftwareCell> SoftwareCells { get; set; }
        public MainModel()
        {
            Connections = new List<DataStream>();
            SoftwareCells = new List<SoftwareCell>();
        }

    }
}