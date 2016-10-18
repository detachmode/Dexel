using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dexel.Editor.Commands
{
     public static class Commands
     {
        public static event Action OnAddNewIOCell;
        public static readonly RoutedCommand AddNewIOCellCommand = new RoutedCommand();
    }
}
