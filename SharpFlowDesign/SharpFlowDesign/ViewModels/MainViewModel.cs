using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PropertyChanged;
using SharpFlowDesign.Behavior;
using SharpFlowDesign.CustomControls;
using SharpFlowDesign.DebuggingHelper;
using SharpFlowDesign.Model;

namespace SharpFlowDesign.ViewModels
{

    [ImplementPropertyChanged]
    public class MainViewModel : IDropable
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


        public List<Type> AllowedDropTypes => new List<Type>
        {
            typeof(ConnectionViewModel)
        };


        public void Drop(object data, int index = -1)
        {
            data.TryCast<ConnectionViewModel>(
                connectionVM => Interactions.DeConnect(connectionVM.Model, MainModel.Get()));
            Interactions.ViewRedraw();
        }


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





        public static MainViewModel Instance()
        {
            return self ?? (self = new MainViewModel());
        }
    }

}