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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for BookReservation.xaml
    /// </summary>
    public partial class BookReservation : UserControl
    {
        private CancellationTokenSource _cancellationTokenSource;
        private int id_ElevSelectat = 0;
        public BookReservation()
        {
            InitializeComponent();
            SelectBooks();
            SelectStudents();

            SearchTextBox.Text = "Caută o carte...";
            SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            SearchStudentBox.Text = "Caută un elev...";
            SearchStudentBox.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private async void BtnSaveReservation_Click(object sender, RoutedEventArgs e)
        {
            int idBibliotecar = SesiuneBibliotecar.IdBibliotecarCurent;
            if (id_ExemplarSelectat == 0)
            {
                MessageBox.Show("Selectează mai întâi un exemplar din lista de cărți.", "Câmp lipsă", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (id_ElevSelectat == 0)
            {
                MessageBox.Show("Selectează mai întâi un elev din lista de elevi.", "Câmp lipsă", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnSaveReservation.IsEnabled = false;

            try
            {
                SalveazaRezervare(id_ElevSelectat, idCarte, idBibliotecar);
            }
            finally
            {
                BtnSaveReservation.IsEnabled = true;
            }
        }

        private void SalveazaRezervare(int idElev, int idCarte, int idBibliotecar)
        {
            try
            {
                int succes = 0;
                string mesaj = "";

                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_salveaza_rezervare", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id_elev", idElev);
                        cmd.Parameters.AddWithValue("@p_id_carte", idCarte);
                        cmd.Parameters.AddWithValue("@p_id_bibliotecar", idBibliotecar);

                        MySqlParameter paramSucces = new MySqlParameter("@p_succes", MySqlDbType.Byte);
                        paramSucces.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(paramSucces);

                        MySqlParameter paramMesaj = new MySqlParameter("@p_mesaj", MySqlDbType.VarChar, 255);
                        paramMesaj.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(paramMesaj);

                        cmd.ExecuteNonQuery();

                        succes = Convert.ToInt32(paramSucces.Value);
                        mesaj = paramMesaj.Value?.ToString() ?? "";
                    }
                }

                if (succes == 1)
                {
                    MessageBox.Show(mesaj, "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    ReloadWaitList(id_ExemplarSelectat);
                }
                else
                {
                    MessageBox.Show(mesaj, "Rezervare imposibilă", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la salvarea rezervării: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadWaitList(int idExemplar)
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_coada_asteptare_exemplar", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", idExemplar);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        WaitListGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Eroare reîncărcare coadă: " + ex.Message);
            }
        }

        private void SelectBooks()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    string query = "CALL sp_raport_carti_rezervari();";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    BooksDataGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MySqlCommand cmd = new MySqlCommand("sp_raport_exemplare_sortat_rezervari", conn);
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

                BooksDataGrid.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Eroare filtrare: " + ex.Message);
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await CautaCartiAsync(SearchTextBox.Text);
        }

        private async Task CautaCartiAsync(string textCautat)
        {
            textCautat = SearchTextBox.Text.Trim();
            if (textCautat == "Caută o carte...")
            {
                textCautat = "";
            }
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
                    await SelectBooks_Title_Isbn_AuthorAsync(textCautat, textCautat, textCautat);
                }
            }
            catch (TaskCanceledException)
            {
                
            }
        }

        private string titlu_carte, stare;
        private int id_ExemplarSelectat, idCarte;

        private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is DataRowView row)
            {
                try
                {
                    idCarte = int.TryParse(row["Id_carte"].ToString(), out int idC) ? idC : 0;
                    id_ExemplarSelectat = int.TryParse(row["Id_exemplar"].ToString(), out int idExemplar) ? idExemplar : 0;
                    titlu_carte = row["Titlu"].ToString();

                    stare = row["Stare"].ToString().Trim().ToLower();
                    TxtSelectedBook.Text = string.IsNullOrEmpty(titlu_carte) ? "Fără denumire" : titlu_carte;

                    try
                    {
                        using (MySqlConnection conn = DatabaseConfig.GetConnection())
                        {
                            conn.Open();
                            using (MySqlCommand cmd = new MySqlCommand("sp_exemplar_rezervat", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@p_id", idExemplar);

                                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                                DataTable dt = new DataTable();
                                da.Fill(dt);

                                CurrentLoanGrid.ItemsSource = dt.DefaultView;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Eroare la încărcarea datelor: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    try
                    {
                        using (MySqlConnection conn = DatabaseConfig.GetConnection())
                        {
                            conn.Open();
                            using (MySqlCommand cmd = new MySqlCommand("sp_coada_asteptare_exemplar", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@p_id", idExemplar);

                                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                                DataTable dt = new DataTable();
                                da.Fill(dt);

                                WaitListGrid.ItemsSource = dt.DefaultView;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Eroare la încărcarea datelor: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    imgCoperta.Source = UsefulFunction.GetImagineCarte(idExemplar);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Eroare la citirea rândului: " + ex.Message, "Eroare DataGrid", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SelectStudents()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    string query = "CALL sp_selecteaza_studenti();";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dt.Columns.Add("Elev", typeof(string), "Nume + ' ' + Prenume");

                    StudentsDataGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SearchStudentsAsync(string elev = null, string grupa = null)
        {
            try
            {
                DataTable dt = await Task.Run(() =>
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("sp_filtrare_studenti", conn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@p_elev", MySqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(elev) ? DBNull.Value : elev;
                        cmd.Parameters.Add("@p_grupa", MySqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(grupa) ? DBNull.Value : grupa;

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable tempDt = new DataTable();
                        da.Fill(tempDt);

                        tempDt.Columns.Add("Elev", typeof(string), "Nume + ' ' + Prenume");

                        return tempDt;
                    }
                });

                StudentsDataGrid.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare filtrare elevi: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchStudentBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string textCautat = SearchStudentBox.Text.Trim();
            if (textCautat == "Caută un elev...")
            {
                textCautat = "";
            }
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
                    SelectStudents();
                }
                else
                {
                    await SearchStudentsAsync(textCautat, textCautat);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        private void BooksGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string[] coloaneDeAscuns = { "Id_carte", "Id_exemplar", "Cod_Inventar", "ID_categorie", "ID_locatie", "ID_editura", };

            if (coloaneDeAscuns.Contains(e.Column.Header.ToString()))
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
        }

        private void WaitListGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string[] coloaneDeAscuns = { "Id_rezervare", "Id_elev", "Grupa", "Telefon", "Email", "Data_rezervare", "Stare" };

            if (coloaneDeAscuns.Contains(e.Column.Header.ToString()))
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
            if (e.PropertyType == typeof(DateTime) || e.PropertyType == typeof(DateTime?))
            {
                if (e.Column is DataGridTextColumn col)
                {
                    col.Binding.StringFormat = "dd.MM.yyyy";
                }
            }
        }

        private void CurrentLoanGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string[] coloaneDeAscuns = { "Id_elev", "Telefon"};

            if (coloaneDeAscuns.Contains(e.Column.Header.ToString()))
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
        }

        private void StudentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentsDataGrid.SelectedItem is DataRowView row)
            {
                try
                {
                    id_ElevSelectat = int.TryParse(row["ID_elev"].ToString(), out int idE) ? idE : 0;

                    string elev = row["Elev"].ToString();
                    TxtFinalStudent.Text = string.IsNullOrEmpty(elev) ? "Fără nume" : elev;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Eroare la citirea rândului: " + ex.Message, "Eroare DataGrid", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.GotFocus(sender, "Caută o carte...");
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.LostFocus(sender, "Caută o carte...");
        }

        private void SearchStudentBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.GotFocus(sender, "Caută un elev...");
        }

        private void SearchStudentBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.LostFocus(sender, "Caută un elev...");
        }

        private void StudentsGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string[] coloaneDeAscuns = { "ID_elev", "ID_grupa", "Activ", "Nume", "Prenume" };

            if (coloaneDeAscuns.Contains(e.Column.Header.ToString()))
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnSearchBook_Click(object sender, RoutedEventArgs e)
        {
            await CautaCartiAsync(SearchTextBox.Text);
        }
    }
}
