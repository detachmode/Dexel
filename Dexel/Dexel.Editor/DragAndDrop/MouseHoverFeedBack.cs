using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Dexel.Editor.DragAndDrop
{
    public class MouseHoverFeedBack: Behavior<Grid>
    {

        private SolidColorBrush defaultColor;
        private SolidColorBrush hoverBrush;
        protected override void OnAttached()
        {
            base.OnAttached();

            defaultColor = (SolidColorBrush) AssociatedObject.Background;
            hoverBrush = new SolidColorBrush(Color.FromArgb(25, 255, 255, 255));

            
            AssociatedObject.MouseEnter += (sender, args) => AssociatedObject.Background = hoverBrush;
            AssociatedObject.MouseLeave += (sender, args) => AssociatedObject.Background = defaultColor;
        }
    }
}
