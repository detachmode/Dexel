using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace SharpFlowDesign.UserControls
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
            this.DataContext = "Test";
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
         
        }
    }
}
