using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Dexel.Editor.ViewModels;
using Dexel.Model.DataTypes;

namespace Dexel.Editor.Views
{
    /// <summary>
    /// Interaction logic for DataTypesEditor.xaml
    /// </summary>
    public partial class DataTypesEditor : UserControl
    {
        public DataTypesEditor()
        {
            InitializeComponent();
        }

        public void FocusDataType(DataType dataType)
        {
            MainViewModel.Instance().SelectedSoftwareCells.Clear();

            DataTypeCard frameworkelement = null;
            DataTypeViewModel viewmodel = null;

            for (var i = 0; i < DataTypeList.Items.Count; i++)
            {
                var c = (ContentPresenter)DataTypeList.ItemContainerGenerator.ContainerFromIndex(i);
                c.ApplyTemplate();

                frameworkelement = (DataTypeCard)c.ContentTemplate.FindName("TheDataTypeCard", c);
                if (frameworkelement == null) continue;
                viewmodel = (DataTypeViewModel)frameworkelement.DataContext;
                if (viewmodel.Model == dataType)
                    break;
            }

            if (viewmodel == null)
                return;


            Action a = () =>
            {
                frameworkelement.TheNameTextBox.Focus();
                frameworkelement.TheNameTextBox.SelectionStart = frameworkelement.TheNameTextBox.Text.Length;
            };
            frameworkelement.TheNameTextBox.Dispatcher.BeginInvoke(DispatcherPriority.Background, a);

        }

        private void AddNewDataTypeDefinition(object sender, RoutedEventArgs e)
        {
            var newDataType = Interactions.AddDataTypeDefinition(MainViewModel.Instance().Model);
            FocusDataType(newDataType);
        }
    }
}
