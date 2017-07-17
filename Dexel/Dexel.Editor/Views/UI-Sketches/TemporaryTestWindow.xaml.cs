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
using Dexel.Editor.ViewModels.UI_Sketches;

namespace Dexel.Editor.Views.UI_Sketches
{
    /// <summary>
    /// Interaktionslogik für TemporaryTestWindow.xaml
    /// </summary>
    public partial class TemporaryTestWindow : Window
    {
        public TemporaryTestWindow()
        {
            DataContext = new MainUiSketchViewModel();
            InitializeComponent();
        }
    }
}
