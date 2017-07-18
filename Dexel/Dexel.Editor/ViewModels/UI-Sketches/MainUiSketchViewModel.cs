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
using Dexel.Editor.FileIO;
using Dexel.Editor.Views;
using Dexel.Editor.Views.AdditionalWindows;
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
        #region Variables
        public static MainUiSketchViewModel _self;
        private ObservableCollection<SketchRectangleViewModel> _rectangles;
        private SketchRectangleViewModel _selected;
        private ICommand _selectedCommand;

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
        #endregion

        public MainUiSketchViewModel()
        {
            _rectangles =
                new ObservableCollection<SketchRectangleViewModel>()
                {
                    new SketchRectangleViewModel(SketchRectangleManager.Instance().GetRoot())
                };
        }

        private void AddNewRectangleToCollection()
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
                Selected.Rectangle.Interaction = openFileDialog.FileName;
        }

        private void TryDeleteRectangleFromCollection()
        {
            var selected = CheckForSelectedRectangle();
            var lastelement = CheckForLastElement();
            CheckResultandTakeDeleteAction(selected, lastelement);
            
        }

        private bool CheckForSelectedRectangle()
        {
            if (Selected == null)
            {
                string caption = "Error";
                string message = "Please choose an element first!";
                Popups.ShowMessagePopup(message, caption);
                return false;
            }
            return true;
        }

        private bool CheckForLastElement()
        {
            if (Rectangles.Count == 1)
            {
                string caption = "Error";
                string message = "There has to be at least one element";
                Popups.ShowMessagePopup(message, caption);
                return true;
            }
            return false;
        }

        private void CheckResultandTakeDeleteAction(bool selectedCheckResult, bool lastelementCheckResult)
        {
            if(selectedCheckResult == true && lastelementCheckResult == false)
                Rectangles.Remove(Selected);
        }

        private void SaveCurrentSketch()
        {
            var fileName = "";
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                fileName = saveFileDialog.FileName;
            }

            if (fileName != String.Empty)
            {
                var rectangles = new Collection<SketchRectangle>();
                foreach (var rectangleViewModel in Rectangles)
                {
                    rectangles.Add(rectangleViewModel.Rectangle);
                }
                UISketches_SaveLoad.SaveToFile(fileName, rectangles);
            }
            else
            {
                Popups.ShowMessagePopup("File not saved", "Action aborted");
            }
        }

        private void LoadSketchFromFile()
        {
            var fileName = "";
            var openFileDialog = new OpenFileDialog
            {
                Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                fileName = openFileDialog.FileName;
            }

            if (fileName != String.Empty)
            {
                var rectanglesCollection = UISketches_SaveLoad.LoadFromFile(fileName);
                var tempRectanglesObservableCollection = new ObservableCollection<SketchRectangleViewModel>();

                foreach (var rectangle in rectanglesCollection)
                {
                    tempRectanglesObservableCollection.Add(new SketchRectangleViewModel(rectangle));
                }
                Rectangles = tempRectanglesObservableCollection;
            }
        }

        #region ICommands
        public ICommand AddNewRectangleToCollectionCommand
        {
            get { return new DelegateCommand(AddNewRectangleToCollection); }
        }

        public ICommand TryDeleteRectangleFromCollectionCommand
        {
            get { return new DelegateCommand(TryDeleteRectangleFromCollection); }
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

        public ICommand SaveCurrentSketchCommand
        {
            get { return new DelegateCommand(SaveCurrentSketch); }
        }

        public ICommand LoadSketchFromFileCommand
        {
            get { return new DelegateCommand(LoadSketchFromFile); }
        }

        public ICommand AddInteractionToRectangleCommand
        {
            get { return new DelegateCommand(AddInteractionToRectangle); }
        }
        #endregion

    }
}
