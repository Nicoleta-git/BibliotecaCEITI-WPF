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
        string connectionString = "Server=127.0.0.1;Port=3306;Database=biblioteca_ceiti_go;Uid=root;Pwd=Mihail2026;";
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
                        MySqlCommand cmd = new MySqlCommand("sp_raport_exemplare_manuale", conn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("p_titlu", string.IsNullOrWhiteSpace(titlu) ? DBNull.Value : titlu);
                        cmd.Parameters.AddWithValue("p_isbn", string.IsNullOrWhiteSpace(isbn) ? DBNull.Value : isbn);
                        cmd.Parameters.AddWithValue("p_autor", string.IsNullOrWhiteSpace(autor) ? DBNull.Value : autor);

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

        // 1. READ (Afișare în DataGrid)
        private void LoadData()
        {
            try
            {
                using (var conn = new MySqlConnection(this.connectionString))
                {
                    string query = "SELECT id, cod_inventar, titlu, isbn FROM carti";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    BooksGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading: " + ex.Message); }
        }

        // 2. CREATE (Inserare)
        private void AddBook(string titlu, string isbn)
        {
            using (var conn = new MySqlConnection(this.connectionString))
            {
                string query = "INSERT INTO carti (titlu, isbn) VALUES (@titlu, @isbn)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@titlu", titlu);
                cmd.Parameters.AddWithValue("@isbn", isbn);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            LoadData(); // Reîmprospătăm tabelul
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
            LoadData();
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
            LoadData();
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string textCautat = SearchTextBox.Text;
            await SelectBooks_Title_Isbn_AuthorAsync(textCautat);
        }
    }
}
