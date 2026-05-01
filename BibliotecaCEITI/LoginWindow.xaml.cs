using System;
using System.Windows;
using System.Windows.Input;

namespace BibliotecaCEITI
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void ConectareBtn_Click(object sender, RoutedEventArgs e)
        {
            string utilizator = utlizatorTxt.Text;
            string parola = parolaTxt.Password;

            if (utilizator == "nicoleta" && parola == "1234")
            {
                MainWindow main = new MainWindow();
                main.Show();

                this.Close();
            }
            else
            {
                MessageBox.Show("Nume de utilizator sau parola incorecta!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);

                parolaTxt.Clear();
                parolaTxt.Focus();
            }
        }

        private void Registrare_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Registrare registrareWindow = new Registrare();
                registrareWindow.Show();

                this.Close();
            }
        }

        private void ExitBtn(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}