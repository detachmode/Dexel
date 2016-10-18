using System;
using System.Collections.Generic;
using System.Linq;
using Dexel.Contracts.Model;

namespace Dexel.Model
{

    public class SoftwareCellsManager : ISoftwareCellsManager
    {
        public IEnumerable<ISoftwareCell> GetAll(Guid softwareCellID, IMainModel mainModel)
        {
            return mainModel.SoftwareCells.Where(x => x.ID.Equals(softwareCellID));
        }


        public ISoftwareCell GetFirst(Guid destinationID, IMainModel mainModel)
        {
            return mainModel.SoftwareCells.First(x => x.ID.Equals(destinationID));
        }


        public ISoftwareCell GetFristByID(Guid destinationID, IMainModel mainModel)
        {
            return mainModel.SoftwareCells.First(x => x.ID.Equals(destinationID));
        }


        public ISoftwareCell CreateNew(string name = "")
        {
            return new SoftwareCell
            {
                ID = Guid.NewGuid(),
                Name = name
            };
        }


        public void RemoveDefinitionsFromSourceAndDestination(IDataStreamDefinition defintion, ISoftwareCell source,
            ISoftwareCell destination)
        {
            source.OutputStreams.RemoveAll(
                x => x.DataNames == defintion.DataNames && x.ActionName == defintion.ActionName);
            destination.InputStreams.RemoveAll(
                x => x.DataNames == defintion.DataNames && x.ActionName == defintion.ActionName);
        }
    }

}