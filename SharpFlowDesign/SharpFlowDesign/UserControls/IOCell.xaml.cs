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
            //UserControls.IOCell thumb = e.Source as UserControls.IOCell;

            double left = Canvas.GetLeft(this) + e.HorizontalChange;
            double top = Canvas.GetTop(this) + e.VerticalChange;

            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);

            // Update lines's layouts
            //UpdateLines(thumb);
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //SelectionColor = new SolidColorBrush(Colors.Red);
        }
    }
}
