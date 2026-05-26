using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for Borrow.xaml
    /// </summary>
    public partial class Borrow : UserControl
    {
        private CancellationTokenSource _cancellationTokenSource;
        private int id_imprumutSelectat, id_exemplar, id_carte, id_autor, id_categorie, id_elev, id_grupa;
        private string elev, grupa, carte, autor, data_imprumut, data_returnare, termen, stare;
        int idBibliotecar = SesiuneBibliotecar.IdBibliotecarCurent;
        private bool isInitialized = false;

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.GotFocus(sender, "Caută un elev...");
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.LostFocus(sender, "Caută un elev...");
        }

        public Borrow()
        {
            InitializeComponent();
            SelectBook();
            PopuleazaGrupe();
            PopuleazaStari();
            cbStari.SelectionChanged += cbStari_SelectionChanged;
            isInitialized = true;
        }

        private void ImprumutNou_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.ChangeView(new CheckInBorrow());
        }

        private void Manuale_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.ChangeView(new ManualBorrow());
        }

        private void SelectBook()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    string query = "CALL sp_date_imprumut();";
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

        private void BooksGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string[] coloaneDeAscuns = { "ID_imprumut", "ID_exemplar", "ID_carte", "ID_autor", "ID_categorie", "ID_elev", "ID_grupa", "Termen" };

            if (coloaneDeAscuns.Contains(e.PropertyName))
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
            string[] coloaneDate = { "Data_returnare", "Data_împrumut" };

            if (coloaneDate.Contains(e.PropertyName))
            {
                if (e.Column is DataGridTextColumn col)
                {
                    Binding b = col.Binding as Binding;
                    if (b != null)
                    {
                        b.StringFormat = "dd.MM.yyyy";
                        b.TargetNullValue = "━";
                    }
                }
            }
        }

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInitialized) return;
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
                await AplicaFiltreDB();
            }
            catch (TaskCanceledException)
            {

            }
        }

        private void PopuleazaGrupe()
        {
            string query = "CALL sp_populeaza_grupe();";
            cbGrupe.Items.Clear();
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
                            lista.Add(new ComboItem { Id = -1, Denumire = "Selectează grupa..." });
                            foreach (DataRow rand in dt.Rows)
                            {
                                ComboItem item = new ComboItem
                                {
                                    Id = Convert.ToInt32(rand["ID_grupa"]),
                                    Denumire = rand["Grupa"].ToString()
                                };
                                lista.Add(item);
                            }

                            cbGrupe.ItemsSource = lista;
                            cbGrupe.DisplayMemberPath = "Denumire";
                            cbGrupe.SelectedValuePath = "Id";
                            cbGrupe.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la popularea grupelor: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopuleazaStari()
        {
            string query = "CALL sp_populeaza_stari_imprumuturi();";
            cbStari.Items.Clear();
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
                            lista.Add(new ComboItem { Id = -1, Denumire = "Selectează starea..." });

                            int index = 0;
                            foreach (DataRow rand in dt.Rows)
                            {
                                lista.Add(new ComboItem
                                {
                                    Id = index++,
                                    Denumire = rand["Stare"].ToString()
                                });
                            }

                            cbStari.ItemsSource = lista;
                            cbStari.DisplayMemberPath = "Denumire";
                            cbStari.SelectedValuePath = "Id";
                            cbStari.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la popularea starilor: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BooksGrid.SelectedItem is DataRowView row)
            {
                id_imprumutSelectat = int.TryParse(row["ID_imprumut"].ToString(), out int idImp) ? idImp : 0;
                id_exemplar = int.TryParse(row["ID_exemplar"].ToString(), out int idEx) ? idEx : 0;
                id_carte = int.TryParse(row["ID_carte"].ToString(), out int idCar) ? idCar : 0;
                id_autor = int.TryParse(row["ID_autor"].ToString(), out int idAut) ? idAut : 0;
                id_categorie = int.TryParse(row["ID_categorie"].ToString(), out int idCat) ? idCat : 0;
                id_elev = int.TryParse(row["ID_elev"].ToString(), out int idEl) ? idEl : 0;
                id_grupa = int.TryParse(row["ID_grupa"].ToString(), out int idGr) ? idGr : 0;

                elev = row["Elev"].ToString();
                grupa = row["Grupa"].ToString();
                carte = row["Carte"].ToString();
                autor = row["Autor"].ToString();
                data_imprumut = row["Data_împrumut"].ToString();
                data_returnare = (row["Data_returnare"] == DBNull.Value || string.IsNullOrWhiteSpace(row["Data_returnare"].ToString())) ? "Nereturnat" : row["Data_returnare"].ToString();
                termen = (row["Termen"] == DBNull.Value || string.IsNullOrWhiteSpace(row["Termen"].ToString())) ? "Nespecificat" : row["Termen"].ToString();
                stare = (row["Stare"] == DBNull.Value || string.IsNullOrWhiteSpace(row["Stare"].ToString())) ? "Nespecificat" : row["Stare"].ToString();

                persoana.Text = string.IsNullOrWhiteSpace(elev) ? "N/A" : elev;
                returnare.Text = data_returnare;
                termen_limita.Text = termen;
                stare_imprumut.Text = stare;

                try
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_titlul_cartii", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id", id_carte == 0 ? (object)DBNull.Value : id_carte);

                            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable();
                                da.Fill(dt);
                                titlu.Text = dt.Rows.Count > 0 ? dt.Rows[0]["Titlu"].ToString() : "Fără titlu";
                            }
                        }
                    }
                }
                catch
                {
                    titlu.Text = "Fără titlu";
                }

                BitmapImage imagine = UsefulFunction.GetImagineCarte(id_carte);

                if (imagine != null)
                {
                    imgCoperta.Background = new System.Windows.Media.ImageBrush(imagine);
                }
                else
                {
                    imgCoperta.Background = null;
                }
            }
            if (stare == "returnat" || stare == "Returnat")
            {
                returneaza.IsEnabled = false;
                returneaza.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
                returneaza.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
                ret.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
                return_icon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
            }
            else
            {
                returneaza.IsEnabled = true;
                returneaza.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22EF4444"));
                returneaza.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                ret.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                return_icon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (id_imprumutSelectat == 0)
            {
                MessageBox.Show("Selectează un împrumut din listă.", "Atenționare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand("sp_returneaza_carte", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_id_imprumut", id_imprumutSelectat);
                        cmd.Parameters.AddWithValue("@p_id_bibliotecar_return", idBibliotecar);
                        cmd.Parameters.AddWithValue("@p_observatii", (object)DBNull.Value);

                        MySqlParameter pCod = new MySqlParameter("@p_cod", MySqlDbType.Int32);
                        pCod.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(pCod);

                        MySqlParameter pMesaj = new MySqlParameter("@p_mesaj", MySqlDbType.VarChar, 255);
                        pMesaj.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(pMesaj);

                        cmd.ExecuteNonQuery();

                        int cod = Convert.ToInt32(pCod.Value);
                        string mesaj = pMesaj.Value != null ? pMesaj.Value.ToString() : "";

                        if (cod != 0)
                        {
                            MessageBox.Show("Eroare: " + mesaj, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
                _ = EmailService.NotificaRezervariAsync(id_carte, carte);

                await AplicaFiltreDB();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la returnare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void cbGrupe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized) return;
            await AplicaFiltreDB();
        }

        private async void cbStari_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized) return;
            await AplicaFiltreDB();
        }

        private async Task AplicaFiltreDB()
        {
            if (txtSearch == null || cbGrupe == null || cbStari == null || BooksGrid == null)
                return;

            string textCautat = txtSearch.Text.Trim();
            if (textCautat == "Caută un elev...")
                textCautat = "";

            int? idGrupa = null;
            if (cbGrupe.SelectedItem is ComboItem itemGrupa && itemGrupa.Id > 0)
                idGrupa = itemGrupa.Id;

            string stareSelectata = null;
            if (cbStari.SelectedItem is ComboItem itemStare && itemStare.Id != -1)
                stareSelectata = itemStare.Denumire;

            try
            {
                DataTable dt = await Task.Run(() =>
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_filtrare_completa_imprumuturi", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_text", string.IsNullOrWhiteSpace(textCautat) ? (object)DBNull.Value : textCautat);
                            cmd.Parameters.AddWithValue("@p_id_grupa", idGrupa == null ? (object)DBNull.Value : idGrupa);
                            cmd.Parameters.AddWithValue("@p_stare", string.IsNullOrWhiteSpace(stareSelectata) ? (object)DBNull.Value : stareSelectata);

                            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                            {
                                DataTable tempDt = new DataTable();
                                da.Fill(tempDt);
                                return tempDt;
                            }
                        }
                    }
                });

                BooksGrid.ItemsSource = dt.DefaultView;
                BooksGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Eroare filtrare: " + ex.Message);
            }
        }

    }
}