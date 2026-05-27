using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    public partial class MainWindow : Window
    {
        public static string NumeUtilizator { get; set; }
        public static string Rol { get; set; }
        public static int IdBibliotecar { get; set; }
        public static string TokenSesiune { get; set; }

        private bool _isDarkTheme = false;

        private DispatcherTimer timer;

        public MainWindow()
        {
            this.WindowState = WindowState.Maximized;
            InitializeComponent();
            Dashboard dashView = new Dashboard();
            MainContentContainer.Content = dashView;
            InitTimer();
        }

        // ── Constructor nou pentru login cu sesiune ──────────────────────
        public MainWindow(string nume, string rol, int id, string token) : this()
        {
            // Salvează datele sesiunii în proprietăți statice
            // (accesibile din orice UserControl cu MainWindow.Rol etc.)
            NumeUtilizator = nume;
            Rol = rol;
            IdBibliotecar = id;
            TokenSesiune = token;

            // Poți afișa numele utilizatorului în interfață
            // Decomentează linia de jos dacă ai un TextBlock numit txtNume în XAML
            // txtNume.Text = $"Bun venit, {nume}!";
        }
        private void EfectueazaLogout()
        {
            // Exemplu: curățare sesiune
            TokenSesiune = null;
            NumeUtilizator = null;
            Rol = null;
            IdBibliotecar = 0;
        }

        // ── Logout la închiderea ferestrei ───────────────────────────────
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!string.IsNullOrEmpty(TokenSesiune))
                EfectueazaLogout();

            base.OnClosing(e);
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
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

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
           Settings settings = new Settings();
            MainContentContainer.Content = settings;
        }
        private void ReserveBook_Click(object sender, RoutedEventArgs e)
        {
            BookReservation reservation = new BookReservation();
            MainContentContainer.Content = reservation;
        }
        private void BtnAtention_Click(object sender, RoutedEventArgs e)
        {
            Attention attention = new Attention();
            MainContentContainer.Content = attention;
        }
        private void BtnDeconectare_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();

            Window.GetWindow(this)?.Close();
        }
        private void Student_Click(object sender, RoutedEventArgs e)
        {
            Students students = new Students();
            MainContentContainer.Content = students;
        }

        private void ThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;

            string targetThemePath = _isDarkTheme ? "DarkTheme.xaml" : "LightTheme.xaml";

            try
            {
                ResourceDictionary loadedTheme = new ResourceDictionary
                {
                    Source = new Uri(targetThemePath, UriKind.RelativeOrAbsolute)
                };

                Application.Current.Resources.MergedDictionaries[0] = loadedTheme;

                if (_isDarkTheme)
                {
                    ThemeBtn.Background = (Brush)new BrushConverter().ConvertFromString("#F59E0B");
                    ThemeIcon.Foreground = Brushes.White;
                    ThemeIcon.Icon = FontAwesome5.EFontAwesomeIcon.Solid_CloudSun;
                    ThemeIcon.Width = 18;
                }
                else
                {
                    ThemeBtn.Background = (Brush)new BrushConverter().ConvertFromString("#4F46E5");
                    ThemeIcon.Foreground = Brushes.White;
                    ThemeIcon.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Moon;
                    ThemeIcon.Width = 14;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply runtime theme layout: {ex.Message}", "Theme Switcher Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void InitTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            string zi = DateTime.Now.Day.ToString();
            string luna = DateTime.Now.Month.ToString();
            if (luna == "1")
            {
                luna = "Ianuarie";
            } else if (luna == "2")
            {
                luna = "Februarie";
            } else if (luna == "3")
            {
                luna = "Martie";
            } else if (luna == "4")
            {
                luna = "Aprilie";
            } else if (luna == "5")
            {
                luna = "Mai";
            } else if (luna == "6")
            {
                luna = "Iunie";
            } else if (luna == "7")
            {
                luna = "Iulie";
            } else if (luna == "8")
            {
                luna = "August";
            } else if (luna == "9")
            {
                luna = "Septembrie";
            } else if (luna == "10")
            {
                luna = "Octombrie";
            } else if (luna == "11")
            {
                luna = "Noiembrie";
            } else if (luna == "12")
            {
                luna = "Decembrie";
            }
            string an = DateTime.Now.Year.ToString();
            data_curenta.Text = zi + " " + luna + " " + an;
            ora_actuala.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void BtnStatistica_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}