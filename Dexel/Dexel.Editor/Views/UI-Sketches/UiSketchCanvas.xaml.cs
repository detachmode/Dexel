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
using Dexel.Editor.Views.Common;
using Dexel.Editor.Views.CustomControls;


namespace Dexel.Editor.Views.UI_Sketches
{
    /// <summary>
    /// Interaktionslogik für UiSketchCanvas.xaml
    /// </summary>
    public partial class UiSketchCanvas
    {
        public UiSketchCanvas()
        {
            InitializeComponent();
            
        }


        private void UiSketchCanvas_OnLoaded(object sender, RoutedEventArgs e)
        {
            PreviewMouseDown += OnMouseDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
