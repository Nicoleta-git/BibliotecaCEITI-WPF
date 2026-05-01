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
    /// Interaction logic for Registrare.xaml
    /// </summary>
    public partial class Registrare : Window
    {
        public Registrare()
        {
            InitializeComponent();
        }

        private void ExitBtn(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
