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
        public Load()
        {
            InitializeComponent();
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double i = MyRowdef.ActualHeight;
            tbName.FontSize = i * 0.75F;
            tbName.Text = i.ToString("#.##");
        }
    }
}
