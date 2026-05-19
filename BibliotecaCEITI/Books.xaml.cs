using MySql.Data.MySqlClient;
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
    /// Interaction logic for Books.xaml
    /// </summary>
    public partial class Books : UserControl
    {
        private CancellationTokenSource _cancellationTokenSource;
        private int id_CarteSelectata, id_categorie_CarteSelectata, id_editura_CarteSelectata, id_locatie_CarteSelectata;
        public Books()
        {
            InitializeComponent();
            //TestConnection();
            SelectBooks();
        }

        private void TestConnection()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    MessageBox.Show("Succes connection");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error MySQL: " + ex.Message + "\nError code: " + ex.Number);
            }
            catch (Exception ex)
            {
                MessageBox.Show("General error: " + ex.Message);
            }
        }

        private void SelectBooks()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    string query = "CALL sp_numar_exemplare_per_carte();";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    BooksGrid.ItemsSource = dt.DefaultView;
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private async Task SelectBooks_Title_Isbn_AuthorAsync(string titlu = null, string isbn = null, string autor = null)
        {
            try
            {
                DataTable dt = await Task.Run(() =>
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("sp_raport_exemplare_manuale_titlu_isbn_autori", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@p_titlu", MySqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(titlu) ? DBNull.Value : titlu;
                        cmd.Parameters.Add("@p_isbn", MySqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(isbn) ? DBNull.Value : isbn;
                        cmd.Parameters.Add("@p_autor", MySqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(autor) ? DBNull.Value : autor;

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable tempDt = new DataTable();
                        da.Fill(tempDt);
                        return tempDt;
                    }
                });

                BooksGrid.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Eroare filtrare: " + ex.Message);
            }
        }

        private void StergeBtn_Click(object sender, RoutedEventArgs e)
        {
            Delete delete = new Delete(id_CarteSelectata);
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow) {
                mainWindow.ChangeView(delete);
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (id_CarteSelectata <= 0)
            {
                MessageBox.Show("Vă rugăm să selectați o carte din listă pentru a o edita.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            BookDetails updateControl = new BookDetails(id_CarteSelectata);
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.ChangeView(updateControl);
            }
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            BookDetails addBook = new BookDetails(0);
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow) 
            {
                mainWindow.ChangeView(addBook);
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string textCautat = SearchTextBox.Text;
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                await Task.Delay(300, token);
                await SelectBooks_Title_Isbn_AuthorAsync(textCautat, textCautat, textCautat);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void BooksGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string[] coloaneDeAscuns = { "Id_carte", "Cod_Inventar", "Stare", "ID_categorie", "ID_locatie", "ID_editura" };

            if (coloaneDeAscuns.Contains(e.Column.Header.ToString()))
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
        }

        private void RefreshBooks_Click(object sender, RoutedEventArgs e)
        {
            SelectBooks();
        }

        private string cod_Inventar_CarteSelectata, titlu_CarteSelectata, autor_CarteSelectata, isbn_CarteSelectata, stare_CarteSelectata;
        private double pret_CarteSelectata;

        private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BooksGrid.SelectedItem is DataRowView row)
            {
                string colId = row.Row.Table.Columns.Contains("Id_Carte") ? "Id_Carte" : "Id_carte";
                id_CarteSelectata = int.TryParse(row[colId].ToString(), out int idCarte) ? idCarte : 0;

                if (id_CarteSelectata > 0)
                {
                    try
                    {
                        using (MySqlConnection conn = DatabaseConfig.GetConnection())
                        {
                            conn.Open();

                            string queryCarte = "SELECT id_categorie, id_editura FROM carti WHERE id = @idCarte";
                            using (MySqlCommand cmd = new MySqlCommand(queryCarte, conn))
                            {
                                cmd.Parameters.AddWithValue("@idCarte", id_CarteSelectata);
                                using (MySqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        id_categorie_CarteSelectata = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                        id_editura_CarteSelectata = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        id_categorie_CarteSelectata = 0;
                        id_editura_CarteSelectata = 0;
                    }
                }

                cod_Inventar_CarteSelectata = row.Row.Table.Columns.Contains("Cod_Inventar") ? row["Cod_Inventar"].ToString() : "";
                stare_CarteSelectata = row.Row.Table.Columns.Contains("Stare") ? row["Stare"].ToString() : "";

                titlu_CarteSelectata = row["Titlu"].ToString();
                titlu_exemplar.Text = titlu_CarteSelectata;
                autor_CarteSelectata = row["Autor"].ToString();
                isbn_CarteSelectata = row["ISBN"].ToString();
                pret_CarteSelectata = double.TryParse(row["Pret"].ToString(), out double pret) ? pret : 0;

                try
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("sp_date_categorie", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", id_categorie_CarteSelectata == 0 ? DBNull.Value : id_categorie_CarteSelectata);
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        tip.Text = dt.Rows.Count > 0 ? dt.Rows[0]["Tip"].ToString() : "Carte";
                    }
                }
                catch { tip.Text = "Carte"; }

                try
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("sp_editura_cartii", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", id_editura_CarteSelectata == 0 ? DBNull.Value : id_editura_CarteSelectata);
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        editura.Text = dt.Rows.Count > 0 ? dt.Rows[0]["Denumire"].ToString() : "Nespecificat";
                    }
                }
                catch { editura.Text = "Nespecificat"; }

                try
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_imagine_carte", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id", id_CarteSelectata);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read() && !reader.IsDBNull(0))
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
                catch { imgCoperta.Source = null; }
            }
        }


        private void BtnAdaugaExemplar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn == null) return;

                DataRowView row = btn.DataContext as DataRowView;
                if (row == null) return;

                string colId = row.Row.Table.Columns.Contains("Id_Carte") ? "Id_Carte" : "Id_carte";
                int idCarte = int.TryParse(row[colId].ToString(), out int idC) ? idC : 0;

                if (idCarte <= 0)
                {
                    MessageBox.Show("Nu s-a putut identifica ID-ul cărții selectate.", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string raspuns = Microsoft.VisualBasic.Interaction.InputBox("Câte exemplare doriți să adăugați pentru această carte?", "Adăugare Exemplare Multiple", "1");

                if (string.IsNullOrWhiteSpace(raspuns)) return;

                if (!int.TryParse(raspuns, out int numarExemplare) || numarExemplare <= 0)
                {
                    MessageBox.Show("Vă rugăm să introduceți un număr întreg valid și mai mare decât 0.", "Validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int idLocatieImplicit = 1;

                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();

                    for (int i = 0; i < numarExemplare; i++)
                    {
                        using (MySqlCommand cmd = new MySqlCommand("sp_adaugă_exemplar_carte", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id_carte", idCarte);
                            cmd.Parameters.AddWithValue("@p_id_locatie", idLocatieImplicit);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read()) { }
                            }
                        }
                    }

                    MessageBox.Show($"S-au adăugat cu succes {numarExemplare} exemplare pentru această carte!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                SelectBooks();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Eroare la adăugarea exemplarelor în baza de date: " + ex.Message, "Eroare MySQL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare generală: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
