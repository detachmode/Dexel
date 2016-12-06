using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dexel.Model.DataTypes
{
    public interface IModelWithID
    {
        Guid ID { get; set; }
    }
}
