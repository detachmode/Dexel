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
using SharpFlowDesign.ViewModels;

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
            DataContext = new IOCellViewModel();
        }

        public void SetPostion(Point pt)
        {
            Canvas.SetLeft(this, pt.X);
            Canvas.SetTop(this, pt.Y);
        }

        // Event hanlder for dragging functionality support same to all thumbs
        private void onDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            MainWindow myWindow = (MainWindow)Window.GetWindow(this);

            var cells = myWindow.GetSelection();

            foreach (var cell in cells)
            {
                var pos = ((IOCellViewModel)cell.DataContext).Position;
                pos.X += e.HorizontalChange;
                pos.Y += e.VerticalChange;
                ((IOCellViewModel)cell.DataContext).Position = pos;

            }



            // Update lines's layouts
            //UpdateLines(thumb);
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isSelected = !isSelected;
            if (isSelected)
            {
                FU.SelectionColor = new SolidColorBrush(Colors.DodgerBlue);
            }
            else
            {
                FU.SelectionColor = new SolidColorBrush(Colors.Black);
            }


        }

    }
}
