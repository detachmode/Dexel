using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlowDesign.Model;

namespace SharpFlowDesign
{

    public static class SoftwareCellsManager
    {
        public static IEnumerable<SoftwareCell> GetAll(Guid softwareCellID, MainModel mainModel)
        {
            return mainModel.SoftwareCells.Where(x => x.ID.Equals(softwareCellID));
        }

        public static SoftwareCell GetFirst(Guid destinationID, MainModel mainModel)
        {
            return mainModel.SoftwareCells.First(x => x.ID.Equals(destinationID));
        }


    }

}