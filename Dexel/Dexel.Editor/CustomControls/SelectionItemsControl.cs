using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Dexel.Library;

namespace Dexel.Editor.CustomControls
{

    public class SelectionItemsControl : ItemsControl
    {

 
        
    }

    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }

}