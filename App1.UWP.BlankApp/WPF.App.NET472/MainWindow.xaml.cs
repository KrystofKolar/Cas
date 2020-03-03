using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF.App.NET472
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Assembly asm = Assembly.GetEntryAssembly();
            string s = asm.FullName;
            if (asm.GlobalAssemblyCache)
            {
                int i = 0;
            }
            MethodInfo info = asm.EntryPoint;

            Module md = asm.ManifestModule;


            // .NET Core Version problems were:

            // cannot create another app in the same appdomain
            //MyLevelEditor.App app = new MyLevelEditor.App();
            //app.Run();

            // multiple appdomains, not supported in .NET Core
            //AppDomain domain = AppDomain.CreateDomain("Domain.Mg");
            //Type tMg = typeof(MyLevelEditor.App);
            //MyLevelEditor.App dapp = (MyLevelEditor.App)domain.CreateInstanceAndUnwrap(asm.FullName, tMg.FullName); 


            // .NET problems are:
            // cannot create another app in the same appdomain
            //WPF.Monogame.App.NET472.App app = new Monogame.App.NET472.App();
            //    app.Run();

            //using mg = WPF.Monogame.App.NET472; //Version >= 8
            //try multiple app domains
            AppDomain domain = AppDomain.CreateDomain("Domain.Mg");

            //Type mw= typeof(WPF.Monogame.App.NET472.MainWindow);

            //WPF.Monogame.App.NET472.MainWindow instance = 
            //    (WPF.Monogame.App.NET472.MainWindow)
            //    domain.CreateInstanceAndUnwrap(asm.FullName, mw.FullName); 


            Type gfxType = typeof(WPF.Monogame.App.NET472.GfxItem);

            WPF.Monogame.App.NET472.GfxItem item =
                (WPF.Monogame.App.NET472.GfxItem)
                domain.CreateInstanceAndUnwrap(gfxType.Assembly.FullName, gfxType.FullName);

            var res = item.Calc();



            Type mw= typeof(WPF.Monogame.App.NET472.MainWindow);

            WPF.Monogame.App.NET472.MainWindow instance = 
                (WPF.Monogame.App.NET472.MainWindow)
                domain.CreateInstanceAndUnwrap(mw.Assembly.FullName, mw.FullName);
            instance.Show();
        }
    }
}
