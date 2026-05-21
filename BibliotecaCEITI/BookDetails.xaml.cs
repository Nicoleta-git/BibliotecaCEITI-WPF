using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
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
using System.Windows.Shapes;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for UpdateBook.xaml
    /// </summary>
    public partial class BookDetails : UserControl
    {
        private int _idCarte;
        private string functie = "salvare";
        public BookDetails(int idCarteSelectata = 0)
        {
            InitializeComponent();
            PopuleazaAutori();
            PopuleazaCategorii();
            PopuleazaEdituri();
            PopuleazaLimbi();

            _idCarte = idCarteSelectata;

            if (_idCarte > 0)
            {
                functie = "editare";
                action_book.Text = "Editare detalii carte";
                IncarcaDateEditare();
            } else
            {
                functie = "salvare";
                action_book.Text = "Salvare carte";
            }

        }

        private void IncarcaDateEditare()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    string query = "sp_date_carte_selectata";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", _idCarte);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtTitlu.Text = reader["Titlu"].ToString();
                                txtIsbn.Text = reader["ISBN"].ToString();
                                txtDescriere.Text = reader["Descriere"] != DBNull.Value ? reader["Descriere"].ToString() : "";
                                txtAnPublicare.Text = reader["An_Publicare"].ToString();
                                txtPretMdl.Text = reader["Pret_Vanzare"].ToString();
                                txtPretChirie.Text = reader["Pret_Chirie"].ToString();
                                cbAutor.SelectedValue = reader["Autor"] != DBNull.Value ? Convert.ToInt32(reader["Autor"]) : -1;
                                cbCategorie.SelectedValue = reader["Categorie"] != DBNull.Value ? Convert.ToInt32(reader["Categorie"]) : -1;
                                cbEditura.SelectedValue = reader["Editura"] != DBNull.Value ? Convert.ToInt32(reader["Editura"]) : -1;
                                cbLimba.SelectedValue = reader["Limba"] != DBNull.Value ? Convert.ToInt32(reader["Limba"]) : -1;
                            }
                            if (reader["Imagine"] != DBNull.Value)
                            {
                                byte[] copertaBytes = (byte[])reader["Imagine"];
                                using (MemoryStream ms = new MemoryStream(copertaBytes))
                                {
                                    BitmapImage bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.StreamSource = ms;
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.EndInit();
                                    bitmap.Freeze();
                                    imgCoperta.Source = bitmap;
                                }
                            }
                            else
                            {
                                imgCoperta.Source = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea detaliilor cărții: " + ex.Message);
            }
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
                MySqlConnection conn = DatabaseConfig.GetConnection();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<ComboItem> lista = new List<ComboItem>();

                foreach (DataRow rand in dt.Rows)
                {
                    ComboItem item = new ComboItem();
                    item.Id = Convert.ToInt32(rand["id"]);
                    item.Denumire = rand["nume"].ToString();
                    lista.Add(item);
                }

                cbAutor.ItemsSource = lista;
                cbAutor.DisplayMemberPath = "Denumire";
                cbAutor.SelectedValuePath = "Id";

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        private void PopuleazaCategorii()
        {
            string query = "CALL sp_categorii_carti();";

            try
            {
                MySqlConnection conn = DatabaseConfig.GetConnection();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<ComboItem> lista = new List<ComboItem>();

                foreach (DataRow rand in dt.Rows)
                {
                    ComboItem item = new ComboItem();
                    item.Id = Convert.ToInt32(rand["id"]);
                    item.Denumire = rand["denumire"].ToString();
                    lista.Add(item);
                }

                cbCategorie.ItemsSource = lista;
                cbCategorie.DisplayMemberPath = "Denumire";
                cbCategorie.SelectedValuePath = "Id";

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        private void PopuleazaEdituri()
        {
            string query = "CALL sp_edituri();";

            try
            {
                MySqlConnection conn = DatabaseConfig.GetConnection();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<ComboItem> lista = new List<ComboItem>();

                foreach (DataRow rand in dt.Rows)
                {
                    ComboItem item = new ComboItem();
                    item.Id = Convert.ToInt32(rand["id"]);
                    item.Denumire = rand["denumire"].ToString();
                    lista.Add(item);
                }

                cbEditura.ItemsSource = lista;
                cbEditura.DisplayMemberPath = "Denumire";
                cbEditura.SelectedValuePath = "Id";

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        private void PopuleazaLimbi()
        {
            string query = "CALL sp_limbi();";

            try
            {
                MySqlConnection conn = DatabaseConfig.GetConnection();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<ComboItem> lista = new List<ComboItem>();

                foreach (DataRow rand in dt.Rows)
                {
                    ComboItem item = new ComboItem();
                    item.Id = Convert.ToInt32(rand["id"]);
                    item.Denumire = rand["denumire"].ToString();
                    lista.Add(item);
                }

                cbLimba.ItemsSource = lista;
                cbLimba.DisplayMemberPath = "Denumire";
                cbLimba.SelectedValuePath = "Id";

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
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

        private async Task AddBookAsync(string titlu, string autor, string categorie, string descriere, string isbn, string editura, int anPublicare, string limba, double pretVanzare, double pretChirie, byte[]? copertaBytes, int idBibliotecar)
        {
            using (MySqlConnection conn = DatabaseConfig.GetConnection())
            using (var cmd = new MySqlCommand("sp_insert_carte", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@p_titlu", titlu);
                cmd.Parameters.AddWithValue("@p_autor", autor);
                cmd.Parameters.AddWithValue("@p_categorie", categorie);
                cmd.Parameters.AddWithValue("@p_descriere", (object?)descriere ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_isbn", (object?)isbn ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_editura", (object?)editura ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_an_publicare", anPublicare);
                cmd.Parameters.AddWithValue("@p_limba", limba);
                cmd.Parameters.AddWithValue("@p_pret_mdl", pretVanzare);
                cmd.Parameters.AddWithValue("@p_pret_chirie_mdl", pretChirie);

                var pBlob = cmd.Parameters.Add("@p_coperta", MySqlDbType.LongBlob);
                pBlob.Value = (object?)copertaBytes ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@p_creat_de", idBibliotecar);

                var pIdCarteNou = cmd.Parameters.Add("@p_id_carte_nou", MySqlDbType.UInt32);
                pIdCarteNou.Direction = System.Data.ParameterDirection.Output;

                var pCod = cmd.Parameters.Add("@p_cod", MySqlDbType.Int32);
                pCod.Direction = System.Data.ParameterDirection.Output;
                var pMsg = cmd.Parameters.Add("@p_mesaj", MySqlDbType.VarChar, 255);
                pMsg.Direction = System.Data.ParameterDirection.Output;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateBookAsync(int idCarte, string titlu, string autor, string categorie, string descriere, string isbn, string editura, int anPublicare, string limba, double pretVanzare, double pretChirie, byte[]? copertaBytes, int idBibliotecar)
        {
            using (MySqlConnection conn = DatabaseConfig.GetConnection())
            using (var cmd = new MySqlCommand("sp_update_carte", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@p_id", idCarte);
                cmd.Parameters.AddWithValue("@p_titlu", titlu);
                cmd.Parameters.AddWithValue("@p_autor", autor);
                cmd.Parameters.AddWithValue("@p_categorie", categorie);
                cmd.Parameters.AddWithValue("@p_descriere", (object?)descriere ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_isbn", (object?)isbn ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_editura", (object?)editura ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_an_publicare", anPublicare);
                cmd.Parameters.AddWithValue("@p_limba", limba);
                cmd.Parameters.AddWithValue("@p_pret_mdl", pretVanzare);
                cmd.Parameters.AddWithValue("@p_pret_chirie_mdl", pretChirie);

                var pBlob = cmd.Parameters.Add("@p_coperta", MySqlDbType.LongBlob);
                pBlob.Value = (object?)copertaBytes ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@p_modificat_de", idBibliotecar);

                var pCod = cmd.Parameters.Add("@p_cod", MySqlDbType.Int32);
                pCod.Direction = System.Data.ParameterDirection.Output;
                var pMsg = cmd.Parameters.Add("@p_mesaj", MySqlDbType.VarChar, 255);
                pMsg.Direction = System.Data.ParameterDirection.Output;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async void btnSalveazaModificari_Click(object sender, RoutedEventArgs e)
        {
            btnSalveazaModificari.IsEnabled = false;

            try
            {
                int idBibliotecarLogat = 1;
                byte[]? copertaBytes = ImageToBytes(imgCoperta);

                if (!int.TryParse(txtAnPublicare.Text, out int an))
                {
                    MessageBox.Show("Anul publicării este invalid.");
                    return;
                }

                if (!double.TryParse(txtPretMdl.Text, out double pret))
                {
                    MessageBox.Show("Prețul este invalid.");
                    return;
                }

                if (!double.TryParse(txtPretChirie.Text, out double chirie))
                {
                    MessageBox.Show("Prețul chiriei este invalid.");
                    return;
                }

                if (functie == "salvare")
                {
                    await AddBookAsync(txtTitlu.Text, cbAutor.Text, cbCategorie.Text, txtDescriere.Text, txtIsbn.Text, cbEditura.Text, an, cbLimba.Text, pret, chirie, copertaBytes, idBibliotecarLogat);
                }
                else if (functie == "editare")
                {
                    await UpdateBookAsync(_idCarte, txtTitlu.Text, cbAutor.Text, cbCategorie.Text, txtDescriere.Text, txtIsbn.Text, cbEditura.Text, an, cbLimba.Text, pret, chirie, copertaBytes, idBibliotecarLogat);
                }
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.MainContentContainer.Content = new Books();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare la salvare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnSalveazaModificari.IsEnabled = true;
            }
        }
        private byte[]? ImageToBytes(System.Windows.Controls.Image imgControl)
        {
            if (imgControl.Source == null)
                return null;

            var bitmapSource = imgControl.Source as System.Windows.Media.Imaging.BitmapSource;
            if (bitmapSource == null)
                return null;

            using (var ms = new System.IO.MemoryStream())
            {
                var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapSource));
                encoder.Save(ms);
                return ms.ToArray();
            }
        }

    }
}
