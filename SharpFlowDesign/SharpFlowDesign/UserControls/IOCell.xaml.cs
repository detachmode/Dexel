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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharpFlowDesign.UserControls
{
    /// <summary>
    /// Interaction logic for IOCell.xaml
    /// </summary>
    public partial class IOCell : UserControl
    {

        public bool isSelected { get; set; }

        public IOCell()
        {
            InitializeComponent();
            
        }

        public void SetPostion(Point pt)
        {
            Canvas.SetLeft(this, pt.X);
            Canvas.SetTop(this, pt.Y);
        }

        // Event hanlder for dragging functionality support same to all thumbs
        private void onDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            MainWindow myWindow =  (MainWindow)Window.GetWindow(this);
            var cells = myWindow.GetSelection();
            //UserControls.IOCell thumb = e.Source as UserControls.IOCell;
            foreach (var cell in cells)
            {
                double left = Canvas.GetLeft(cell) + e.HorizontalChange;
                double top = Canvas.GetTop(cell) + e.VerticalChange;

                Canvas.SetLeft(cell, left);
                Canvas.SetTop(cell, top);

            }



            // Update lines's layouts
            //UpdateLines(thumb);
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isSelected = true;

           
            
            FU.SelectionColor = new SolidColorBrush(Colors.DodgerBlue);
        }

    }
}
 