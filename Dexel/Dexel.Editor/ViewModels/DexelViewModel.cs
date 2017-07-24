using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dexel.Editor.ViewModels.DataTypeEditor;
using Dexel.Model.Common;
using Dexel.Model.DataTypes;
using Dexel.Model.Mockdata;
using PropertyChanged;

namespace Dexel.Editor.ViewModels
{
    [ImplementPropertyChanged]
    class DexelViewModel
    {
        public DexelViewModel()
        {
            Diagrams = new ObservableCollection<MainViewModel>();
            var vm1 = new MainViewModel();
            vm1.Model = Mockdata.StartMainModel();
            var vm2 = new MainViewModel();
            vm2.Model = new MainModel();
            Diagrams.Add(vm1);
            Diagrams.Add(vm2);
        }

        public ObservableCollection<MainViewModel> Diagrams { get; set; }
        public ObservableCollection<DataTypeViewModel> DataTypes { get; set; }
        public MainViewModel SelectedDiagram { get; set; }
        public ICommand AddNewDiagramCommand => new DelegateCommand(AddNewDiagram);
        public ICommand CloseCurrentDiagramCommand => new DelegateCommand(CloseCurrentDiagram);

        private void AddNewDiagram()
        {
            var vm = new MainViewModel();
            vm.Model = new MainModel();
            Diagrams.Add(vm);
        }

        private void CloseCurrentDiagram()
        {
            Diagrams.Remove(SelectedDiagram);
        }


        
    }
}
