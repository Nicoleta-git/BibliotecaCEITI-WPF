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
using MySql.Data.MySqlClient;

namespace BibliotecaCEITI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Dashboard dashView = new Dashboard();
            MainContentContainer.Content = dashView;
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

        // ── Logout la închiderea ferestrei ───────────────────────────────
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!string.IsNullOrEmpty(TokenSesiune))
                EfectueazaLogout();

            base.OnClosing(e);
        }

        private void EfectueazaLogout()
        {
            try
            {
                using var conn = new MySqlConnection(CONNECTION_STRING);
                conn.Open();

                using var cmd = new MySqlCommand("sp_logout", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("p_token", TokenSesiune);

                var pCod = cmd.Parameters.Add("p_cod", MySqlDbType.Int32);
                var pMesaj = cmd.Parameters.Add("p_mesaj", MySqlDbType.VarChar, 255);
                pCod.Direction = System.Data.ParameterDirection.Output;
                pMesaj.Direction = System.Data.ParameterDirection.Output;

            MainContentContainer.Content = dashView;
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


    }
}