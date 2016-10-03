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
            //TextBox DatanamesTextBox = (TextBox)theThumb.Template.FindName("FUName", theThumb);
            DatanamesTextBox.Focus();
            DatanamesTextBox.SelectionStart = DatanamesTextBox.Text.Length; // add some logic if length is 0
            DatanamesTextBox.SelectionLength = 0;
        }
    }

}