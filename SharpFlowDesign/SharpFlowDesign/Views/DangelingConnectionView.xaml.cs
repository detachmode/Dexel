using System.Windows.Interactivity;

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

        public void SetFocus()
        {
            TheDataNamesControl.SetFocus();
        }


        //private void EnableDrag()
        //{
        //    Interaction.GetBehaviors(ThePath).Clear();
        //}
    }

}