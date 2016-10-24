using System.Collections.Generic;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [ImplementPropertyChanged]
    public class MainModel
    {
        public List<DataStream> Connections { get; internal set; }
        public List<SoftwareCell> SoftwareCells { get; internal set; }
        public MainModel()
        {
            Connections = new List<DataStream>();
            SoftwareCells = new List<SoftwareCell>();
        }

    }
}