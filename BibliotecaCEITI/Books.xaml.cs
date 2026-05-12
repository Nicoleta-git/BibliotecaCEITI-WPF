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
using MySql.Data.MySqlClient;


namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for Books.xaml
    /// </summary>
    public partial class Books : UserControl
    {
        string connectionString = "Server=127.0.0.1;Port=3306;Database=biblioteca_ceiti_go;Uid=root;Pwd=Biblioteca2026!@#;";
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
                using (MySqlConnection conn = new MySqlConnection(this.connectionString))
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
                using (MySqlConnection conn = new MySqlConnection(this.connectionString))
                {
                    conn.Open();
                    string query = "CALL sp_raport_exemplare_manuale();";
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
                    using (MySqlConnection conn = new MySqlConnection(this.connectionString))
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
            Delete delete = new Delete();
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow) {
                mainWindow.ChangeView(delete);
            }
        }

        // Need to unify logic!
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            BookDetails updateControl = new BookDetails();
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.ChangeView(updateControl);
            }
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            BookDetails addBook = new BookDetails();
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow) {
                mainWindow.ChangeView(addBook);
            }
        }

        // 3. UPDATE (Modificare)
        private void UpdateBook(int id, string titlu)
        {
            using (var conn = new MySqlConnection(this.connectionString))
            {
                string query = "UPDATE carti SET titlu = @titlu WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@titlu", titlu);
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            //LoadData();
        }

        // 4. DELETE (Ștergere)
        private void DeleteBook(int id)
        {
            using (var conn = new MySqlConnection(this.connectionString))
            {
                string query = "DELETE FROM carti WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            //LoadData();
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string textCautat = SearchTextBox.Text;

            // 1. Anulăm căutarea precedentă dacă utilizatorul scrie repede
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            // 2. Creăm un nou token
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                // 3. AȘTEPTĂM 300 milisecunde. Dacă utilizatorul mai apasă o tastă, 
                // acest delay va fi anulat și o va lua de la capăt.
                await Task.Delay(300, token);

                // 4. ATENȚIE AICI: Trimitem textul la TOATE cele 3 argumente!
                // Altfel, va căuta textul doar în coloana 'titlu'.
                await SelectBooks_Title_Isbn_AuthorAsync(textCautat, textCautat, textCautat);
            }
            catch (TaskCanceledException)
            {
                // Ignorăm eroarea. Înseamnă doar că utilizatorul a tastat altă literă 
                // înainte să se termine cele 300ms.
            }
        }

        private int id_CarteSelectata, id_categorie_CarteSelectata, id_editura_CarteSelectata, id_locatie_CarteSelectata;

        private void BooksGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string[] coloaneDeAscuns = { "ID_CARTE", "Cod_Inventar", "Stare", "ID_categorie", "ID_locatie", "ID_editura" };

            if (coloaneDeAscuns.Contains(e.Column.Header.ToString()))
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
        }

        private string cod_Inventar_CarteSelectata, titlu_CarteSelectata, autor_CarteSelectata, isbn_CarteSelectata, stare_CarteSelectata;
        private double pret_CarteSelectata;

        private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BooksGrid.SelectedItem is DataRowView row)
            {
                try
                {
                    id_CarteSelectata = Convert.ToInt32(row["ID_CARTE"].ToString()); // hiden
                    id_categorie_CarteSelectata = Convert.ToInt32(row["ID_categorie"].ToString()); // hiden
                    id_editura_CarteSelectata = Convert.ToInt32(row["ID_locatie"].ToString()); // hiden
                    id_locatie_CarteSelectata = Convert.ToInt32(row["ID_editura"].ToString()); // hiden

                    cod_Inventar_CarteSelectata = row["Cod_Inventar"].ToString(); // hiden
                    titlu_CarteSelectata = row["Titlu"].ToString();
                    titlu_exemplar.Text = titlu_CarteSelectata;

                    autor_CarteSelectata = row["Autor"].ToString();
                    isbn_CarteSelectata = row["ISBN"].ToString();
                    stare_CarteSelectata = row["Stare"].ToString(); // hiden

                    pret_CarteSelectata = Convert.ToDouble(row["Pret"].ToString());

                    using (MySqlConnection conn = new MySqlConnection(this.connectionString))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("sp_date_categorie", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", id_categorie_CarteSelectata == 0 ? DBNull.Value : id_categorie_CarteSelectata);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            tip.Text = dt.Rows[0]["Tip"].ToString();
                        }
                        else
                        {
                            tip.Text = "Carte";
                        }
                    }

                    using (MySqlConnection conn = new MySqlConnection(this.connectionString))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("sp_editura_cartii", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", id_editura_CarteSelectata == 0 ? DBNull.Value : id_editura_CarteSelectata);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            editura.Text = dt.Rows[0]["Denumire"].ToString();
                        }
                        else
                        {
                            editura.Text = "Nespecificat";
                        }
                    }

                    using (MySqlConnection conn = new MySqlConnection(this.connectionString))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("sp_date_locatia_cartii", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", id_locatie_CarteSelectata == 0 ? DBNull.Value : id_locatie_CarteSelectata);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            locatie.Text = dt.Rows[0]["Sector"].ToString() + ", " + dt.Rows[0]["Sala"].ToString() + ", " + dt.Rows[0]["Raft"].ToString() + ", " + dt.Rows[0]["Polita"].ToString();
                        }
                        else
                        {
                            locatie.Text = "N/A";
                        }
                    }
                    using (MySqlConnection conn = new MySqlConnection(this.connectionString))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("sp_imagine_carte", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", id_CarteSelectata);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && reader["Imagine"] != DBNull.Value)
                            {
                                byte[] rawData = (byte[])reader["Imagine"];
                                imagine.Source = ToWpfImage(rawData);
                            }
                            else
                            {
                                imagine.Source = null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Eroare detalii: " + ex.Message);
                }
            }
        }

        private BitmapImage ToWpfImage(byte[] array)
        {
            if (array == null || array.Length == 0) return null;

            var image = new BitmapImage();
            using (var ms = new System.IO.MemoryStream(array))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }


    }
}
