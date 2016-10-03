using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using PropertyChanged;
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
            Connections.CollectionChanged += Connections_CollectionChanged;

            AddToViewModelRecursive(FlowDesignManager.Root);
        }


        public ObservableCollection<ConnectionViewModel> Connections { get; }
        public ObservableCollection<IOCellViewModel> SoftwareCells { get; set; }
        public ConnectionViewModel TemporaryConnection { get; set; }

        private void Connections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            foreach (ConnectionViewModel item in e.NewItems)
            {
                item.PropertyChanged += connection_PropertyChanged;
            }
        }

        private void connection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsDragging") return;
            var vm = sender as ConnectionViewModel;

            // as long as an connection is dragged around don't change the Collections
            if (vm != null && vm.IsDragging) return;

            MaybeRemoveConnection(vm);
        }

        private void MaybeRemoveConnection(ConnectionViewModel vm)
        {
            if (vm.End == null)
                Connections.Remove(vm);
        }


        public void AddToViewModelRecursive(SoftwareCell cell, IOCellViewModel previous = null)
        {
            var cellvm = IOCellViewModel.Create(cell);
            SoftwareCells.Add(cellvm);
            if (previous != null)
            {
                Connections.Add(new ConnectionViewModel(previous, cellvm) {Name = cell.InputStreams.First().DataNames});
            }

            var destinations = cell.OutputStreams.SelectMany(stream => stream.Destinations).ToList();
            destinations.ForEach(x => AddToViewModelRecursive(x, cellvm));
        }




        public static MainViewModel Instance()
        {
            return self ?? (self = new MainViewModel());
        }


        public void RemoveConnection(ConnectionViewModel connectionViewModel)
        {
            Connections.Remove(connectionViewModel);
        }
    }
}