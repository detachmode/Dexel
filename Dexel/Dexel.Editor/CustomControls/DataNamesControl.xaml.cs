using System.Windows.Controls;
using System.Xml;
using Dexel.Model;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Dexel.Editor.CustomControls
{
    /// <summary>
    /// Interaktionslogik für DataNamesControl.xaml
    /// </summary>
    public partial class DataNamesControl : UserControl
    {
        private static XshdSyntaxDefinition _xshd;
        private static readonly HighlightingManager Man = new HighlightingManager();

        public DataNamesControl()
        {
            InitializeComponent();

            LoadColorShema();
            TextBox.LostFocus += (sender, args) => TextBox.TextArea.ClearSelection();
           
            TextBox.TextChanged += (sender, args) =>
            {
                var caret =  TextBox.SelectionStart;
                var str = TextBox.Document.Text;
                DataContext.TryCast<DataStream>(datastream => datastream.DataNames = str );
                DataContext.TryCast<DataStreamDefinition>(dataStreamDefinition => dataStreamDefinition.DataNames = str);
                TextBox.SelectionStart = caret;
            };

        }


        private void LoadColorShema()
        {
            if (_xshd == null)
            {
                using (var reader = new XmlTextReader(@"FlowDesignColor.xshd"))
                {
                    _xshd = HighlightingLoader.LoadXshd(reader);
                }                            
            }

            TextBox.SyntaxHighlighting = HighlightingLoader.Load(_xshd, Man);
        }


        public void SetFocus()
        {
            TextBox.Focus();
            TextBox.SelectionStart = TextBox.Text.Length;
            TextBox.SelectionLength = 0;
        }
    }
}
