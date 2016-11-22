using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views;
using Dexel.Model;

namespace Dexel.Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AllocConsole();

            var mockMainModel = Mockdata.StartMainModel();
            var mainviewModel = MainViewModel.Instance();
            mainviewModel.LoadFromModel(mockMainModel);

            var mainwindow = new MainWindow(mainviewModel);

            //var mainwindow = new TestWindow();
            mainwindow.Show();

            App.Current.DispatcherUnhandledException += AppOnDispatcherUnhandledException;
        }

        private void AppOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;
            var inputDialog = new UnkownErrorDialog(args.Exception.ToString());
            if (inputDialog.ShowDialog() == true)
                App.Current.Shutdown();
        }

    }
}
