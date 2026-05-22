using MySql.Data.MySqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for CheckInBorrowStep_1.xaml
    /// </summary>
    public partial class CheckInBorrowStep_1 : UserControl
    {
        private CancellationTokenSource _cancellationTokenSource;
        public CheckInBorrowStep_1()
        {
            InitializeComponent();
            SelectStudentsAsync();

            SearchBox.Text = "Caută un elev...";
            SearchBox.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private async Task SelectStudentsAsync()
        {
            List<ElevModel> elevi = new List<ElevModel>();

            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();

                    using (MySqlCommand cmd = new MySqlCommand("sp_selecteaza_studenti", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int id = Convert.ToInt32(reader["ID_elev"]);
                                string nume = reader["Nume"].ToString();
                                string prenume = reader["Prenume"].ToString();
                                string telefon = reader["Telefon"].ToString();
                                string email = reader["Email"].ToString();
                                string grupa = reader["Grupa"].ToString();

                                string numeComplet = nume + " " + prenume;

                                string initiale = "";
                                if (nume.Length > 0 && prenume.Length > 0)
                                {
                                    initiale = nume.Substring(0, 1) + prenume.Substring(0, 1);
                                    initiale = initiale.ToUpper();
                                }

                                ElevModel elev = new ElevModel(id, numeComplet, initiale, "#4483EC", telefon, email, grupa);
                                elevi.Add(elev);
                            }
                        }
                    }
                }

                StudentsGrid.ItemsSource = elevi;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea elevilor: " + ex.Message);
            }
        }

        private async Task SearchStudentsAsync(string elev, string grupa)
        {
            List<ElevModel> eleviGasiți = new List<ElevModel>();

            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();

                    using (MySqlCommand cmd = new MySqlCommand("sp_filtrare_studenti", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (string.IsNullOrWhiteSpace(elev))
                            cmd.Parameters.AddWithValue("@p_elev", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@p_elev", elev);

                        if (string.IsNullOrWhiteSpace(grupa))
                            cmd.Parameters.AddWithValue("@p_grupa", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@p_grupa", grupa);

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int id = Convert.ToInt32(reader["ID_elev"]);
                                string nume = reader["Nume"].ToString();
                                string prenume = reader["Prenume"].ToString();
                                string telefon = reader["Telefon"].ToString();
                                string email = reader["Email"].ToString();
                                string grupaVal = reader["Grupa"].ToString();

                                string numeComplet = nume + " " + prenume;

                                string initiale = "";
                                if (nume.Length > 0 && prenume.Length > 0)
                                {
                                    initiale = nume.Substring(0, 1) + prenume.Substring(0, 1);
                                    initiale = initiale.ToUpper();
                                }

                                ElevModel elevModel = new ElevModel(id, numeComplet, initiale, "#4483EC", telefon, email, grupaVal);
                                eleviGasiți.Add(elevModel);
                            }
                        }
                    }
                }

                StudentsGrid.ItemsSource = eleviGasiți;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la filtrarea elevilor: " + ex.Message);
            }
        }

        public event Action<int> IdSelected;
        private int id_elevSelectat, activ;
        private string elev, telefon, email, grupa, initiale;

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Caută un elev...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Caută un elev...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string textCautat = SearchBox.Text.Trim();
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
                    await SelectStudentsAsync();
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

        private void StudentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is ElevModel row)
            {
                id_elevSelectat = row.Id;
                elev = row.NumeElev;
                telefon = row.Telefon;
                email = row.Email;
                grupa = row.Grupa;
                initiale = row.Initiale;

                nume_elev.Text = elev;
                grupa_elev.Text = "Grupa: " + grupa;
                ID_elev.Text = "ID Elev: " + id_elevSelectat;
                telefon_elev.Text = "Telefon: " + telefon;
                initiale_elev.Text = initiale;

                int id = row.Id;
                IdSelected?.Invoke(id);

                try
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_returneaza_stare_elev", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id", id_elevSelectat == 0 ? (object)DBNull.Value : id_elevSelectat);

                            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable();
                                da.Fill(dt);

                                if (dt.Rows.Count > 0)
                                {
                                    activ = Convert.ToInt32(dt.Rows[0]["activ"]);

                                    if (activ == 1)
                                    {
                                        activ_elev.Text = "Este activ";
                                        activ_elev.Foreground = Brushes.Green;
                                        fon_activ.Background = new SolidColorBrush(Color.FromArgb(30, 0, 128, 0));
                                    }
                                    else
                                    {
                                        activ_elev.Text = "Este inactiv";
                                        activ_elev.Foreground = Brushes.Red;
                                        fon_activ.Background = new SolidColorBrush(Color.FromArgb(30, 255, 64, 64));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    activ_elev.Text = "Eroare";
                    activ_elev.Foreground = Brushes.OrangeRed;
                }
            }
        }


    }
}
