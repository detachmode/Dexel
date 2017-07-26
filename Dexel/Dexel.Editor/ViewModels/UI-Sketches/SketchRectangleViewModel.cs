using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Dexel.Editor.Views.DragAndDrop;
using Dexel.Model.Common;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using System.Windows.Input;
using Dexel.Editor.Views.CustomControls;

namespace Dexel.Editor.ViewModels.UI_Sketches
{
    public class SketchRectangleViewModel:ViewModelBase
    {
        public SketchRectangleViewModel(SketchRectangle sr)
        {
            _rectangle = sr;
        }

        private SketchRectangle _rectangle;
        public SketchRectangle Rectangle
        {
            get { return _rectangle; }
            set
            {
                _rectangle = value;
                OnPropertyChanged("Rectangle");
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

    }
}
