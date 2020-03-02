using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for CanvasWindow.xaml
    /// </summary>
    public partial class CanvasWindow : Window
    {

        public CanvasWindow()
        {
            InitializeComponent();
        }

        ~CanvasWindow()
        {
            Debug.WriteLine("ctor CanvasWindow");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Close?", 
                                                   "title close", 
                                                   MessageBoxButton.YesNo, 
                                                   MessageBoxImage.Question, 
                                                   MessageBoxResult.Yes);

            e.Cancel = res != MessageBoxResult.Yes;
        }
    }
}
