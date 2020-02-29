using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Load.xaml
    /// </summary>
    public partial class Load : Window
    {
        public Window Caller { get; private set; }

        public Load(Window w)
        {
            InitializeComponent();

            Caller = w;
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double i = MyRowdef.ActualHeight;
            lbName.FontSize = i * 0.75F;
            lbName.Content = i.ToString("00");

            bnImage.Height = i;
            string s = "A";
            int len = System.Text.ASCIIEncoding.Unicode.GetByteCount(s);
            // System.Text.ASCIIEncoding.ASCII.GetByteCount(s);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();

            MainWindow mw = (MainWindow)Caller;
            mw.ShowDialog();

            Close();

        }
    }
}
