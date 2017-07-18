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
        private bool _isSelected;
        private AdornerLayer aLayer;
        private UIElement selectedElement;
        private ICommand _mouseDownCommand;

        #region Variablendeklarierung

        public SketchRectangle Rectangle
        {
            get { return _rectangle; }
            set
            {
                _rectangle = value;
                OnPropertyChanged("Rectangle");
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RemoveNotUsedAdorners();
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion Variablen

        void RemoveNotUsedAdorners()
        {
            if(IsSelected == false && selectedElement != null && (aLayer.GetAdorners(selectedElement) != null))
                aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
        }

        private void MouseDown(object e)
        {
            var eventArgs = e as MouseEventArgs;
            selectedElement = eventArgs.Source as UIElement;

            aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
            aLayer.Add(new ResizingAdorner(selectedElement));
        }

        public ICommand MouseDownCommand
        {
            get
            {
                if (_mouseDownCommand == null)
                {
                    _mouseDownCommand = new CommandBase(MouseDown, null);
                }
                return _mouseDownCommand;
            }
        }
    }
}
