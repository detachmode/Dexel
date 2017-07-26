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
    /// Interaktionslogik für UiSketchCanvasView.xaml
    /// </summary>
    public partial class UiSketchCanvasView
    {

        private AdornerLayer aLayer;
        private UIElement selectedElement;
        private bool selected = false;

        public UiSketchCanvasView()
        {
            InitializeComponent();
        }

        private void CheckAdornerCondition()
        {
            if (selected)
            {
                selected = false;
                if (selectedElement != null)
                {
                    RemoveAdorner();
                }
            }
        }

        private void RemoveAdorner()
        {
            aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
            selectedElement = null;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(Canvas))
            {
                CheckAdornerCondition();
                var source = e.OriginalSource as Canvas;
                var dataContext = source.DataContext as MainUiSketchViewModel;
                dataContext.RemoveSelected();
            }

            if (e.OriginalSource.GetType() == typeof(Rectangle))
            {
                CheckAdornerCondition();


                selectedElement = e.OriginalSource as UIElement;

                aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
                aLayer.Add(new ResizingAdorner(selectedElement));
                selected = true;
            }
        }

        private void UiSketchCanvas_OnLoaded(object sender, RoutedEventArgs e)
        {
            PreviewMouseDown += OnMouseDown;
        }
    }
}
