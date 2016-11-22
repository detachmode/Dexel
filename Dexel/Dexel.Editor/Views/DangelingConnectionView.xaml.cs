using System;
using System.Windows;
using Dexel.Editor.ViewModels;

namespace Dexel.Editor.Views
{

    /// <summary>
    ///     Interaktionslogik für Stream.xaml
    /// </summary>
    public partial class DangelingConnectionView
    {
        public DangelingConnectionView()
        {
            InitializeComponent();
            LayoutUpdated += OnLayoutUpdated;
        }


        private void OnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            if (ViewModel() == null) return;
            ViewModel().Width = ActualWidth;
        }


        public DangelingConnectionViewModel ViewModel() => DataContext as DangelingConnectionViewModel;

        public void SetFocus() => TheDataNamesControl.SetFocus();


        private void DeleteDataStreamDefinition(object sender, RoutedEventArgs e)
        {
            var vm =  (DangelingConnectionViewModel) DataContext;
            Interactions.DeleteDatastreamDefiniton(vm.Model, vm.Parent);
        }
    }

}