using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            Interactions.SetViewModel((MainViewModel) DataContext);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var myWindow = GetWindow(this);
            var transform = myWindow?.TransformToVisual(myCanvas);
            var myUiElementPosition = transform.Transform(border.BeforeContextMenuPoint);

            Interactions.AddNewIOCell(myUiElementPosition);
        }


        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("UIElement_OnMouseUp");
            Keyboard.ClearFocus();

            Interactions.DeselectAll();
            
        }
    }



}

