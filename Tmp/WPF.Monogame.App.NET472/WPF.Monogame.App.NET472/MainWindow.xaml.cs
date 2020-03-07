using System;
using System.Windows;

namespace WPF.Monogame.App.NET472
{
    [Serializable]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowGfx wnd = new WindowGfx();
            wnd.ShowDialog();
        }
    }
}
