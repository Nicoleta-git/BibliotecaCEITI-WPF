using System.Windows;

namespace BibliotecaCEITI
{
    public partial class SurpriseWindow : Window
    {
        public SurpriseWindow()
        {
            InitializeComponent();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}