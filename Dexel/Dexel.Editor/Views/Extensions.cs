using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dexel.Editor.Views
{
    public static class Extensions
    {
        public static void TryGetDataContext<T>(this object element, Action<T> doAction)
        {
            try
            {
                var frameworkelement = (FrameworkElement)element;
                var vm = (T)frameworkelement.DataContext;
                doAction(vm);
            }
            catch
            {
                // ignored
            }
        }
    }
}
