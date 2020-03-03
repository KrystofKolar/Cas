using System;
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

    public class starter : MarshalByRefObject
    {
        [STAThread]
        public static void Main()
        {
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }

}
