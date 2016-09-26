using System.Windows.Controls;

namespace SharpFlowDesign.Views
{

    /// <summary>
    ///     Interaktionslogik für Flow.xaml
    /// </summary>
    public partial class Flow : UserControl
    {
        public Flow()
        {
            InitializeComponent();
            DataContext = " ";
        }


        public void FocusTextBox()
        {
            //TextBox textBox = (TextBox)theThumb.Template.FindName("FUName", theThumb);
            textBox.Focus();
            textBox.SelectionStart = textBox.Text.Length; // add some logic if length is 0
            textBox.SelectionLength = 0;
        }
    }

}