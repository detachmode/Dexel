using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dexel.Library;
using Dexel.Model.DataTypes;

namespace Dexel.Model
{
    public static class DataTypeManager
    {
        public static List<string> GetUndefinedTypenames(MainModel mainmodel)
        {
            var list = CollectAllTypesFromDsds(mainmodel);
            var alltypes = list.Concat(CollectAllSubtypes(mainmodel)).ToList();
            alltypes = FilterOutDuplicatesAndCustomTypes(alltypes, mainmodel);
            return alltypes;
        }

        public static IEnumerable<string> CollectAllTypesFromDsds(MainModel mainmodel)
        {
            var alldsds = mainmodel.FunctionUnits.SelectMany(sc => sc.InputStreams.Concat(sc.OutputStreams).ToList());
            return alldsds.SelectMany(dsd => DataStreamParser.GetInputAndOutput(dsd.DataNames).Select(x => x.Type));
        }

        public static List<string> FilterOutDuplicatesAndCustomTypes(List<string> types, MainModel mainmodel)
        {
            types = types.Distinct().ToList();
            types.RemoveAll(t => mainmodel.DataTypes.Any(dt => dt.Name == t));
            types.RemoveAll(string.IsNullOrEmpty);
            return types;

        }




        public static IEnumerable<string> CollectAllSubtypes(MainModel mainmodel)
        {
            return mainmodel.DataTypes.Where(x => x.SubDataTypes != null).SelectMany(dt => dt.SubDataTypes.Select(subDt => subDt.Type.Trim()));
        }


        public static List<CustomDataType> GetTypesRecursive(List<CustomDataType> found, MainModel mainModel)
        {
            var count = found.Count;
            var subtypes = found.Select(dt => dt.SubDataTypes.SelectMany(x => x.Type));
            mainModel.DataTypes.Where(dt => subtypes.Contains(dt.Name)).ForEach(found.AddUnique);
            return found.Count == count ? found : GetTypesRecursive(found, mainModel);
        }
    }
}
