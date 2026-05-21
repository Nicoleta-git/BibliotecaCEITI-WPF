using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for CheckInBorrowStep_2.xaml
    /// </summary>
    public partial class CheckInBorrowStep_2 : UserControl
    {
        private CancellationTokenSource _cancellationTokenSource;
        private int id_CarteSelectata;
        public CheckInBorrowStep_2()
        {
            InitializeComponent();
            SelectBooks();
        }

        private void SelectBooks()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    string query = "CALL sp_raport_exemplare_carti();";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    BooksGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
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
                        MySqlCommand cmd = new MySqlCommand("sp_raport_exemplare_carti_sortat", conn);
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

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string textCautat = SearchBox.Text;
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
                if (string.IsNullOrWhiteSpace(textCautat))
                {
                    SelectBooks();
                }
                else
                {
                    await SelectBooks_Title_Isbn_AuthorAsync(textCautat, textCautat);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        private void BooksGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string[] coloaneDeAscuns = { "Id_carte", "Id_exemplar", "Cod_Inventar", "ID_categorie", "ID_locatie", "ID_editura", "Stare" };

            if (coloaneDeAscuns.Contains(e.Column.Header.ToString()))
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
        }

        public event Action<int> IdSelected;
        private string titlu_carte, cod_inventar, autor, stare;

        private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BooksGrid.SelectedItem is DataRowView row)
            {
                try
                {
                    id_CarteSelectata = int.TryParse(row["Id_exemplar"].ToString(), out int idCarte) ? idCarte : 0;
                    titlu_carte = row["Titlu"].ToString();
                    cod_inventar = row["Cod_Inventar"].ToString();
                    autor = row["Autor"].ToString();

                    stare = row["Stare"].ToString().Trim().ToLower();
                    titlu_carte_selectata.Text = string.IsNullOrEmpty(titlu_carte) ? "Fără denumire" : titlu_carte;
                    autor_carte_selectata.Text = string.IsNullOrEmpty(autor) ? "N/A" : autor;

                    if (string.IsNullOrEmpty(cod_inventar))
                    {
                        cod_inventar_carte_selectata.Text = "Cod: 0000";
                    }
                    else
                    {
                        cod_inventar_carte_selectata.Text = "Cod: " + cod_inventar;
                    }

                    if (string.IsNullOrEmpty(stare))
                    {
                        disponibilitate_carte_selectata.Text = "Nespecificat";
                        disponibilitate_carte_selectata.Foreground = Brushes.Orange;
                        fon_disponibilitate.Background = new SolidColorBrush(Color.FromArgb(30, 255, 165, 0));
                    }
                    else if (stare == "imprumutat" || stare == "împrumutat")
                    {
                        disponibilitate_carte_selectata.Text = "Împrumutat";
                        disponibilitate_carte_selectata.Foreground = Brushes.Red;
                        fon_disponibilitate.Background = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0));
                    }
                    else if (stare == "disponibil")
                    {
                        disponibilitate_carte_selectata.Text = "Disponibilă";
                        disponibilitate_carte_selectata.Foreground = Brushes.Green;
                        fon_disponibilitate.Background = new SolidColorBrush(Color.FromArgb(30, 34, 197, 94));
                    }
                    else
                    {
                        disponibilitate_carte_selectata.Text = stare;
                        disponibilitate_carte_selectata.Foreground = Brushes.Gray;
                        fon_disponibilitate.Background = new SolidColorBrush(Color.FromArgb(30, 128, 128, 128));
                    }

                    IdSelected?.Invoke(idCarte);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Eroare la citirea rândului: " + ex.Message, "Eroare DataGrid", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }
    }
}
