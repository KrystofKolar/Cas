using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace WPF.Monogame.App.NET472
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new MainWindow().Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }


    }

    [Serializable]
    public class starter : MarshalByRefObject
    {
        [STAThread]
        public static void Main()
        {
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }

        [STAThread]
        public static void Go()
        {
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }

        public static string GetName()
        {
            Type mw = typeof(WPF.Monogame.App.NET472.starter);
            string str = mw.Assembly.ManifestModule.Name;
            return str;
        }
    }

}
