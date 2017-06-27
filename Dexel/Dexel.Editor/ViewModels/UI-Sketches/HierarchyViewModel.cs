using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dexel.Model.Common;
using Dexel.Model.Manager;

namespace Dexel.Editor.ViewModels.UI_Sketches
{
    public class HierarchyViewModel:ViewModelBase
    {

        private static HierarchyViewModel _self;

        public static HierarchyViewModel Instance()
        {
            return _self ?? (_self = new HierarchyViewModel());
        }

        private HierarchyViewModel() { }

        private List<SketchRectangleViewModel> _root;
        private SketchRectangleViewModel _selected;
        private ICommand _selectedCommand;

        public List<SketchRectangleViewModel> Root
        {
            get
            {
                if (_root == null)
                {
                    _root = new List<SketchRectangleViewModel>();
                    _root.Add(new SketchRectangleViewModel(SketchRectangleManager.Instance().GetRoot()));
                }
                return _root;
            }
        }

        public SketchRectangleViewModel Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                _selected.IsSelected = true;
                OnPropertyChanged("Selected");
            }
        }

        private void SetSelected(object sketchRectangle)
        {
            this.Selected = sketchRectangle as SketchRectangleViewModel;
        }

        public ICommand SelectedCommand
        {
            get
            {
                if (_selectedCommand == null)
                {
                    _selectedCommand = new CommandBase(i => this.SetSelected(i), null);
                }
                return _selectedCommand;
            }
        }
    }
}
