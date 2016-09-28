using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Petzold.Media2D;
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
            Interactions.SetViewModel((MainViewModel)DataContext);

            
            //ConnectionArrow connectionArrow = new ConnectionArrow();

            //(DataContext as MainViewModel).Items.Add(new IOCellViewModel());
            //(DataContext as MainViewModel).Items.Add(new IOCellViewModel() { Position = new Point(400, 200) });
            //var cell1 = ((MainViewModel) DataContext).Items[0];
            //var cell2 = ((MainViewModel) DataContext).Items[1];
            //connectionCanvas.Connection.StartPoint = cell1.Position;
            //connectionCanvas.EndPoint = cell2.Position;
            //cell1.ArrowLinesStart.Add(connectionArrow);
            //cell2.ArrowLinesEnd.Add(connectionArrow);
            //connectionArrow.Arrow.Stroke = Brushes.Red;

            //connectionArrow.Arrow.StrokeThickness = 3;
            //connectionArrow.textBox.Text = "hello";
            
            //theCanvas.Children.Add(connectionArrow);
           
            //var line = new LineGeometry();
            //line.StartPoint = new Point(0, 0);
            //line.EndPoint = new Point(100, 100);
            //connectors.Children.Add(line);

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

