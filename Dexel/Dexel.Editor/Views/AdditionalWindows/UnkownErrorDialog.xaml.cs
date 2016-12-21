using System;
using System.Windows;

namespace Dexel.Editor.Views.AdditionalWindows
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
            DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            txtError.SelectAll();
            txtError.Focus();
        }

        public string Answer => txtError.Text;
    }
}
