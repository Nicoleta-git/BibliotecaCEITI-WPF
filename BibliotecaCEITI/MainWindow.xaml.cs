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

        private DispatcherTimer timer;

        public MainWindow()
        {
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
        private void Student_Click(object sender, RoutedEventArgs e)
        {
            Students students = new Students();
            MainContentContainer.Content = students;
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
            data_curenta.Text = DateTime.Now.ToString("dd MMMM yyyy");
            ora_actuala.Text = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}