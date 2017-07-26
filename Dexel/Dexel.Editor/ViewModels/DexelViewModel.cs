using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dexel.Editor.ViewModels.DataTypeEditor;
using Dexel.Editor.Views;
using Dexel.Model.Common;
using Dexel.Model.DataTypes;
using Dexel.Model.Mockdata;
using Microsoft.Win32;
using PropertyChanged;


namespace Dexel.Editor.ViewModels
{
    [ImplementPropertyChanged]
    public class DexelViewModel
    {
        public DexelViewModel()
        {
            Diagrams = new ObservableCollection<MainViewModel>();
            var vm1 = new MainViewModel { Model = Mockdata.StartMainModel() };
            Diagrams.Add(vm1);
            Interactions.ViewRedraw(Diagrams[0], Diagrams[0].Model);
            SelectedDiagram = Diagrams[0];
            App.LoadLastUsedTheme(this);
        }

        public ObservableCollection<MainViewModel> Diagrams { get; set; }
        public ObservableCollection<DataTypeViewModel> DataTypes { get; set; }
        public MainViewModel SelectedDiagram { get; set; }
        public ICommand AddNewDiagramCommand => new DelegateCommand(AddNewDiagram);
        public ICommand CloseCurrentDiagramCommand => new DelegateCommand(CloseCurrentDiagram);
        public ICommand LoadSingleDiagramCommand => new DelegateCommand(LoadSingleDiagram);
        private ICommand _saveAllOpenedElementsCommand;

        public ICommand SaveAllOpenedElementsCommand
        {
            get
            {
                if (_saveAllOpenedElementsCommand == null)
                {
                    _saveAllOpenedElementsCommand =
                        new CommandBase(extension => SaveAllOpenedElements((String)extension), null);
                }
                return _saveAllOpenedElementsCommand;
            }
        }

        private void AddNewDiagram()
        {
            var vm = new MainViewModel {Model = new MainModel()};
            Diagrams.Add(vm);
            SelectedDiagram = Diagrams[Diagrams.Count - 1];
        }

        private void CloseCurrentDiagram()
        {
            Diagrams.Remove(SelectedDiagram);
        }

        private void LoadSingleDiagram()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "YAML (*.yaml)|*.yaml|Json (*json)|*.json|XML (*.xml)|*.xml|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var mainModel = Interactions.LoadFromFile(new MainViewModel(), openFileDialog.FileName);
                var vm = new MainViewModel();
                vm.Model = mainModel;
                Diagrams.Add(vm);
                Interactions.ViewRedraw(Diagrams[Diagrams.Count -1], Diagrams[Diagrams.Count - 1].Model);
                SelectedDiagram = Diagrams[Diagrams.Count - 1];
            }
        }

        private void SaveAllOpenedElements(string extension)
        {
            var saveFileDialog = new TAlex.WPF.CommonDialogs.FolderBrowserDialog();

            if (saveFileDialog.ShowDialog() == true)
            {
                var directory = saveFileDialog.SelectedPath;
                SaveAllOpenedDiagrams(directory, extension);
                //Put other methods to save here.
            }
        }

        private void SaveAllOpenedDiagrams(string directory, string extension)
        {
            foreach (var mainViewModel in Diagrams)
            {
                var completeSavePath = directory + "\\diagrams\\" + mainViewModel.Model.Name + extension;
                Directory.CreateDirectory(directory + "\\diagrams\\");
                Interactions.SaveToFile(completeSavePath, mainViewModel.Model);
            }
        }

        public void ChangeTheme(string resourceDict, string syntaxHighlighting)
        {
            ResourceDictionary dict = new ResourceDictionary();

            dict.Source = new Uri(resourceDict, UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.RemoveAt(0);
            Application.Current.Resources.MergedDictionaries.Add(dict);

            DexelWindow.Xshd = null;
            DexelWindow.SyntaxColortheme = syntaxHighlighting;

            var currentmodel = SelectedDiagram.Model;
            SelectedDiagram.LoadFromModel(new MainModel());
            SelectedDiagram.LoadFromModel(currentmodel);
        }
    }
}

