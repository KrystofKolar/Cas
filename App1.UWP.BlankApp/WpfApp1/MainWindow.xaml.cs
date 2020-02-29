using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            listBox1.ItemsSource = GetItems();
            listBox1.DisplayMemberPath = "Name";
            listBox1.SelectedIndex = 0;
            listBox1.Focus();

            timer.Interval = new TimeSpan(10 * 1000 * 1); // 1ms in ticks(100 ns)
            timer.Tick += Timer_Tick;
        }

        private List<GfxItem.ItemViewable> GetItems()
        {
            List<GfxItem.ItemViewable> items = new List<GfxItem.ItemViewable> {

                      new GfxItem.ItemViewable { Id = 0, Name = "Eins", Visible = false },
                      new GfxItem.ItemViewable { Id = 1, Name = "Zwei", Visible = false },
                      new GfxItem.ItemViewable { Id = 2, Name = "Drei", Visible = false }
            };

            return items;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            Hide();

            Load lw = new Load(this);
            lw.ShowDialog();

            //Close();
        }

        DispatcherTimer timer = new DispatcherTimer();

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private void Button_ClickMgOpen(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Save view item changes
            int i = 0;


        }
    }
}
