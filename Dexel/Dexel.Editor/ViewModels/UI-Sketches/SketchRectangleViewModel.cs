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
        private String _interaction;
        private AdornerLayer aLayer;

        private ICommand _mouseMoveCommand;
        private ICommand _mouseDownCommand;
        private ICommand _secondMouseDownCommand;
        private ICommand _mouseUpCommand;
        private ICommand _mouseLeaveCommand;
        bool _isDown;
        bool _isDragging;
        private Point _startPoint;
        private double _originalLeft;
        private double _originalTop;

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
                OnPropertyChanged("IsSelected");
            }
        }

        public String Interaction
        {
            get { return _interaction; }
            set
            {
                _interaction = value;
                OnPropertyChanged("Interaction");
            }
        }
        #endregion Variablen

        private void MouseMove(object mousePos)
        {
            Point position = (Point)mousePos;

            if ((_isDragging == false) &&
                ((Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                 (Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                _isDragging = true;

            if (_isDragging)
            {
                if (_isDown)
                {
                    SketchRectangleManager.MoveRectangle(_rectangle,
                        position.X - (_startPoint.X - _originalLeft),
                        position.Y - (_startPoint.Y - _originalTop));
                }
            }
        }

        private void MouseDown(object e)
        {
            var eventArgs = e as MouseEventArgs;
            var selectedElement = eventArgs.Source as UIElement;

            aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
            aLayer.Add(new ResizingAdorner(selectedElement));

            if (_rectangle != null)
            {
                _isDown = true;
                _originalLeft = _rectangle.X;
                _originalTop = _rectangle.Y;
            }
        }

        private void MouseUp(object mousePos)
        {
            _isDown = false;
            _isDragging = false;
        }

        private void SecondMouseDown(object mousePos)
        {
            var position = (Point) mousePos;
            _startPoint = position;
            _isDown = true;
        }

        public ICommand MouseMoveCommand
        {
            get
            {
                if (_mouseMoveCommand == null)
                {
                    _mouseMoveCommand = new CommandBase(mousPos => MouseMove(mousPos), null);
                }
                return _mouseMoveCommand;
            }
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

        public ICommand MouseUpCommand
        {
            get
            {
                if (_mouseUpCommand == null)
                {
                    _mouseUpCommand = new CommandBase(MouseUp, null);
                }
                return _mouseUpCommand;
            }
        }

        public ICommand SecondMouseDownCommand
        {
            get
            {
                if (_secondMouseDownCommand == null)
                {
                    _secondMouseDownCommand = new CommandBase(SecondMouseDown, null);
                }
                return _secondMouseDownCommand;
            }
        }

        public ICommand MouseLeaveCommand
        {
            get
            {
                if (_mouseLeaveCommand == null)
                {
                    _mouseLeaveCommand = new CommandBase(MouseUp, null);
                }
                return _mouseLeaveCommand;
            }
        }
    }
}
