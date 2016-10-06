using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Navigation;
using PropertyChanged;
using SharpFlowDesign.DebuggingHelper;
using SharpFlowDesign.Model;

namespace SharpFlowDesign.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel
    {
        private static MainViewModel self;

        public MainViewModel()
        {
            SoftwareCells = new ObservableCollection<IOCellViewModel>();
            Connections = new ObservableCollection<ConnectionViewModel>();
            LoadFromModel(Mockdata.RomanNumbers());
            //AddToViewModelRecursive(FlowDesignManager.Root);
        }


        public ObservableCollection<ConnectionViewModel> Connections { get; set; }
        public ObservableCollection<IOCellViewModel> SoftwareCells { get; set; }
        public ConnectionViewModel TemporaryConnection { get; set; }


        public void LoadFromModel(MainModel mainModel)
        {
            LoadConnection(mainModel.Connections);
            LoadSoftwareCells(mainModel.SoftwareCells);
        }

        private void LoadSoftwareCells(List<SoftwareCell> softwareCells)
        {
            SoftwareCells.Clear();
            softwareCells.ForEach(modelSoftwareCell =>
            {
                var vm = new IOCellViewModel();
                vm.LoadFromModel(modelSoftwareCell);
                SoftwareCells.Add(vm);
            });
        }

        private void LoadConnection(List<DataStream> dataStreams)
        {
            Connections.Clear();
            dataStreams.Where(x => x.Sources.Any() && x.Destinations.Any()).ToList().ForEach(modelConnection =>
            {
                var vm = new ConnectionViewModel();
                vm.LoadFromModel(modelConnection);
                Connections.Add(vm);
            });
        }

        //public void AddToViewModelRecursive(SoftwareCell cell, IOCellViewModel previous = null)
        //{
        //    var cellvm = IOCellViewModel.Create(cell);
        //    SoftwareCells.Add(cellvm);
        //    if (previous != null)
        //    {
        //        Connections.Add(new ConnectionViewModel(previous, cellvm) {DataNames = cell.InputStreams.First().DataNames});
        //    }

        //    var destinations = cell.OutputStreams.SelectMany(stream => stream.Destinations).ToList();
        //    destinations.ForEach(x => AddToViewModelRecursive(x, cellvm));
        //}




        public static MainViewModel Instance()
        {
            return self ?? (self = new MainViewModel());
        }
    }
}