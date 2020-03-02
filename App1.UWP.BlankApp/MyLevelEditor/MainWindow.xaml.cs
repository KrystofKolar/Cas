using System.Windows;

namespace MyLevelEditor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowItemNew win = new WindowItemNew();
            win.ShowDialog();
        }
    }
}
