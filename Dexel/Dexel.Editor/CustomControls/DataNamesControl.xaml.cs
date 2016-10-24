using System.IO;
using System.Windows.Controls;
using System.Xml;
using Dexel.Model;
using Dexel.Model.DataTypes;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Dexel.Editor.CustomControls
{
    /// <summary>
    /// Interaktionslogik für DataNamesControl.xaml
    /// </summary>
    public partial class DataNamesControl : UserControl
    {
        

        public DataNamesControl()
        {
            InitializeComponent();
            LoadColorSchema(@"FlowDesignColor.xshd");

            TextBox.LostFocus += (sender, args) => TextBox.TextArea.ClearSelection();
           
            TextBox.TextChanged += (sender, args) =>
            {
                var caret =  TextBox.SelectionStart;
                var currentText = TextBox.Document.Text;

                DataContext.TryCast<DataStream>(ds => Interactions.ChangeConnectionDatanames(ds, currentText));
                DataContext.TryCast<DataStreamDefinition>(dsd => dsd.DataNames = currentText);
                TextBox.SelectionStart = caret;
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

            TextBox.SyntaxHighlighting = HighlightingLoader.Load(_xshd, Man);
        }

        #endregion



        public void SetFocus()
        {
            TextBox.Focus();
            TextBox.SelectionStart = TextBox.Text.Length;
            TextBox.SelectionLength = 0;
        }
    }
}
