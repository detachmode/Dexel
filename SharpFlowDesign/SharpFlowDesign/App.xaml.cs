using System.Runtime.InteropServices;
using System.Windows;
using SharpFlowDesign.DebuggingHelper;
using SharpFlowDesign.Model;

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

            var mainwindow = new MainWindow();
            mainwindow.Show();
        }
    }

}