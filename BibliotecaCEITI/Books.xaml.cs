using FontAwesome5;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Pipelines;
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
        private int id_CarteSelectata = 0, id_categorie_CarteSelectata, id_editura_CarteSelectata, id_locatie_CarteSelectata;
        private bool modeArhiva = false, isGestionezExemplare = false;
        private int id_CarteExemplare = 0;

        public Books()
        {
            InitializeComponent();
            SelectBooks();

            SearchTextBox.Text = "Caută o carte...";
            SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
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
                MessageBox.Show("Error loading data: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectExemplare(int idCarte)
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_exemplare_per_carte", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", idCarte);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ExemplareGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectArhivedBooks()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    string query = "CALL sp_numar_exemplare_per_carte_arhivata();";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    BooksGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectExemplareArhivate(int idCarte)
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_exemplare_per_carte_arhivate", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", idCarte);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ExemplareGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task SelectBooks_Title_Isbn_AuthorAsync(string titlu = null, string isbn = null, string autor = null)
        {
            if (modeArhiva)
            {
                try
                {
                    DataTable dt = await Task.Run(() =>
                    {
                        using (MySqlConnection conn = DatabaseConfig.GetConnection())
                        {
                            conn.Open();
                            MySqlCommand cmd = new MySqlCommand("sp_raport_exemplare_manuale_titlu_isbn_autori_arhivate", conn);
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
            else
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
        }

        private async Task FiltrareExemplare(string cod_inventar = null, string stare = null)
        {
            if (modeArhiva)
            {
                try
                {
                    DataTable dt = await Task.Run(() =>
                    {
                        using (MySqlConnection conn = DatabaseConfig.GetConnection())
                        {
                            conn.Open();
                            MySqlCommand cmd = new MySqlCommand("sp_raport_filtrat_exemplare_arhivate", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@p_cod_inventar", MySqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(cod_inventar) ? DBNull.Value : cod_inventar;
                            cmd.Parameters.Add("@p_stare", MySqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(stare) ? DBNull.Value : stare;

                            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                            DataTable tempDt = new DataTable();
                            da.Fill(tempDt);
                            return tempDt;
                        }
                    });

                    ExemplareGrid.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Eroare filtrare: " + ex.Message);
                }
            }
            else
            {
                try
                {
                    DataTable dt = await Task.Run(() =>
                    {
                        using (MySqlConnection conn = DatabaseConfig.GetConnection())
                        {
                            conn.Open();
                            MySqlCommand cmd = new MySqlCommand("sp_raport_filtrat_exemplare", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@p_cod_inventar", MySqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(cod_inventar) ? DBNull.Value : cod_inventar;
                            cmd.Parameters.Add("@p_stare", MySqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(stare) ? DBNull.Value : stare;

                            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                            DataTable tempDt = new DataTable();
                            da.Fill(tempDt);
                            return tempDt;
                        }
                    });

                    ExemplareGrid.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Eroare filtrare: " + ex.Message);
                }
            }
        }

        private async void StergeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isGestionezExemplare)
            {
                if (ExemplareGrid.SelectedItem is not DataRowView row)
                {
                    MessageBox.Show("Vă rugăm să selectați un exemplar din listă pentru a șterge.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int idExemplar = int.TryParse(row["Id_exemplar"].ToString(), out int idEx) ? idEx : 0;

                if (idExemplar <= 0)
                {
                    MessageBox.Show("Nu s-a putut identifica exemplarul selectat.", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirmare = MessageBox.Show("Sigur doriți să ștergeți definitiv acest exemplar? Acțiunea este ireversibilă.", "Confirmare ștergere", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmare != MessageBoxResult.Yes) return;

                await stergeExemplar(idExemplar);
            }
            else
            {
                if (id_CarteSelectata <= 0)
                {
                    MessageBox.Show("Vă rugăm să selectați o carte din listă pentru a o șterge.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Delete delete = new Delete(id_CarteSelectata);
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow is MainWindow mainWindow)
                    mainWindow.ChangeView(delete);
            }
        }

        private async Task stergeExemplar(int idExemplar)
        {
            try
            {
                int cod = -1;
                string mesaj = "";

                await Task.Run(() =>
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_sterge_exemplar", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id_exemplar", idExemplar);
                            cmd.Parameters.AddWithValue("@p_id_bibliotecar", SesiuneBibliotecar.IdBibliotecarCurent);

                            MySqlParameter paramCod = new MySqlParameter("@p_cod", MySqlDbType.Int32);
                            paramCod.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramCod);

                            MySqlParameter paramMesaj = new MySqlParameter("@p_mesaj", MySqlDbType.VarChar, 255);
                            paramMesaj.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramMesaj);

                            cmd.ExecuteNonQuery();

                            cod = Convert.ToInt32(paramCod.Value);
                            mesaj = paramMesaj.Value?.ToString() ?? "";
                        }
                    }
                });

                if (cod == 0)
                {
                    MessageBox.Show(mesaj, "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    SelectExemplareArhivate(id_CarteExemplare);
                }
                else
                {
                    MessageBox.Show(mesaj, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (isGestionezExemplare)
            {
                string textCautat = SearchTextBox.Text.Trim();
                if (textCautat == "Caută un exemplar...")
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
                        SelectExemplare(id_CarteSelectata);
                    }
                    else
                    {
                        await FiltrareExemplare(textCautat, textCautat);
                    }
                    titlu_exemplar.Text = "Selectați o carte";
                    editura.Text = "Nespecificat";
                    tip.Text = "Carte";
                    imgCoperta.Source = null;
                    locatie.Text = "N/A";
                }
                catch (TaskCanceledException)
                {

                }
            } else
            {
                id_CarteSelectata = 0;
                string textCautat = SearchTextBox.Text.Trim();
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
                    titlu_exemplar.Text = "Selectați o carte";
                    editura.Text = "Nespecificat";
                    tip.Text = "Carte";
                    imgCoperta.Source = null;
                    locatie.Text = "N/A";
                }
                catch (TaskCanceledException)
                {

                }
            }
        }

        private void RefreshBooks_Click(object sender, RoutedEventArgs e)
        {
            id_CarteSelectata = 0;
            isGestionezExemplare = false;
            modeArhiva = false;
            BooksGrid.Visibility = Visibility.Visible;
            ExemplareGrid.Visibility = Visibility.Collapsed;
            SelectBooks();

            btnArhiveaza.Visibility = Visibility.Visible;
            btnDezarhiveaza.Visibility = Visibility.Collapsed;
            iconArhiva.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Archive;
            TextArhiva.Text = "Vezi Arhiva";

            locatie_exemplar.Visibility = Visibility.Visible;
            titlu_exemplar.Text = "Selectați o carte";
            detalii.Text = "DETALII CARTE";
            editura.Text = "Nespecificat";
            tip.Text = "Carte";
            btnEditare.Visibility = Visibility.Visible;
            btnShowExemplare.Visibility = Visibility.Visible;
            AddBook.Visibility = Visibility.Visible;
            SearchTextBox.Text = "Caută o carte...";
            SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            txtBook.Text = "Gestiune Inventar (Cărți)";
            imgCoperta.Source = null;
            locatie.Text = "N/A";

            SearchTextBox.TextChanged -= SearchTextBox_TextChanged;
            SearchTextBox.Text = "Caută o carte...";
            SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
        }

        private string cod_Inventar_CarteSelectata, titlu_CarteSelectata, autor_CarteSelectata, isbn_CarteSelectata, stare_CarteSelectata;

        private async Task arhiveazaCarte()
        {
            try
            {
                int cod = -1;
                string mesaj = "";

                await Task.Run(() =>
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_arhiveaza_carte", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id_carte", id_CarteSelectata);
                            cmd.Parameters.AddWithValue("@p_id_bibliotecar", SesiuneBibliotecar.IdBibliotecarCurent);

                            MySqlParameter paramCod = new MySqlParameter("@p_cod", MySqlDbType.Int32);
                            paramCod.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramCod);

                            MySqlParameter paramMesaj = new MySqlParameter("@p_mesaj", MySqlDbType.VarChar, 255);
                            paramMesaj.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramMesaj);

                            cmd.ExecuteNonQuery();

                            cod = Convert.ToInt32(paramCod.Value);
                            mesaj = paramMesaj.Value?.ToString() ?? "";
                        }
                    }
                });

                if (cod == 0)
                {
                    MessageBox.Show(mesaj, "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(mesaj, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task arhiveazaExemplar(int idExemplar)
        {
            try
            {
                int cod = -1;
                string mesaj = "";

                await Task.Run(() =>
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_arhiveaza_exemplar", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id_exemplar", idExemplar);
                            cmd.Parameters.AddWithValue("@p_id_bibliotecar", SesiuneBibliotecar.IdBibliotecarCurent);

                            MySqlParameter paramCod = new MySqlParameter("@p_cod", MySqlDbType.Int32);
                            paramCod.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramCod);

                            MySqlParameter paramMesaj = new MySqlParameter("@p_mesaj", MySqlDbType.VarChar, 255);
                            paramMesaj.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramMesaj);

                            cmd.ExecuteNonQuery();

                            cod = Convert.ToInt32(paramCod.Value);
                            mesaj = paramMesaj.Value?.ToString() ?? "";
                        }
                    }
                });

                if (cod == 0)
                {
                    SelectBooks();
                }
                else
                {
                    MessageBox.Show(mesaj, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void arhiveaza_Click(object sender, RoutedEventArgs e)
        {
            if (isGestionezExemplare)
            {
                if (ExemplareGrid.SelectedItem is not DataRowView row)
                {
                    MessageBox.Show("Vă rugăm să selectați un exemplar din listă pentru a arhiva.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int idExemplar = int.TryParse(row["Id_exemplar"].ToString(), out int idEx) ? idEx : 0;
                if (idExemplar <= 0) return;

                await arhiveazaExemplar(idExemplar);
                SelectExemplare(id_CarteExemplare);
            }
            else
            {
                if (id_CarteSelectata <= 0)
                {
                    MessageBox.Show("Vă rugăm să selectați o carte din listă pentru a o arhiva.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                await arhiveazaCarte();
                SelectBooks();
            }
        }

        private void BtnSwitchArhiva_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.TextChanged -= SearchTextBox_TextChanged;
            SearchTextBox.Text = isGestionezExemplare ? "Caută un exemplar..." : "Caută o carte...";
            SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;

            if (isGestionezExemplare) 
            {
                id_CarteSelectata = 0;

                modeArhiva = !modeArhiva;
                if (modeArhiva)
                {
                    btnArhiveaza.Visibility = Visibility.Collapsed;
                    btnDezarhiveaza.Visibility = Visibility.Visible;
                    iconArhiva.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Book;
                    TextArhiva.Text = "Exemplare Active";
                    ColArhivareExemplar.Visibility = Visibility.Collapsed;
                    ColDezarhivareExemplar.Visibility = Visibility.Visible;
                    SelectExemplareArhivate(id_CarteExemplare);
                }
                else
                {
                    btnArhiveaza.Visibility = Visibility.Visible;
                    btnDezarhiveaza.Visibility = Visibility.Collapsed;
                    iconArhiva.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Archive;
                    TextArhiva.Text = "Vezi Arhiva";
                    ColArhivareExemplar.Visibility = Visibility.Visible;
                    ColDezarhivareExemplar.Visibility = Visibility.Collapsed;
                    SelectExemplare(id_CarteExemplare);
                }
            } else
            {
                modeArhiva = !modeArhiva;

                if (modeArhiva)
                {
                    btnArhiveaza.Visibility = Visibility.Collapsed;
                    btnDezarhiveaza.Visibility = Visibility.Visible;
                    iconArhiva.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Book;
                    TextArhiva.Text = "Cărți Active";
                    ColActions.Visibility = Visibility.Collapsed;
                    AddBook.Visibility = Visibility.Collapsed;
                    SelectArhivedBooks();
                    titlu_exemplar.Text = "Selectați o carte";
                    editura.Text = "Nespecificat";
                    tip.Text = "Carte";
                    imgCoperta.Source = null;
                    locatie.Text = "N/A";
                }
                else
                {
                    btnArhiveaza.Visibility = Visibility.Visible;
                    btnDezarhiveaza.Visibility = Visibility.Collapsed;
                    iconArhiva.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Archive;
                    TextArhiva.Text = "Vezi Arhiva";
                    ColActions.Visibility = Visibility.Visible;
                    AddBook.Visibility = Visibility.Visible;
                    SelectBooks();
                    titlu_exemplar.Text = "Selectați o carte";
                    editura.Text = "Nespecificat";
                    tip.Text = "Carte";
                    imgCoperta.Source = null;
                    locatie.Text = "N/A";
                }
            }
        }

        private void btnShowExemplare_Click(object sender, RoutedEventArgs e)
        {
            isGestionezExemplare = true;
            modeArhiva = false;

            if (id_CarteSelectata <= 0)
            {
                isGestionezExemplare = false;
                MessageBox.Show("Vă rugăm să selectați o carte din listă.", "Atenție",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BooksGrid.Visibility = Visibility.Collapsed;
            ExemplareGrid.Visibility = Visibility.Visible;

            btnArhiveaza.Visibility = Visibility.Visible;
            btnDezarhiveaza.Visibility = Visibility.Collapsed;
            iconArhiva.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Archive;
            TextArhiva.Text = "Vezi Arhiva";

            locatie_exemplar.Visibility = Visibility.Visible;
            titlu_exemplar.Text = "Selectați un exemplar";
            detalii.Text = "LOCALIZARE ȘI DETALII EXEMPLAR";
            editura.Text = "Nespecificat";
            tip.Text = "Carte";
            btnEditare.Visibility = Visibility.Collapsed;
            btnShowExemplare.Visibility = Visibility.Collapsed;
            AddBook.Visibility = Visibility.Collapsed;
            txtBook.Text = "Gestiune Inventar (Exemplare)";
            imgCoperta.Source = null;
            locatie.Text = "N/A";

            SearchTextBox.TextChanged -= SearchTextBox_TextChanged;
            SearchTextBox.Text = "Caută un exemplar...";
            SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;

            id_CarteExemplare = id_CarteSelectata;
            SelectExemplare(id_CarteExemplare);
            id_CarteSelectata = 0;
        }

        private async void BtnActiuneExemplar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            DataRowView row = btn.DataContext as DataRowView;
            if (row == null) return;

            int idExemplar = int.TryParse(row["Id_exemplar"].ToString(), out int idEx) ? idEx : 0;
            string cod_inventar = row["Cod_Inventar"].ToString();

            if (idExemplar <= 0)
            {
                MessageBox.Show("Nu s-a putut identifica ID-ul cărții selectate.", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (modeArhiva)
                {
                    await dezarhiveazaExemplar(idExemplar);
                    SelectExemplareArhivate(id_CarteExemplare);
                }
                else
                {
                    await arhiveazaExemplar(idExemplar);
                    SelectExemplare(id_CarteExemplare);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: "  + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task dezarhiveazaCarte()
        {
            try
            {
                int cod = -1;
                string mesaj = "";

                await Task.Run(() =>
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_dezarhiveaza_carte", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id_carte", id_CarteSelectata);
                            cmd.Parameters.AddWithValue("@p_id_bibliotecar", SesiuneBibliotecar.IdBibliotecarCurent);

                            MySqlParameter paramCod = new MySqlParameter("@p_cod", MySqlDbType.Int32);
                            paramCod.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramCod);

                            MySqlParameter paramMesaj = new MySqlParameter("@p_mesaj", MySqlDbType.VarChar, 255);
                            paramMesaj.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramMesaj);

                            cmd.ExecuteNonQuery();

                            cod = Convert.ToInt32(paramCod.Value);
                            mesaj = paramMesaj.Value?.ToString() ?? "";
                        }
                    }
                });

                if (cod == 0)
                {
                    MessageBox.Show(mesaj, "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                    SelectArhivedBooks();
                }
                else
                {
                    MessageBox.Show(mesaj, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task dezarhiveazaExemplar(int idExemplar)
        {
            try
            {
                int cod = -1;
                string mesaj = "";

                await Task.Run(() =>
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_dezarhiveaza_exemplar", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id_exemplar", idExemplar);
                            cmd.Parameters.AddWithValue("@p_id_bibliotecar", SesiuneBibliotecar.IdBibliotecarCurent);

                            MySqlParameter paramCod = new MySqlParameter("@p_cod", MySqlDbType.Int32);
                            paramCod.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramCod);

                            MySqlParameter paramMesaj = new MySqlParameter("@p_mesaj", MySqlDbType.VarChar, 255);
                            paramMesaj.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramMesaj);

                            cmd.ExecuteNonQuery();

                            cod = Convert.ToInt32(paramCod.Value);
                            mesaj = paramMesaj.Value?.ToString() ?? "";
                        }
                    }
                });

                if (cod == 0)
                {
                    SelectExemplareArhivate(id_CarteExemplare);
                }
                else
                {
                    MessageBox.Show(mesaj, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void dezarhiveaza_Click(object sender, RoutedEventArgs e)
        {
            if (isGestionezExemplare)
            {
                if (ExemplareGrid.SelectedItem is not DataRowView row)
                {
                    MessageBox.Show("Vă rugăm să selectați un exemplar din listă pentru a dezarhiva.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int idExemplar = int.TryParse(row["Id_exemplar"].ToString(), out int idEx) ? idEx : 0;
                if (idExemplar <= 0) return;

                await dezarhiveazaExemplar(idExemplar);
                SelectExemplareArhivate(id_CarteExemplare);
            }
            else
            {
                if (id_CarteSelectata <= 0)
                {
                    MessageBox.Show("Vă rugăm să selectați o carte din listă pentru a o dezarhiva.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                await dezarhiveazaCarte();
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (isGestionezExemplare)
            {
                UsefulFunction.GotFocus(sender, "Caută un exemplar...");
            } else
            {
                UsefulFunction.GotFocus(sender, "Caută o carte...");
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (isGestionezExemplare)
            {
                UsefulFunction.LostFocus(sender, "Caută un exemplar...");
            }
            else
            {
                UsefulFunction.LostFocus(sender, "Caută o carte...");
            }
        }

        private double pret_CarteSelectata;

        private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BooksGrid.SelectedItem is DataRowView row)
            {
                id_CarteSelectata = int.TryParse(row["Id_carte"].ToString(), out int idCarte) ? idCarte : 0;

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

                imgCoperta.Source = UsefulFunction.GetImagine(id_CarteSelectata, "Imagine", "sp_imagine_carte");
            }
        }

        private void ExemplareGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idExemplarSelectat = 0;
            if (ExemplareGrid.SelectedItem is DataRowView row)
            {
                idExemplarSelectat = int.TryParse(row["Id_exemplar"].ToString(), out int idExemplar) ? idExemplar : 0;
                    try
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_date_exemplar_selectat", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id", idExemplarSelectat);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                int idCarte = 0;
                                if (reader.Read())
                                {
                                    idCarte = Convert.ToInt32(reader["Id_carte"]);
                                    titlu_exemplar.Text = reader["Titlu"].ToString();
                                    locatie.Text = reader["Locatie"].ToString();
                                    editura.Text = reader["Editura"].ToString();
                                    tip.Text = reader["Categorie"].ToString();
                                }
                                imgCoperta.Source = UsefulFunction.GetImagine(idCarte, "Imagine", "sp_imagine_carte");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                    MessageBox.Show("Nu s-a putut identifica ID-ul cărții selectate.", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    SearchTextBox.Text = "Caută o carte...";
                    SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
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
