using System;
using System.Windows;
using Dexel.Editor.ViewModels;
using Dexel.Editor.ViewModels.DrawingBoard;

namespace Dexel.Editor.Views.DrawingBoard
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

        public void SetFocus() => TheDataNamesControl.SetFocus();



    }

}