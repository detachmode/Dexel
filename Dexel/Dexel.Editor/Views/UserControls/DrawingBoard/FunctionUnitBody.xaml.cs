using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dexel.Editor.Views.UserControls.DrawingBoard
{
    /// <summary>
    /// Interaktionslogik für FunctionUnit.xaml
    /// </summary>
    public partial class FunctionUnitBody
    {
        public List<LineGeometry> EndLines { get; private set; }
        public List<LineGeometry> StartLines { get; private set; }

        public FunctionUnitBody()
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
          DependencyProperty.Register("LabelContent", typeof(string), typeof(FunctionUnitBody));

        public SolidColorBrush SelectionColor
        {
            get { return (SolidColorBrush)GetValue(SelectionColorProperty); }
            set { SetValue(SelectionColorProperty, value); }
        }

        public static readonly DependencyProperty SelectionColorProperty =
          DependencyProperty.Register("SelectionColor", typeof(SolidColorBrush), typeof(FunctionUnitBody));


        private void TheTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Select(0,0);
          
        }


        
    }
}
