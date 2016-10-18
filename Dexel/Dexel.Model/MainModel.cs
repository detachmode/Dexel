using System.Collections.Generic;
using Dexel.Contracts.Model;

namespace Dexel.Model
{
    public class MainModel : IMainModel
    {
        public MainModel()
        {
            Connections = new List<IDataStream>();
            SoftwareCells = new List<ISoftwareCell>();
        }

        public List<IDataStream> Connections { get; set; }
        public List<ISoftwareCell> SoftwareCells { get; set; }
    }
}