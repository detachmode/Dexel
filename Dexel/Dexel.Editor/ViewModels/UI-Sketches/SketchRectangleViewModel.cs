using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dexel.Model.Common;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;

namespace Dexel.Editor.ViewModels.UI_Sketches
{
    public class SketchRectangleViewModel:ViewModelBase
    {
        #region Variablen
        public SketchRectangleViewModel(SketchRectangle sr)
        {
            _id = sr.Id;
            _position = sr.Position;
            _height = sr.Height;
            _width = sr.Width;
            _name = sr.Name;
            _parentId = sr.ParentId;
        }

        private int _id;
        private Point _position;
        private int _height;
        private int _width;
        private int _parentId;
        private string _name;
        private ObservableCollection<SketchRectangleViewModel> _children;
        private bool _isSelected;

        public int ID
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        public Point Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnPropertyChanged("Position");
            }
        }

        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged("Height");
            }
        }

        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged("Width");
            }
        }

        public int ParentId
        {
            get { return _parentId; }
            set
            {
                _parentId = value;
                OnPropertyChanged("ParentId");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
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


        public ObservableCollection<SketchRectangleViewModel> Children
        {
            get
            {
                if (_children == null)
                    return GetChildren();
                return _children;
            }
            set
            {
                _children = value;
                OnPropertyChanged("Children");
            }
        }
        #endregion

        public ObservableCollection<SketchRectangleViewModel> GetChildren()
        {
            _children = new ObservableCollection<SketchRectangleViewModel>();
            foreach (var rectangle in SketchRectangleManager.Instance().GetChildren(ID))
            {
                Children.Add(new SketchRectangleViewModel(rectangle));
            }
            return Children;
        }
    }
}
