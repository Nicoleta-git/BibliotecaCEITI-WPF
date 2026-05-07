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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();

            Dashboard dashView = new Dashboard();

            MainContentContainer.Content = dashView;

<<<<<<< HEAD

=======
            // Initialize timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            //timer.Tick += Timer_Tick;
            timer.Start();
>>>>>>> 182d611c19026e72ab6370e065047b7d46f11961
        }

        

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DashBoardBtn_Click(object sender, RoutedEventArgs e)
        {
            Dashboard dashView = new Dashboard();

            MainContentContainer.Content = dashView;
        }

        private void CartiBtn_Click(object sender, RoutedEventArgs e)
        {
            Books b = new Books();
            MainContentContainer.Content = b;
        }

        public void ChangeView(UserControl newView)
        {
            MainContentContainer.Content = newView;
        }

        private void Imprumuturi_Btn(object sender, RoutedEventArgs e)
        {
            Borrow cb = new Borrow();
            MainContentContainer.Content = cb;
        }
    }
}
