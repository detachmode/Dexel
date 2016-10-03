using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SharpFlowDesign.CustomControls;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign.Views
{
    /// <summary>
    /// Interaktionslogik für SoftwareCell.xaml
    /// </summary>
    public partial class SoftwareCell
    {
        public List<LineGeometry> EndLines { get; private set; }
        public List<LineGeometry> StartLines { get; private set; }

        public SoftwareCell()
        {
            StartLines = new List<LineGeometry>();
            EndLines = new List<LineGeometry>();
            InitializeComponent();

        }

        private void SoftwareCell_Drop(object sender, DragEventArgs e)
        {
            var obj = e.Data.GetData(e.Data.GetFormats()[0]);
            if (obj is Pointer)
            {
                var pointer = (obj as Pointer);
                var datacontext = (ConnectionViewModel)pointer.DataContext;
                var droppedContext = (IOCellViewModel)((Rectangle)sender).DataContext;
                pointer.SetBinding((DependencyProperty)Pointer.EndProperty, "End.InputPoint");
                datacontext.End = droppedContext;

            }
            UndoDragOverFeedback(e);

        }


        private SolidColorBrush hoverfeedbackBrush = new SolidColorBrush(Colors.LimeGreen);

        private void softwareCell_DragOver(object sender, DragEventArgs e)
        {
            var rect = (e.Source as Rectangle);
            if (rect == null) return;
            hoverfeedbackBrush.Opacity = 0.3;
            rect.Fill = hoverfeedbackBrush;

            SetPointerToMousePosition(e);
        }

        private void SetPointerToMousePosition(DragEventArgs e)
        {
            Point p2 = e.GetPosition(this);
            var obj = e.Data.GetData(e.Data.GetFormats()[0]);
            if (!(obj is Pointer)) return;

            var pointer = (obj as Pointer);
            pointer.End = p2;
        }




        private void SoftwareCell_DragLeave(object sender, DragEventArgs e)
        {
            UndoDragOverFeedback(e);
        }


        private static void UndoDragOverFeedback(DragEventArgs e)
        {
            var rect = (e.Source as Rectangle);
            if (rect == null) return;
            rect.Fill = Brushes.Transparent;
        }

        public void FocusTextBox()
        {
            TextBox textBox = (TextBox)TheThumb.Template.FindName("theTextBox", TheThumb);
            textBox.Focus();
        }

        public string LabelContent
        {
            get { return (string)GetValue(LabelContentProperty); }
            set { SetValue(LabelContentProperty, value); }
        }

        public static readonly DependencyProperty LabelContentProperty =
          DependencyProperty.Register("LabelContent", typeof(string), typeof(SoftwareCell));

        public SolidColorBrush SelectionColor
        {
            get { return (SolidColorBrush)GetValue(SelectionColorProperty); }
            set { SetValue(SelectionColorProperty, value); }
        }

        public static readonly DependencyProperty SelectionColorProperty =
          DependencyProperty.Register("SelectionColor", typeof(SolidColorBrush), typeof(SoftwareCell));

    }
}
