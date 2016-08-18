using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpFlowDesign.UserControls;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            //ZoomViewbox.Width = 100;
            //ZoomViewbox.Height = 20;
        }

        private void btnNewAction_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void MainWindow_OnMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    UpdateViewBox((e.Delta > 0) ? 15 : -15);
        //}

        //private void UpdateViewBox(int newValue)
        //{
        //    if ((ZoomViewbox.Width >= 0) && ZoomViewbox.Height >= 0)
        //    {
        //        ZoomViewbox.Width += newValue;
        //        ZoomViewbox.Height += newValue;
        //    }
        }
    }

