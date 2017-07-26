using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dexel.Editor.Views.AdditionalWindows
{
    public static class Popups
    {
        public static void ShowMessagePopup(string message, string caption)
        {
            MessageBox.Show(message, caption);
            
        }
    }
}
