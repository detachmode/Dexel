using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Petzold.Media2D;
using SharpFlowDesign.Model;
using SharpFlowDesign.ViewModels;
using SharpFlowDesign.Views;

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
            var mainviewmodel = DataContext as MainViewModel;

            Interactions.SetViewModel((MainViewModel)DataContext);

            var splitter =  Mockdata.RomanNumbers();
            mainviewmodel.AddToViewModelRecursive(splitter);



        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var myWindow = GetWindow(this);
            var transform = myWindow?.TransformToVisual(itemContainer);
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

