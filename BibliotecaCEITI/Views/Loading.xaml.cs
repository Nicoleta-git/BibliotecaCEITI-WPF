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

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Window
    {
        public Loading()
        {
            InitializeComponent();
            StartLoading();
        }

        //ProgressBar increment -> provide a smooth, non-frozen visual update
        private async void StartLoading()
        {
            for (int i = 0; i <= 100; i++)
            {
                MyProgressBar.Value = i;

                if (i < 30) StatusLabel.Text = "Conectare la baza de date...";
                else if (i < 70) StatusLabel.Text = "Încărcare resurse...";
                else if (i < 90) StatusLabel.Text = "Finalizare...";
                else StatusLabel.Text = "Gata!";

                // Wait 50 milliseconds between each 1%
                await Task.Delay(50);
            }

            LoginWindow lw = new LoginWindow();
            lw.Show();
            this.Close();
        }


    }
}
