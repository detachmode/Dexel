using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SharpFlowDesign.UserControls;
using SharpFlowDesign.XML;

namespace SharpFlowDesign.ViewModels
{

    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            items = new ObservableCollection<IOCellViewModel>();
        }

        private ObservableCollection<IOCellViewModel> items;
        public ObservableCollection<IOCellViewModel> Items
        {
            get
            {
                return items;
            }

            set
            {
                
                items = value;

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
