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
    /// Interaction logic for UpdateBook.xaml
    /// </summary>
    public partial class BookDetails : UserControl
    {
        public BookDetails()
        {
            InitializeComponent();

            // Testing ComboBox functionality
            cbEditura.Items.Add("Editura Cartier");
            cbEditura.Items.Add("Editura Litera");
            cbEditura.Items.Add("Editura ARC");
            cbEditura.Items.Add("Editura Știința");

            cbEditura.SelectedIndex = 0;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new Books();
            }
        }
    }
}
