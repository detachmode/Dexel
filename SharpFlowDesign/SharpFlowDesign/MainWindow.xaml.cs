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

        private void btnNewAction_Click(object sender, RoutedEventArgs e)
        {

        }

        // Event hanlder for dragging functionality support same to all thumbs
        private void onDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            UserControls.IOCell thumb = e.Source as UserControls.IOCell;

            double left = Canvas.GetLeft(thumb) + e.HorizontalChange;
            double top = Canvas.GetTop(thumb) + e.VerticalChange;

            Canvas.SetLeft(thumb, left);
            Canvas.SetTop(thumb, top);

            // Update lines's layouts
            //UpdateLines(thumb);
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
            Canvas.SetZIndex(myThumb1, 1);
            Canvas.SetZIndex(myThumb2, 1);
            Canvas.SetZIndex(myThumb3, 1);
            Canvas.SetZIndex(myThumb4, 1);

            #region Initialize paths for predefined thumbs
            path1 = new Path();
            path1.Stroke = Brushes.Black;
            path1.StrokeThickness = 1;

            path2 = new Path();
            path2.Stroke = Brushes.Blue;
            path2.StrokeThickness = 1;

            path3 = new Path();
            path3.Stroke = Brushes.Green;
            path3.StrokeThickness = 1;

            path4 = new Path();
            path4.Stroke = Brushes.Red;
            path4.StrokeThickness = 1;

            myCanvas.Children.Add(path1);
            myCanvas.Children.Add(path2);
            myCanvas.Children.Add(path3);
            myCanvas.Children.Add(path4);
            #endregion

            #region Initialize line geometry for predefined thumbs
            LineGeometry line1 = new LineGeometry();
            path1.Data = line1;

            LineGeometry line2 = new LineGeometry();
            path2.Data = line2;

            LineGeometry line3 = new LineGeometry();
            path3.Data = line3;

            LineGeometry line4 = new LineGeometry();
            path4.Data = line4;
            #endregion

            #region Setup connections for predefined thumbs
            //myThumb1.StartLines.Add(line1);
            //myThumb2.EndLines.Add(line1);

            //myThumb2.StartLines.Add(line2);
            //myThumb3.EndLines.Add(line2);

            //myThumb3.StartLines.Add(line3);
            //myThumb4.EndLines.Add(line3);

            //myThumb4.StartLines.Add(line4);
            //myThumb1.EndLines.Add(line4);
            #endregion

            #region Update lines' layouts
            //UpdateLines(myThumb1);
            //UpdateLines(myThumb2);
            //UpdateLines(myThumb3);
            //UpdateLines(myThumb4);
            #endregion

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

