using System;
using System.Windows.Controls;
using System.Windows.Input;
using Dexel.Editor.ViewModels;
using Dexel.Editor.ViewModels.DrawingBoard;

namespace Dexel.Editor.Views.UserControls.DrawingBoard
{

    /// <summary>
    ///     Interaktionslogik für Stream.xaml
    /// </summary>
    public partial class DangelingConnectionView
    {
        public DangelingConnectionView()
        {
            InitializeComponent();
            LayoutUpdated += OnLayoutUpdated;
        }


        private void OnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            if (ViewModel() == null) return;
            if (MainViewModel.Instance().LoadingModelFlag)
                return;

            ViewModel().Width = ActualWidth;
        }


        public DangelingConnectionViewModel ViewModel() => DataContext as DangelingConnectionViewModel;

        public void SetFocusDatanames() => TheDataNamesControl.SetFocus(keepPosition:false);

        private void ActionNameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Interactions.Validate(MainViewModel.Instance().Model);
        }


        private void TheDataNamesControl_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                ActionNameTextBox.Focus();
                ActionNameTextBox.SelectionStart = ActionNameTextBox.Text.Length;
                ActionNameTextBox.SelectionLength = 0;

            }
        }


        private void ActionNameTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                TheDataNamesControl.SetFocus(keepPosition:true);

            }
        }
    }

}