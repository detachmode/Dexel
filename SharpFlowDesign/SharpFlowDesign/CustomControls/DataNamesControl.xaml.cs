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

namespace SharpFlowDesign.CustomControls
{
    /// <summary>
    /// Interaktionslogik für DataNamesControl.xaml
    /// </summary>
    public partial class DataNamesControl : UserControl
    {
        public DataNamesControl()
        {
            InitializeComponent();
        }


        public void SetFocus()
        {
            TextBox.Focus();
            TextBox.SelectionStart = TextBox.Text.Length;
            TextBox.SelectionLength = 0;
        }
    }
}
