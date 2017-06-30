using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Dexel.Editor.ViewModels.UI_Sketches;

namespace Dexel.Editor.Common
{
    public static class MyDebug
    {
        private static string _lastline = "";
        public static void WriteLineIfDifferent(string line)
        {
            if (_lastline == line) return;
            Debug.WriteLine(line);
            _lastline = line;
        }
    }

    public static class FlattenExtension
    {
        public static ObservableCollection<SketchRectangleViewModel> Flatten(
            this ObservableCollection<SketchRectangleViewModel> notFlattenedRectangleViewModelsCollection)
        {
           var flattenedRectangleIEnumerable = notFlattenedRectangleViewModelsCollection.SelectMany(c => c.Children.Flatten()).Concat(notFlattenedRectangleViewModelsCollection);
           return new ObservableCollection<SketchRectangleViewModel>(flattenedRectangleIEnumerable);
        }
    }

}
