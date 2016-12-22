using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Dexel.Editor.ViewModels;
using Dexel.Editor.ViewModels.DataTypeEditor;
using Dexel.Editor.Views.Common;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Dexel.Editor.Views.UserControls.DataTypeEditor
{
    /// <summary>
    /// Interaction logic for DataTypeCard.xaml
    /// </summary>
    public partial class DataTypeCard : UserControl
    {
        public DataTypeCard()
        {

            InitializeComponent();
            LoadColorSchema(@"Views/Themes/FlowDesignColor.xshd");


            

            TheDefinitionTextBox.TextChanged += (sender, args) =>
            {
                var caret = TheDefinitionTextBox.SelectionStart;
                var currentText = TheDefinitionTextBox.Document.Text;

                sender.TryGetDataContext<DataTypeViewModel>(vm => vm.UpdateModel(currentText));
                Interactions.UpdateMissingDataTypesCounter(MainViewModel.Instance().Model);
                TheDefinitionTextBox.SelectionStart = caret;
            };
        }


              
       

        #region load color schema
        private static XshdSyntaxDefinition _xshd;
        private static readonly HighlightingManager Man = new HighlightingManager();

        private void LoadColorSchema(string url)
        {
            if (_xshd == null)
            {
                if (!File.Exists(url))
                {
                    return;
                }
                using (var reader = new XmlTextReader(url))
                {
                    _xshd = HighlightingLoader.LoadXshd(reader);
                }
            }

            TheDefinitionTextBox.SyntaxHighlighting = HighlightingLoader.Load(_xshd, Man);
        }

        #endregion

        private void DeleteDataTypeDefinition(object sender, RoutedEventArgs e)
        {
            sender.TryGetDataContext<DataTypeViewModel>(vm => Interactions.DeleteDataTypeDefinition(vm.Model, MainViewModel.Instance().Model));

        }

        //public ItemsControl GetItemControlParent(object item)
        //{
        //    DependencyObject parent = VisualTreeHelper.GetParent(item);
        //    while (!(parent is TreeViewItem || parent is ItemsControl))
        //    {
        //        parent = VisualTreeHelper.GetParent(parent);
        //    }

        //    return parent as ItemsControl;
        //}

        private void AddNewDataTypeDefinition(object sender, RoutedEventArgs e)
        {
            Interactions.AddDataTypeDefinition(MainViewModel.Instance().Model);          
        }

        private void DataTypeCard_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Tab) return;

            e.Handled = true;

            if (TheNameTextBox.IsKeyboardFocused)
            {
                TheDefinitionTextBox.Focus();
                TheDefinitionTextBox.SelectionStart = TheDefinitionTextBox.Text.Length;
            }

            else
            {
                TheNameTextBox.Focus();
                TheNameTextBox.SelectionStart = TheNameTextBox.Text.Length;
            }
        }
    }
}
