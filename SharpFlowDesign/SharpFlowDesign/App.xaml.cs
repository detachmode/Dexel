using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using MyGUI.XAML;
using SharpFlowDesign.DebuggingHelper;

namespace SharpFlowDesign
{

    /// <summary>
    ///     Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();




        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            AllocConsole();

            //Mockdata.RomanNumbers();

            //DebugPrinter.PrintRecursive();

            var mainwindow = new Views.MainWindow();
            mainwindow.Show();
            App.Current.DispatcherUnhandledException += AppOnDispatcherUnhandledException;
        }

        private void AppOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;
            UnkownErrorDialog inputDialog = new UnkownErrorDialog(args.Exception.ToString());
            if (inputDialog.ShowDialog() == true)
                App.Current.Shutdown();
        }
    }

}