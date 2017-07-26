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
using System.Windows.Shapes;

namespace Dexel.Editor.Views.AdditionalWindows
{
    /// <summary>
    /// Interaktionslogik für RenameDiagramWindow.xaml
    /// </summary>
    public partial class RenameDiagramWindow : Window
    {
        public RenameDiagramWindow()
        {
            InitializeComponent();
        }

        public string NewDiagramName
        {
            get { return NewDiagramNameTextBox.Text; }
            set { NewDiagramNameTextBox.Text = value; }
        }

        private void ApplyChangeButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
