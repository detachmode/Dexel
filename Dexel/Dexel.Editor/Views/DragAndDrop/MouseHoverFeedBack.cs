using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Dexel.Editor.Views.DragAndDrop
{
    public class MouseHoverFeedBack: Behavior<Grid>
    {

        private SolidColorBrush _defaultColor;
        private SolidColorBrush _hoverBrush;
        protected override void OnAttached()
        {
            base.OnAttached();

            _defaultColor = (SolidColorBrush) AssociatedObject.Background;
            _hoverBrush = new SolidColorBrush(Color.FromArgb(25, 255, 255, 255));

            
            AssociatedObject.MouseEnter += (sender, args) => AssociatedObject.Background = _hoverBrush;
            AssociatedObject.MouseLeave += (sender, args) => AssociatedObject.Background = _defaultColor;
        }
    }
}
