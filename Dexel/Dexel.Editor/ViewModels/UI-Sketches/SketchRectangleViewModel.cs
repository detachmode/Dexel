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
        
        public SketchRectangleViewModel(SketchRectangle sr)
        {
            _id = sr.Id;
            _x = sr.x;
            _y = sr.y;
            _height = sr.Height;
            _width = sr.Width;
            _name = sr.Name;
            _parentId = sr.ParentId;
        }

        private Guid _id;
        private int _x;
        private int _y;
        private int _height;
        private int _width;
        private Guid _parentId;
        private string _name;
        private ObservableCollection<SketchRectangleViewModel> _children = new ObservableCollection<SketchRectangleViewModel>();
        private bool _isSelected;

        #region Variablendeklarierung
        public Guid ID
        {
            get { return _id; }

        }

        public int X
        {
            get { return _x; }
            set
            {
                _x = value;
                OnPropertyChanged("X");
            }
        }

        public int Y
        {
            get { return _y; }
            set
            {
                _y = value;
                OnPropertyChanged("Y");
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

        public Guid ParentId
        {
            get { return _parentId; }
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
                return _children;
            }
            set
            {
                _children = value;
                OnPropertyChanged("Children");
            }
        }
        #endregion Variablen


    }
}
