using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dexel.Editor.Annotations;
using Dexel.Library;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels
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
                var str = s.Split(':');              
                return new DataType
                {
                    Name = str.First(),
                    Type = str.Last()
                };
            }).ToList();
        }


    }
}