using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dexel.Editor.Behavior
{
    public class MouseHoverFeedBack: Behavior<Grid>
    {

        private SolidColorBrush defaultColor;
        protected override void OnAttached()
        {
            base.OnAttached();
            defaultColor = (SolidColorBrush) AssociatedObject.Background;
            AssociatedObject.MouseEnter += (sender, args) => AssociatedObject.Background = Brushes.AliceBlue;
            AssociatedObject.MouseLeave += (sender, args) => AssociatedObject.Background = defaultColor;
        }
    }
}
