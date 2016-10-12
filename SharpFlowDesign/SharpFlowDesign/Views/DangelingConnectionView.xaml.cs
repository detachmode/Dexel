using System.Windows;
using System.Windows.Interactivity;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign.Views
{

    /// <summary>
    ///     Interaktionslogik für Stream.xaml
    /// </summary>
    public partial class DangelingConnectionView
    {
        public DangelingConnectionView()
        {
            InitializeComponent();
            
        }

        public void SetFocus()
        {
            TheDataNamesControl.SetFocus();
        }


        private void DeleteDataStreamDefinition(object sender, RoutedEventArgs e)
        {
            var vm =  (DangelingConnectionViewModel) DataContext;
            Interactions.DeleteDatastreamDefiniton(vm.Model, vm.Parent);
        }
    }

}