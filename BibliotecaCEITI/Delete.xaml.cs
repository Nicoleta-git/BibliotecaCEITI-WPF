using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for Sterge.xaml
    /// </summary>
    public partial class Delete : UserControl
    {
        private int _idCarte;
        private string motivul_stergerii = "Selectează motivul", observatie_stergere = "";
        public Delete(int idCarteSelectata = 0)
        {
            InitializeComponent();
            _idCarte = idCarteSelectata;
            IncarcaDateStergere();

            txtObservatii.Text = "Ex: Scrie aici explicația suplimentară...";
            txtObservatii.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new Books();
            }

        }

        private void btnCaseaza_Click(object sender, RoutedEventArgs e)
        {
            if (_idCarte <= 0)
            {
                MessageBox.Show("Vă rugăm să selectați o carte din listă pentru a o șterge.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            motivul_stergerii = cbMotiv.Text;
            observatie_stergere = txtObservatii.Text;
            if (string.IsNullOrWhiteSpace(motivul_stergerii) || motivul_stergerii == "Selectează motivul")
            {
                MessageBox.Show("Vă rugăm să selectați un motiv al ștergerii cărții.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DeleteBook();
            txtNumeCarte.Clear();
            cbMotiv.SelectedIndex = -1;
            txtObservatii.Clear();
            _idCarte = 0;

            MessageBox.Show("Cartea a fost ștearsă cu succes!", "Informație", MessageBoxButton.OK, MessageBoxImage.Information);
            BackBtn_Click(sender, e);
        }

        private void DeleteBook()
        {

            using (MySqlConnection conn = DatabaseConfig.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand("sp_delete_carte", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_id", _idCarte);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void txtObservatii_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Ex: Scrie aici explicația suplimentară...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtObservatii_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Ex: Scrie aici explicația suplimentară...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void IncarcaDateStergere()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    string titlu = "";
                    string autor = "";
                    int idAutor = 0;
                    using (MySqlCommand cmd = new MySqlCommand("sp_date_carte_selectata", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", _idCarte);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                titlu = reader.GetString(reader.GetOrdinal("Titlu"));
                                idAutor = reader.GetInt32(reader.GetOrdinal("Autor"));
                            }
                        }
                    }
                    using (MySqlCommand cmd = new MySqlCommand("sp_autor", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", idAutor);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                autor = reader.GetString(reader.GetOrdinal("Autor"));
                            }
                        }
                        txtNumeCarte.Text = titlu + " - " + autor;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea detaliilor cărții: " + ex.Message);
            }
        }

    }
}
