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
    /// Interaction logic for Books.xaml
    /// </summary>
    public partial class Books : UserControl
    {
        public Books()
        {
            InitializeComponent();

            // Test - datagrid
            var dateTest = new List<object>
            {
                new { Cod = "001", Titlu = "Luceafarul", Autor = "Mihai Eminescu", Pret = 55.50 },
                new { Cod = "002", Titlu = "Ion", Autor = "Liviu Rebreanu", Pret = 40.00 },
                new { Cod = "003", Titlu = "Baltagul", Autor = "Mihail Sadoveanu", Pret = 35.25 },
                new { Cod = "004", Titlu = "Enigma Otiliei", Autor = "George Calinescu", Pret = 48.00 }
            };

            BooksGrid.ItemsSource = dateTest;
        }


        private void StergeBtn_Click(object sender, RoutedEventArgs e)
        {
            Delete delete = new Delete();
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow) {
                mainWindow.ChangeView(delete);
            }
        }

        // Need to unify logic!
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            BookDetails updateControl = new BookDetails();
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.ChangeView(updateControl);
            }
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            BookDetails addBook = new BookDetails();
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow) {
                mainWindow.ChangeView(addBook);
            }
        }
    }
}
