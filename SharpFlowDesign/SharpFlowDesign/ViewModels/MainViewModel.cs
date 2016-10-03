using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PropertyChanged;
using SharpFlowDesign.CustomControls;
using SharpFlowDesign.Model;
using SharpFlowDesign.XML;

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
        }

        private void Connections_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            var vm = (sender as ConnectionViewModel);

            // as long as an connection is dragged around don't change the Collections
            if (vm.IsDragging) return;

            MaybeRemoveConnection(vm);
        }

        private void MaybeRemoveConnection(ConnectionViewModel vm)
        {
            if (vm.End == null)
                this.Connections.Remove(vm);
        }


        public ObservableCollection<ConnectionViewModel> Connections { get; private set; }
        public ObservableCollection<IOCellViewModel> SoftwareCells { get; set; }
        public ConnectionViewModel TemporaryConnection { get; set; }


        public void AddToViewModelRecursive(SoftwareCell cell, IOCellViewModel previous = null)
        {
            var cellvm = IOCellViewModel.Create(cell);
            SoftwareCells.Add(cellvm);
            if (previous != null)
            {
                Connections.Add(new ConnectionViewModel(previous, cellvm));
            }
          
            var destinations =  cell.OutputStreams.SelectMany(stream => stream.Destinations).ToList();
            destinations.ForEach(x => AddToViewModelRecursive(x, previous:cellvm));
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //DragDrop.DoDragDrop((DependencyObject)e.Source, "Sample", DragDropEffects.Copy);
        }


        public static MainViewModel Instance()
        {
            if (self == null)
                self = new MainViewModel();
            return self;
        }


        public void RemoveConnection(ConnectionViewModel connectionViewModel)
        {
            Connections.Remove(connectionViewModel);
        }
    }

}
