using System;
using System.Windows;

namespace Dexel.Editor.Views.Common
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
