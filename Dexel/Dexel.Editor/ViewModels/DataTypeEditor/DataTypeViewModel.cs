using System.Linq;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels.DataTypeEditor
{
    [ImplementPropertyChanged]
    public class DataTypeViewModel
    {

        public CustomDataType Model { get; set; }
        public string Definitions { get; set; }



        public void UpdateModel(string text)
        {
            Model.SubDataTypes = text.Split('\n').Where(x => !string.IsNullOrEmpty(x)).Select(s =>
            {
                var splitted = s.Split(':');
                if (splitted.Length == 1)
                {
                    return new SubDataType
                    {
                        Name = "",
                        Type = splitted.Last()
                    };
                }             
                return new SubDataType
                {
                    Name = splitted.First(),
                    Type = splitted.Last()
                };
            }).ToList();
        }


    }
}