using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dexel.Model.Common;
using Dexel.Model.Manager;
using Dexel.Model.DataTypes;

namespace Dexel.Editor.ViewModels.UI_Sketches
{
    public class MainUiSketchViewModel: ViewModelBase
    {
        public static MainUiSketchViewModel _self;
        private ObservableCollection<SketchRectangleViewModel> _rectangles;
        private SketchRectangleViewModel _selected;
        private ICommand _selectedCommand;

        public MainUiSketchViewModel()
        {
            _rectangles =
                new ObservableCollection<SketchRectangleViewModel>()
                {
                    new SketchRectangleViewModel(SketchRectangleManager.Instance().GetRoot())
                };
        }

        public static MainUiSketchViewModel Instance() => _self ?? (_self = new MainUiSketchViewModel());

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

        public ObservableCollection<SketchRectangleViewModel> Rectangles
        {
            get { return _rectangles; }
            set
            {
                _rectangles = value;
                OnPropertyChanged("Rectangles");
            }
        }


        private void AddHierarchyElement()
        {
            if (Selected == null)
                return;
            var srtest = new SketchRectangle
            {
                Id = Guid.NewGuid(),
                Name = "New Window",
                ParentId = Selected.ID,
                Height = Selected.Height - 100,
                Width = Selected.Width - 100,
                x = Selected.X + 100,
                y = Selected.Y + 100
            };
            var test = new SketchRectangleViewModel(srtest);
            Selected.Children.Add(test);
        }

        private void SetSelected(object sketchRectangle)
        {
            this.Selected = sketchRectangle as SketchRectangleViewModel;
        }

        public ICommand AddHierarchyElementCommand
        {
            get { return new DelegateCommand(AddHierarchyElement); }
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
