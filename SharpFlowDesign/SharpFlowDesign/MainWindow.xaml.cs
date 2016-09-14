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
using System.Windows;
using SharpFlowDesign.ViewModels;
using FunctionUnit = SharpFlowDesign.UserControls.FunctionUnit;

namespace SharpFlowDesign
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Path path1;
        private Path path4;
        private Path path2;
        private Path path3;


        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            //ZoomViewbox.Width = 100;
            //ZoomViewbox.Height = 20;
        }


        public List<IOCell> GetSelection()
        {
            var result = new List<IOCell>();
            for (int i = 0; i < myCanvas.Items.Count; i++)
            {
                var cell = myCanvas.Items[i] as IOCell;
                if (cell != null)
                {
                    if (cell.isSelected)
                    {
                        result.Add(cell);
                    }

                }
            }
            return result;
        }

        private void btnNewAction_Click(object sender, RoutedEventArgs e)
        {

        }



        // This method updates all the starting and ending lines assigned for the given thumb 
        // according to the latest known thumb position on the canvas
        //private void UpdateLines(MyThumb thumb)
        //{
        //    double left = Canvas.GetLeft(thumb);
        //    double top = Canvas.GetTop(thumb);

        //    for (int i = 0; i < thumb.StartLines.Count; i++)
        //        thumb.StartLines[i].StartPoint = new Point(left + thumb.ActualWidth / 2, top + thumb.ActualHeight / 2);

        //    for (int i = 0; i < thumb.EndLines.Count; i++)
        //        thumb.EndLines[i].EndPoint = new Point(left + thumb.ActualWidth / 2, top + thumb.ActualHeight / 2);
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Move all the predefined thumbs to the front to be over the lines
            //Canvas.SetZIndex(myThumb1, 1);
            //Canvas.SetZIndex(myThumb2, 1);
            //Canvas.SetZIndex(myThumb3, 1);
            //Canvas.SetZIndex(myThumb4, 1);


            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Window1_PreviewMouseLeftButtonDown);
        }

        // Event handler for creating new thumb element by left mouse click
        // and visually connecting it to the myThumb2 element
        void Window1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isAddNew)
            {
                // Create new thumb object
                //MyThumb newThumb = new MyThumb();
                // Assign our custom template to it
                //newThumb.Template = this.Resources["template1"] as ControlTemplate;
                // Calling ApplyTemplate enables us to navigate the visual tree right now (important!)
                //newThumb.ApplyTemplate();
                // Add the "onDragDelta" event handler that is common to all objects
                //newThumb.DragDelta += new DragDeltaEventHandler(onDragDelta);
                // Put newly created thumb on the canvas
                //myCanvas.Children.Add(newThumb);

                // Access the image element of our custom template and assign it to the new image
                //Image img = (Image)newThumb.Template.FindName("tplImage", newThumb);
                //img.Source = new BitmapImage(new Uri("Images/gear_connection.png", UriKind.Relative));

                // Access the textblock element of template and change it too
                //TextBlock txt = (TextBlock)newThumb.Template.FindName("tplTextBlock", newThumb);
                //txt.Text = "System action";

                // Set the position of the object according to the mouse pointer                
                //Point position = e.GetPosition(this);
                //Canvas.SetLeft(newThumb, position.X);
                //Canvas.SetTop(newThumb, position.Y);
                //// Move our thumb to the front to be over the lines
                //Canvas.SetZIndex(newThumb, 1);
                //// Manually update the layout of the thumb (important!)
                //newThumb.UpdateLayout();

                // Create new path and put it on the canvas
                //Path newPath = new Path();
                //newPath.Stroke = Brushes.Black;
                //newPath.StrokeThickness = 1;
                //myCanvas.Children.Add(newPath);

                //// Create new line geometry element and assign the path to it
                //LineGeometry newLine = new LineGeometry();
                //newPath.Data = newLine;

                //// Predefined "myThumb2" element will host the starting point
                //myThumb2.StartLines.Add(newLine);
                //// Our new thumb will host the ending point of the line
                //newThumb.EndLines.Add(newLine);

                //// Update the layout of line geometry
                ////UpdateLines(newThumb);
                ////UpdateLines(myThumb2);

                //isAddNew = false;
                //Mouse.OverrideCursor = null;
                ////btnNewAction.IsEnabled = true;
                //e.Handled = true;
            }
        }

        public bool isAddNew { get; set; }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var myWindow = Window.GetWindow(this);
            var transform = myWindow.TransformToVisual(myCanvas);

            var myUiElementPosition = transform.Transform(border.BeforeContextMenuPoint);

            AddNewIOCell(myUiElementPosition);
        }

        private void AddNewIOCell(Point pos)
        {
            var iocell = new IOCell();
           var datacontext =  ((IOCellViewModel) iocell.DataContext);
            datacontext.Input = "neuer input";

            pos.X -= 100 ;
            pos.Y -= 20;

            datacontext.Position = new Point(pos.X,pos.Y);

            myCanvas.Items.Add(iocell);

            //var myWindow = Window.GetWindow(this);
            //myWindow.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //myWindow.Arrange(new Rect(0, 0, myWindow.ActualWidth, myWindow.ActualHeight));


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        // Event handler for enabling new thumb creation by left mouse button click
        //private void btnNewAction_Click(object sender, RoutedEventArgs e)
        //{
        //    isAddNew = true;
        //    Mouse.OverrideCursor = Cursors.SizeAll;
        //    //btnNewAction.IsEnabled = false;
        //}


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

