using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SharpFlowDesign.Behavior
{
    public class MouseHoverFeedBack: Behavior<Shape>
    {

        private SolidColorBrush defaultColor;
        protected override void OnAttached()
        {
            base.OnAttached();
            defaultColor = (SolidColorBrush) AssociatedObject.Fill;
            AssociatedObject.MouseEnter += (sender, args) => AssociatedObject.Fill = Brushes.AliceBlue;
            AssociatedObject.MouseLeave += (sender, args) => AssociatedObject.Fill = defaultColor;
        }
    }
}
