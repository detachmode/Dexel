using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using Dexel.Editor.Views;
using Dexel.Editor.Views.Common;
using Dexel.Editor.Views.CustomControls;
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
            var srtest = new SketchRectangle
            {
                Id = Guid.NewGuid(),
                Name = "New Window",
                Height = 100,
                Width = 100,
                X = 100,
                Y = 100
            };
            Rectangles.Add(new SketchRectangleViewModel(srtest));
        }

        public void SetSelected(object sketchRectangle)
        {
            this.Selected = sketchRectangle as SketchRectangleViewModel;
        }

        private void AddInteractionToRectangle()
        {
            if (Selected == null)
                return;
            var openFileDialog = new OpenFileDialog
            {
                Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*"
            };
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
                    _selectedCommand = new CommandBase(rectangle => SetSelected(rectangle), null);
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
