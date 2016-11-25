using System.Linq;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels.DataTypeEditor
{
    [ImplementPropertyChanged]
    public class DataTypeViewModel
    {

        public DataType Model { get; set; }
        public string Definitions { get; set; }



        public void UpdateModel(string text)
        {
            Model.DataTypes = text.Split('\n').Where(x => !string.IsNullOrEmpty(x)).Select(s =>
            {
                var splitted = s.Split(':');
                if (splitted.Length == 1)
                {
                    return new DataType
                    {
                        Name = "",
                        Type = splitted.Last()
                    };
                }             
                return new DataType
                {
                    Name = splitted.First(),
                    Type = splitted.Last()
                };
            }).ToList();
        }


    }
}