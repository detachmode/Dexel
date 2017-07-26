using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Dexel.Editor.ViewModels;
using Dexel.Editor.ViewModels.DataTypeEditor;
using Dexel.Model.DataTypes;

namespace Dexel.Editor.Views.UserControls.DataTypeEditor
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

        public void FocusDataType(CustomDataType customDataType)
        {
            ((MainViewModel)DataContext).SelectedFunctionUnits.Clear();

            DataTypeCard frameworkelement = null;
            DataTypeViewModel viewmodel = null;

            for (var i = 0; i < DataTypeList.Items.Count; i++)
            {
                var c = (ContentPresenter)DataTypeList.ItemContainerGenerator.ContainerFromIndex(i);
                c.ApplyTemplate();

                frameworkelement = (DataTypeCard)c.ContentTemplate.FindName("TheDataTypeCard", c);
                if (frameworkelement == null) continue;
                viewmodel = (DataTypeViewModel)frameworkelement.DataContext;
                if (viewmodel.Model == customDataType)
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
            var newDataType = Interactions.AddDataTypeDefinition(((MainViewModel)DataContext));
            FocusDataType(newDataType);
        }


        private void CreateMissingTypes_click(object sender, RoutedEventArgs e)
        {
            Interactions.AddMissingDataTypes(((MainViewModel)DataContext));
        }
    }
}
