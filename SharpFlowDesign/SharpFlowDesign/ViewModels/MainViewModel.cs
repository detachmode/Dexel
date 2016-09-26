using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using SharpFlowDesign.XML;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel 
    {
        public MainViewModel()
        {
            Items = new ObservableCollection<IOCellViewModel>();
        }

        public ObservableCollection<IOCellViewModel> Items { get; set; }

       
    }

}
