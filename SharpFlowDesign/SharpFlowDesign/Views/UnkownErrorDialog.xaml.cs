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

namespace MyGUI.XAML
{
    /// <summary>
    /// Interaktionslogik für UnkownErrorDialog.xaml
    /// </summary>
    public partial class UnkownErrorDialog
    {

        public UnkownErrorDialog(string errormsg = "")
        {
            InitializeComponent();
            txtError.Text = errormsg;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtError.SelectAll();
            txtError.Focus();
        }

        public string Answer
        {
            get { return txtError.Text; }
        }

    }
}
