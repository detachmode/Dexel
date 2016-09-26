using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SharpFlowDesign.Views
{
    /// <summary>
    /// Interaktionslogik für FunctionUnit.xaml
    /// </summary>
    public partial class FunctionUnit : UserControl
    {
        public List<LineGeometry> EndLines { get; private set; }
        public List<LineGeometry> StartLines { get; private set; }

        public FunctionUnit()
        {
            StartLines = new List<LineGeometry>();
            EndLines = new List<LineGeometry>();
            InitializeComponent();

        }



        public void FocusTextBox()
        {
            TextBox textBox = (TextBox)theThumb.Template.FindName("FUName", theThumb);
            textBox.Focus();
        }

        public string LabelContent
        {
            get { return (string)GetValue(LabelContentProperty); }
            set { SetValue(LabelContentProperty, value); }
        }

        public static readonly DependencyProperty LabelContentProperty =
          DependencyProperty.Register("LabelContent", typeof(string), typeof(FunctionUnit));

        public SolidColorBrush SelectionColor
        {
            get { return (SolidColorBrush)GetValue(SelectionColorProperty); }
            set { SetValue(SelectionColorProperty, value); }
        }

        public static readonly DependencyProperty SelectionColorProperty =
          DependencyProperty.Register("SelectionColor", typeof(SolidColorBrush), typeof(FunctionUnit));

    }
}
