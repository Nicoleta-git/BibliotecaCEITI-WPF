using System.Windows;
using System.Windows.Controls;

namespace BibliotecaCEITI
{
    public partial class ManualBorrow : UserControl
    {
        public ManualBorrow()
        {
            InitializeComponent();
        }

        private void BtnInapoi_Click(object sender, RoutedEventArgs e) {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.ChangeView(new Borrow());
        }
        private void CmbSelecteaza_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        
        }
        private void CmbGrupa_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        
        }
        private void ChkBifeazaToti_Changed(object sender, RoutedEventArgs e) { 
        
        }
        private void BtnSalveaza_Click(object sender, RoutedEventArgs e) {
        
        }
    }
}