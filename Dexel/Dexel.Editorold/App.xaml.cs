using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dexel.Editor.Commands;
using Dexel.Editor.ViewModels;
using Dexel.Editor.Views;
using Dexel.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dexel.Editor
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();


        public static void Main()
        {
            Thread thread = new Thread(RunApp);
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join(); //Wait for the thread to end
            
        }


        public static void RunApp()
        {
            var newapp = new App();
          
            newapp.Run();
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AllocConsole();

            var mockMainModel = Mockdata.MakeRandomPerson2();
            var mainviewModel = MainViewModel.Instance();
            mainviewModel.LoadFromModel(mockMainModel);

            var mainwindow = new MainWindow(mainviewModel);          
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
