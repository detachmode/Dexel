using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dexel.Editor.Views
{
    /// <summary>
    /// Interaktionslogik für SoftwareCell.xaml
    /// </summary>
    public partial class SoftwareCell
    {
        public List<LineGeometry> EndLines { get; private set; }
        public List<LineGeometry> StartLines { get; private set; }

        public SoftwareCell()
        {
            StartLines = new List<LineGeometry>();
            EndLines = new List<LineGeometry>();
            InitializeComponent();

        }


        public string LabelContent
        {
            get { return (string)GetValue(LabelContentProperty); }
            set { SetValue(LabelContentProperty, value); }
        }

        public static readonly DependencyProperty LabelContentProperty =
          DependencyProperty.Register("LabelContent", typeof(string), typeof(SoftwareCell));

        public SolidColorBrush SelectionColor
        {
            get { return (SolidColorBrush)GetValue(SelectionColorProperty); }
            set { SetValue(SelectionColorProperty, value); }
        }

        public static readonly DependencyProperty SelectionColorProperty =
          DependencyProperty.Register("SelectionColor", typeof(SolidColorBrush), typeof(SoftwareCell));


        private void TheTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Select(0,0);
          
        }


        
    }
}
