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
            LoadColorSchema(MainWindow.SyntaxColortheme);


            

            TheDefinitionTextBox.TextChanged += (sender, args) =>
            {
                var caret = TheDefinitionTextBox.SelectionStart;
                var currentText = TheDefinitionTextBox.Document.Text;

                sender.TryGetDataContext<DataTypeViewModel>(vm => vm.UpdateModel(currentText));
                Interactions.UpdateMissingDataTypesCounter(((DataTypeViewModel)DataContext).MainModel);
                TheDefinitionTextBox.SelectionStart = caret;
            };
        }


              
       

        #region load color schema
       
        private static readonly HighlightingManager Man = new HighlightingManager();

        private void LoadColorSchema(string url)
        {
            if (MainWindow.Xshd == null)
            {
                if (!File.Exists(url))
                {
                    return;
                }
                using (var reader = new XmlTextReader(url))
                {
                    MainWindow.Xshd = HighlightingLoader.LoadXshd(reader);
                }
            }

            TheDefinitionTextBox.SyntaxHighlighting = HighlightingLoader.Load(MainWindow.Xshd, Man);
        }

        #endregion

        private void DeleteDataTypeDefinition(object sender, RoutedEventArgs e)
        {
            sender.TryGetDataContext<DataTypeViewModel>(vm => Interactions.DeleteDataTypeDefinition(((MainViewModel)DataContext), vm.Model));

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
            Interactions.AddDataTypeDefinition(((MainViewModel)DataContext));          
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
