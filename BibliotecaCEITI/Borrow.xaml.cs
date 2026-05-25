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
        int idBibliotecar = 1;

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Caută un elev...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Caută un elev...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        public Borrow()
        {
            InitializeComponent();
            SelectBook();
            PopuleazaGrupe();
            PopuleazaStari();
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
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void BooksGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string[] coloaneDeAscuns = { "ID_imprumut", "ID_exemplar", "ID_carte", "ID_autor", "ID_categorie", "ID_elev", "ID_grupa", "Termen" };

            if (coloaneDeAscuns.Contains(e.PropertyName))
            {
                e.Column.Visibility = Visibility.Collapsed;
            }
        }

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
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
                MessageBox.Show("Eroare la popularea grupelor: " + ex.Message);
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
                MessageBox.Show("Eroare la popularea starilor: " + ex.Message);
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

                try
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_imagine_carte", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id", id_carte);

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
                                        imgCoperta.Background = new System.Windows.Media.ImageBrush(bitmap);
                                    }
                                }
                                else
                                {
                                    imgCoperta.Background = null;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    imgCoperta.Background = null;
                }
            }
            if (stare == "returnat" || stare == "Returnat")
            {
                returneaza.IsEnabled = false;
            }
            else
            {
                returneaza.IsEnabled = true;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (id_imprumutSelectat == 0)
            {
                MessageBox.Show("Selectează un împrumut din listă.");
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
                            MessageBox.Show("Eroare: " + mesaj);
                            return;
                        }
                    }
                }
                _ = EmailService.NotificaRezervariAsync(id_carte, carte);

                await AplicaFiltreDB();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la returnare: " + ex.Message);
            }
        }

        private async void cbGrupe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await AplicaFiltreDB();
        }

        private async void cbStari_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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