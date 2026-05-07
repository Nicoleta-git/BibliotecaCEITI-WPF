using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
        string connectionString = "Server=127.0.0.1;Port=3306;Database=biblioteca_ceiti_go;Uid=root;Pwd=Mihail2026;";
        public BookDetails()
        {
            InitializeComponent();
            PopuleazaAutori();
            PopuleazaCategorii();
            PopuleazaEdituri();
            PopuleazaLimbi();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new Books();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Fișiere Imagine (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|Toate fișierele (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string caleFisier = openFileDialog.FileName;

                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(caleFisier, UriKind.Absolute);
                    bitmap.EndInit();
                    txtCopertaUrl.Text = caleFisier;

                    imgCoperta.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Eroare la încărcarea imaginii: " + ex.Message);
                }
            }
        }

        private void PopuleazaAutori()
        {
            string query = "CALL sp_autori();";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cbAutor.ItemsSource = dt.DefaultView;
                    cbAutor.DisplayMemberPath = "nume";
                    cbAutor.SelectedValuePath = "id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la popularea listei: " + ex.Message);
            }
        }

        private void PopuleazaCategorii()
        {
            string query = "CALL sp_categorii_carti();";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cbCategorie.ItemsSource = dt.DefaultView;
                    cbCategorie.DisplayMemberPath = "denumire";
                    cbCategorie.SelectedValuePath = "id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la popularea listei: " + ex.Message);
            }
        }

        private void PopuleazaEdituri()
        {
            string query = "CALL sp_edituri();";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cbEditura.ItemsSource = dt.DefaultView;
                    cbEditura.DisplayMemberPath = "denumire";
                    cbEditura.SelectedValuePath = "id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la popularea listei: " + ex.Message);
            }
        }

        private void PopuleazaLimbi()
        {
            string query = "CALL sp_limbi();";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cbLimba.ItemsSource = dt.DefaultView;
                    cbLimba.DisplayMemberPath = "denumire";
                    cbLimba.SelectedValuePath = "id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la popularea listei: " + ex.Message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new Books();
            }
        }

        private void btnSalveazaModificari_Click(object sender, RoutedEventArgs e)
        {
            AddBook();
        }

        private void AddBook()
        {
            using (var conn = new MySqlConnection(this.connectionString))
            {
                string query = "INSERT INTO carti (titlu, isbn) VALUES (@titlu, @isbn)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                //cmd.Parameters.AddWithValue("@titlu", titlu);
                //cmd.Parameters.AddWithValue("@isbn", isbn);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

    }
}
