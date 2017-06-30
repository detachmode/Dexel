using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dexel.Editor.Views;
using Dexel.Model.Common;
using Dexel.Model.Manager;
using Dexel.Model.DataTypes;
using Microsoft.Win32;

namespace Dexel.Editor.ViewModels.UI_Sketches
{
    public class MainUiSketchViewModel: ViewModelBase
    {
        public static MainUiSketchViewModel _self;
        private ObservableCollection<SketchRectangleViewModel> _rectangles;
        private ObservableCollection<SketchRectangleViewModel> _flattenedRectanglesCollection;
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
                UpdateFlattenedRectanglesCollection(Rectangles);
                OnPropertyChanged("Rectangles");
            }
        }

        public ObservableCollection<SketchRectangleViewModel> FlattenedRectanglesCollection
        {
            get { return _flattenedRectanglesCollection; }
            set
            {
                _flattenedRectanglesCollection = value;
                OnPropertyChanged("FlattenedRectanglesCollection");
            }
        }

        private void UpdateFlattenedRectanglesCollection(ObservableCollection<SketchRectangleViewModel> rectangleCollection)
        {
            FlattenedRectanglesCollection = Common.FlattenExtension.Flatten(rectangleCollection);
        }

        private void AddNewRectangleToNotFlattenedRectanglesCollection(SketchRectangleViewModel selectedSketchRectangleViewModel)
        {
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

        private void AddHierarchyElement()
        {
            if (Selected == null)
                return;
            AddNewRectangleToNotFlattenedRectanglesCollection(Selected);
            UpdateFlattenedRectanglesCollection(Rectangles);
            
        }

        private void SetSelected(object sketchRectangle)
        {
            this.Selected = sketchRectangle as SketchRectangleViewModel;
        }

        private void AddInteractionToRectangle()
        {
            if (Selected == null)
                return;
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                Selected.Interaction = openFileDialog.FileName;
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

        public ICommand AddInteractionToRectangleCommand
        {
            get { return new DelegateCommand(AddInteractionToRectangle); }
        }
    }
}
