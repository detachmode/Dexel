using System.Windows.Controls;

namespace Dexel.Editor.Views.CustomControls
{

    public class SelectionItemsControl : ItemsControl
    {

 
        
    }

    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }

}