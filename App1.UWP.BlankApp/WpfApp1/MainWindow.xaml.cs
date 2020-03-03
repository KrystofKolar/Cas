using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<GfxItem.ItemViewable> items;
        public MainWindow()
        {
            InitializeComponent();

            items = GetItems();
            listBox1.ItemsSource = items;
            listBox1.DisplayMemberPath = "Name";
            listBox1.SelectedIndex = 0;
            listBox1.Focus();

            timer.Interval = new TimeSpan(10 * 1000 * 1); // 1ms in ticks(100 ns)
            timer.Tick += Timer_Tick;
        }

        private ObservableCollection<GfxItem.ItemViewable> GetItems()
        {
            ObservableCollection<GfxItem.ItemViewable> items = new ObservableCollection<GfxItem.ItemViewable> {

                      new GfxItem.ItemViewable { Id = 1, Name = "Eins", Visible = true },
                      new GfxItem.ItemViewable { Id = 2, Name = "Zwei", Visible = false },
                      new GfxItem.ItemViewable { Id = 3, Name = "Drei", Visible = true }
            };

            return items;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            //Hide();

            Load lw = new Load(this);
            lw.ShowDialog();

            //Close();
        }

        DispatcherTimer timer = new DispatcherTimer();

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }


        private void Button_Add(object sender, RoutedEventArgs e)
        {
            txtId.Text = "";
            txtName.Text = "";
            txtVisible.Text = "";

            txtId.Focus();

            Binding(false);
        }

        private void Button_Delete(object sender, RoutedEventArgs e)
        {
            int idx = listBox1.SelectedIndex;
            items.RemoveAt(idx);

            if (items.Count == 0) 
                bnDelete.IsEnabled = false;
            else 
                listBox1.SelectedIndex = 0;
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            GfxItem.ItemViewable item = new GfxItem.ItemViewable();

            int num;
            bool success = Int32.TryParse(txtId.Text, out num);

            if (success)
            {
                item.Id = num;
                item.Name = txtName.Text;
                item.Visible = txtVisible.Text.Length != 0 ? true : false;

                items.Add(item);

                Binding(true);
            };

            //todo some tooltip etc.
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            Binding(true);
            listBox1.SelectedIndex = 0;
        }

        private void Binding(bool create)
        {
            if (create)
            {
                Binding binding = new Binding("Id");
                txtId.SetBinding(TextBox.TextProperty, binding);

                binding = new Binding("Name");
                txtName.SetBinding(TextBox.TextProperty, binding);

                binding = new Binding("Visible");
                txtVisible.SetBinding(TextBox.TextProperty, binding);
            }
            else
            {
                BindingOperations.ClearBinding(txtId, TextBox.TextProperty);
                BindingOperations.ClearBinding(txtName, TextBox.TextProperty);
                BindingOperations.ClearBinding(txtVisible, TextBox.TextProperty);
            }
        }

        private void Button_ClickAppDomain(object sender, RoutedEventArgs e)
        {
            //TestFrame.MainWindow mw = new TestFrame.MainWindow();
            //mw.Show();

            Assembly asm = Assembly.GetEntryAssembly();
            string s = asm.FullName;
            if (asm.GlobalAssemblyCache)
            {
                int i = 0;
            }
            MethodInfo info = asm.EntryPoint;

            Module md = asm.ManifestModule;

            // cannot create another app in the same appdomain

            //MyLevelEditor.App app = new MyLevelEditor.App();
            //app.Run();

            // multiple appdomains, not supported in .NET Core

            //AppDomain domain = AppDomain.CreateDomain("Domain.Mg");
            //Type tMg = typeof(MyLevelEditor.App);
            //MyLevelEditor.App dapp = (MyLevelEditor.App)domain.CreateInstanceAndUnwrap(asm.FullName, tMg.FullName); 


            // Starting the library 
            MyLevelEditor.MainWindow mv = new MyLevelEditor.MainWindow();
            mv.ShowDialog();
        }

        private void Button_ClickOpenCanvas(object sender, RoutedEventArgs e)
        {
            //Hide();

            CanvasWindow cw = new CanvasWindow();
            cw.ShowDialog(); // modal

            //Show();
        }
    }
}
