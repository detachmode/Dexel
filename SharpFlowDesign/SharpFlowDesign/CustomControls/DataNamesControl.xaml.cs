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
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace SharpFlowDesign.CustomControls
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


            if (_xshd == null)
            {
               
                using (XmlTextReader reader = new XmlTextReader(@"FlowDesignColor.xshd"))
                {
                    _xshd = HighlightingLoader.LoadXshd(reader);
                }
            }
            TextBox.LostFocus += (sender, args) => TextBox.TextArea.ClearSelection();
            TextBox.SyntaxHighlighting = HighlightingLoader.Load(_xshd, Man);
            TextBox.TextChanged += (sender, args) =>
            {
              
            };

        }


        public void SetFocus()
        {
            TextBox.Focus();
            TextBox.SelectionStart = TextBox.Text.Length;
            TextBox.SelectionLength = 0;
        }
    }
}
