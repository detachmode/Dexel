namespace SharpFlowDesign.Views
{

    /// <summary>
    ///     Interaktionslogik für Stream.xaml
    /// </summary>
    public partial class DangelingConnectionView
    {
        public DangelingConnectionView()
        {
            InitializeComponent();
           
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