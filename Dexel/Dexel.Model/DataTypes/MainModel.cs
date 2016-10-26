using System;
using System.Collections.Generic;
using PropertyChanged;

namespace Dexel.Model.DataTypes
{
    [Serializable]
    [ImplementPropertyChanged]
    public class MainModel
    {
        public List<SoftwareCell> SoftwareCells { get; set; }
        public List<DataStream> Connections { get;  set; }
        
        public MainModel()
        {
            Connections = new List<DataStream>();
            SoftwareCells = new List<SoftwareCell>();
        }

    }
}