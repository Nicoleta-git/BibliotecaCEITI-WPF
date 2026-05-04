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
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateBook updateControl = new UpdateBook();
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.ChangeView(updateControl);
            }
        }

        private void StergeBtn_Click(object sender, RoutedEventArgs e)
        {
            Delete delete = new Delete();
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow) {
                mainWindow.ChangeView(delete);
            }
        }
    }
}
