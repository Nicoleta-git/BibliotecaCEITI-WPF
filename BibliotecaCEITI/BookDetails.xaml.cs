using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                incarca_imagine.Text = "Încarcă altă imagine";
                IncarcaDateEditare();
            }
            else
            {
                functie = "salvare";
                action_book.Text = "Salvare carte";

                txtTitlu.Text = "Ex: Amintiri din copilărie...";
                txtTitlu.Foreground = new SolidColorBrush(Colors.Gray);
                txtIsbn.Text = "Ex: 978-973-46-1234-5...";
                txtIsbn.Foreground = new SolidColorBrush(Colors.Gray);
                txtAnPublicare.Text = "Ex: 2020...";
                txtAnPublicare.Foreground = new SolidColorBrush(Colors.Gray);
                txtPretMdl.Text = "Ex: 350...";
                txtPretMdl.Foreground = new SolidColorBrush(Colors.Gray);
                txtPretChirie.Text = "Ex: 100...";
                txtPretChirie.Foreground = new SolidColorBrush(Colors.Gray);
            }

            if (string.IsNullOrEmpty(txtDescriere.Text))
            {
                txtDescriere.Text = "Scrie o scurtă prezentare a cărții...";
                txtDescriere.Foreground = new SolidColorBrush(Colors.Gray);
            }

            if (imgCoperta.Source != null)
            {
                incarca_imagine.Text = "Încarcă altă imagine";
            }
            else
            {
                incarca_imagine.Text = "Încarcă o imagine";
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

            if (imgCoperta.Source != null)
            {
                incarca_imagine.Text = "Încarcă altă imagine";
            }
            else
            {
                incarca_imagine.Text = "Încarcă o imagine";
            }
        }

        private void PopuleazaAutori()
        {
            string query = "CALL sp_autori();";
            cbAutor.Items.Clear();
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            List<ComboItem> lista = new List<ComboItem>();
                            lista.Add(new ComboItem { Id = -1, Denumire = "Selectează autor..." });
                            foreach (DataRow rand in dt.Rows)
                            {
                                ComboItem item = new ComboItem
                                {
                                    Id = Convert.ToInt32(rand["id"]),
                                    Denumire = rand["nume"].ToString()
                                };
                                lista.Add(item);
                            }

                            cbAutor.ItemsSource = lista;
                            cbAutor.DisplayMemberPath = "Denumire";
                            cbAutor.SelectedValuePath = "Id";
                            cbAutor.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        private void PopuleazaCategorii()
        {
            string query = "CALL sp_categorii_carti();";
            cbCategorie.Items.Clear();
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            List<ComboItem> lista = new List<ComboItem>();
                            lista.Add(new ComboItem { Id = -1, Denumire = "Selectează categoria..." });
                            foreach (DataRow rand in dt.Rows)
                            {
                                ComboItem item = new ComboItem
                                {
                                    Id = Convert.ToInt32(rand["id"]),
                                    Denumire = rand["denumire"].ToString()
                                };
                                lista.Add(item);
                            }

                            cbCategorie.ItemsSource = lista;
                            cbCategorie.DisplayMemberPath = "Denumire";
                            cbCategorie.SelectedValuePath = "Id";
                            cbCategorie.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        private void PopuleazaEdituri()
        {
            string query = "CALL sp_edituri();";
            cbEditura.Items.Clear();
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            List<ComboItem> lista = new List<ComboItem>();
                            lista.Add(new ComboItem { Id = -1, Denumire = "Selectează editura..." });
                            foreach (DataRow rand in dt.Rows)
                            {
                                ComboItem item = new ComboItem
                                {
                                    Id = Convert.ToInt32(rand["id"]),
                                    Denumire = rand["denumire"].ToString()
                                };
                                lista.Add(item);
                            }

                            cbEditura.ItemsSource = lista;
                            cbEditura.DisplayMemberPath = "Denumire";
                            cbEditura.SelectedValuePath = "Id";
                            cbEditura.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        private void PopuleazaLimbi()
        {
            string query = "CALL sp_limbi();";
            cbLimba.Items.Clear();
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        conn.Open();
                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            List<ComboItem> lista = new List<ComboItem>();
                            lista.Add(new ComboItem { Id = -1, Denumire = "Selectează limba..." });
                            foreach (DataRow rand in dt.Rows)
                            {
                                ComboItem item = new ComboItem
                                {
                                    Id = Convert.ToInt32(rand["id"]),
                                    Denumire = rand["denumire"].ToString()
                                };
                                lista.Add(item);
                            }

                            cbLimba.ItemsSource = lista;
                            cbLimba.DisplayMemberPath = "Denumire";
                            cbLimba.SelectedValuePath = "Id";
                            cbLimba.SelectedIndex = 0;
                        }
                    }
                }
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

        private bool ValidateTextBox(TextBox textBox, string placeholder, string errorMessage)
        {
            if (textBox.Text == placeholder || string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show(errorMessage);
                textBox.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                return false;
            }
            textBox.ClearValue(TextBox.BorderBrushProperty);
            return true;
        }

        private bool ValidateComboBox(ComboBox comboBox, string errorMessage)
        {
            if (comboBox.SelectedIndex == 0)
            {
                MessageBox.Show(errorMessage);
                comboBox.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                return false;
            }
            comboBox.ClearValue(ComboBox.BorderBrushProperty);
            return true;
        }

        private bool ValidateNumber(TextBox textBox, string errorMessage, out int value)
        {
            if (!int.TryParse(textBox.Text, out value))
            {
                MessageBox.Show(errorMessage);
                textBox.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                return false;
            }
            textBox.ClearValue(TextBox.BorderBrushProperty);
            return true;
        }

        private bool ValidateDouble(TextBox textBox, string errorMessage, out double value)
        {
            if (!double.TryParse(textBox.Text, out value))
            {
                MessageBox.Show(errorMessage);
                textBox.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                return false;
            }
            textBox.ClearValue(TextBox.BorderBrushProperty);
            return true;
        }

        private async void btnSalveazaModificari_Click(object sender, RoutedEventArgs e)
        {
            btnSalveazaModificari.IsEnabled = false;

            try
            {
                int idBibliotecarLogat = 1;
                byte[]? copertaBytes = ImageToBytes(imgCoperta);

                if (!ValidateTextBox(txtTitlu, "Ex: Amintiri din copilărie...", "Vă rugăm să introduceți titlul cărții.")) return;
                if (!ValidateComboBox(cbAutor, "Vă rugăm să introduceți autorul cărții.")) return;
                if (!ValidateComboBox(cbCategorie, "Vă rugăm să alegeți categoria cărții.")) return;
                if (!ValidateTextBox(txtIsbn, "Ex: 978-973-46-1234-5...", "Vă rugăm să introduceți ISBN-ul cărții.")) return;
                if (!ValidateComboBox(cbEditura, "Vă rugăm să alegeți editura cărții.")) return;
                if (!ValidateNumber(txtAnPublicare, "Anul publicării este invalid.", out int an)) return;
                if (!ValidateComboBox(cbLimba, "Vă rugăm să alegeți limba cărții.")) return;
                if (!ValidateDouble(txtPretMdl, "Prețul este invalid.", out double pret)) return;
                if (!ValidateDouble(txtPretChirie, "Prețul chiriei este invalid.", out double chirie)) return;

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

        private void txtTitlu_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Ex: Amintiri din copilărie...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtTitlu_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Ex: Amintiri din copilărie...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void txtDescriere_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Scrie o scurtă prezentare a cărții...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtDescriere_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Scrie o scurtă prezentare a cărții...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void txtIsbn_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Ex: 978-973-46-1234-5...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtIsbn_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Ex: 978-973-46-1234-5...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void txtAnPublicare_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Ex: 2020...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtAnPublicare_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Ex: 2020...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void txtPretMdl_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Ex: 350...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtPretMdl_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Ex: 350...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void txtPretChirie_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Ex: 100...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtPretChirie_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Ex: 100...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private bool isbnFormatat = false;
        private void txtIsbn_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isbnFormatat) return;

            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (textBox.Text != "Ex: 978-973-46-1234-5...")
            {
                textBox.ClearValue(TextBox.BorderBrushProperty);
            }
            if (textBox.Text == "Ex: 978-973-46-1234-5...") return;
            string rawDigits = new string(textBox.Text.Where(char.IsDigit).ToArray());
            string separator = "";
            for (int i = 0; i < rawDigits.Length; i++)
            {
                // Adăugăm cratimă după a 3-a, a 6-a, a 9-a și a 13-a cifră
                if (i == 3 || i == 6 || i == 9 || i == 13)
                {
                    separator += "-";
                }
                separator += rawDigits[i];
            }

            isbnFormatat = true;
            textBox.Text = separator;
            textBox.CaretIndex = textBox.Text.Length;
            isbnFormatat = false;
        }

        private void txtIsbn_TextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtAnPublicare_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtPretMdl_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtPretChirie_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtTitlu_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtTitlu.Text != "Ex: Amintiri din copilărie..." && !string.IsNullOrWhiteSpace(txtTitlu.Text))
            {
                txtTitlu.ClearValue(TextBox.BorderBrushProperty);
            }
        }

        private void cbAutor_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        { 
            if (cbAutor.SelectedIndex > 0) cbAutor.ClearValue(Control.BorderBrushProperty); 
        }

        private void cbCategorie_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        { 
            if (cbCategorie.SelectedIndex > 0) cbCategorie.ClearValue(Control.BorderBrushProperty); 
        }

        private void cbEditura_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        { 
            if (cbEditura.SelectedIndex > 0) cbEditura.ClearValue(Control.BorderBrushProperty); 
        }

        private void cbLimba_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        { 
            if (cbLimba.SelectedIndex > 0) cbLimba.ClearValue(Control.BorderBrushProperty); 
        }

        private void txtAnPublicare_TextChanged(object sender, TextChangedEventArgs e) 
        { 
            if (int.TryParse(txtAnPublicare.Text, out _)) txtAnPublicare.ClearValue(Control.BorderBrushProperty); 
        }

        private void txtPretMdl_TextChanged(object sender, TextChangedEventArgs e) 
        { 
            if (double.TryParse(txtPretMdl.Text, out _)) txtPretMdl.ClearValue(Control.BorderBrushProperty); 
        }

        private void txtPretChirie_TextChanged(object sender, TextChangedEventArgs e) 
        { 
            if (double.TryParse(txtPretChirie.Text, out _)) txtPretChirie.ClearValue(Control.BorderBrushProperty); 
        }
    }
}

