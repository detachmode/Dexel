using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowDesignModel
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


        public static SoftwareCell GetFristByID(Guid destinationID, MainModel mainModel)
        {
            return mainModel.SoftwareCells.First(x => x.ID.Equals(destinationID));
        }


        public static SoftwareCell CreateNew(string name = "")
        {
            return new SoftwareCell()
            {
                ID = Guid.NewGuid(),
                Name = name
            };
        }

        public static void RemoveDefinitionsFromSourceAndDestination(DataStreamDefinition defintion, SoftwareCell source, SoftwareCell destination)
        {
            source.OutputStreams.RemoveAll(x => x.DataNames == defintion.DataNames && x.ActionName == defintion.ActionName);
            destination.InputStreams.RemoveAll(x => x.DataNames == defintion.DataNames && x.ActionName == defintion.ActionName);
        }
        

    }

}