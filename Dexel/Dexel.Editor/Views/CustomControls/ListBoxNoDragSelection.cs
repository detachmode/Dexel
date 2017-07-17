using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Dexel.Editor.Views.CustomControls
{
    public class ListBoxNoDragSelection:ListBox
    {
        
       protected override DependencyObject GetContainerForItemOverride()
       {
                return new ListBoxExItem();
       }
       protected override bool IsItemItsOwnContainerOverride(object item)
       {
                return item is ListBoxExItem;
       }
       }
        public class ListBoxExItem : ListBoxItem
        {
            private Selector ParentSelector
            {
                get { return ItemsControl.ItemsControlFromItemContainer(this) as Selector; }
            }

            protected override void OnMouseEnter(MouseEventArgs e)
            {
            }
            protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
            {
                base.OnMouseLeftButtonDown(e);
                ParentSelector?.ReleaseMouseCapture();
            
        }
    }
}