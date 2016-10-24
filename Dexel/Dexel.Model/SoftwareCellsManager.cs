using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Model.DataTypes;

namespace Dexel.Model
{

    public class SoftwareCellsManager
    {
        public IEnumerable<SoftwareCell> GetAll(Guid softwareCellID, MainModel mainModel)
        {
            return mainModel.SoftwareCells.Where(x => x.ID.Equals(softwareCellID));
        }


        public SoftwareCell GetFirst(Guid destinationID, MainModel mainModel)
        {
            return mainModel.SoftwareCells.First(x => x.ID.Equals(destinationID));
        }


        public SoftwareCell GetFristByID(Guid destinationID, MainModel mainModel)
        {
            return mainModel.SoftwareCells.First(x => x.ID.Equals(destinationID));
        }


        public SoftwareCell CreateNew(string name = "")
        {
            return new SoftwareCell
            {
                ID = Guid.NewGuid(),
                Name = name
            };
        }


        public void RemoveDefinitionsFromSourceAndDestination(DataStreamDefinition defintion, SoftwareCell source,
            SoftwareCell destination)
        {
            source.OutputStreams.RemoveAll(
                x => x.DataNames == defintion.DataNames && x.ActionName == defintion.ActionName);
            destination.InputStreams.RemoveAll(
                x => x.DataNames == defintion.DataNames && x.ActionName == defintion.ActionName);
        }
    }

}