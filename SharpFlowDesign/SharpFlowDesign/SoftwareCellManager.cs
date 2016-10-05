using System;
using System.Linq;
using SharpFlowDesign.Model;

namespace SharpFlowDesign
{

    public static class SoftwareCellManager
    {
        public static SoftwareCell GetFristByID(Guid destinationID, MainModel mainModel)
        {
            return mainModel.SoftwareCells.First(x => x.ID.Equals(destinationID));
        }
    }

}