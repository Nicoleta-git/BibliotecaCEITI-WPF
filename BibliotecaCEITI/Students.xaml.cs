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
    /// Interaction logic for Students.xaml
    /// </summary>
    public partial class Students : UserControl
    {
        private CancellationTokenSource _cancellationTokenSource;
        public Students()
        {
            InitializeComponent();
            //SelectStudents();
            SelectStudentsAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddStudent student = new AddStudent();
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.ChangeView(student);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void EditareBtn_Click(object sender, RoutedEventArgs e)
        {
            StudentDetails studentDetails = new StudentDetails();
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.ChangeView(studentDetails);
            }
        }

        private void SelectStudents()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                using (MySqlCommand cmd = new MySqlCommand("sp_selecteaza_studenti", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        List<ElevModel> listaElevi = new List<ElevModel>();

                        foreach (DataRow rand in dt.Rows)
                        {
                            string nume = rand["Nume"].ToString();
                            string prenume = rand["Prenume"].ToString();
                            string numeComplet = nume + " " + prenume;
                            string telefon = rand["Telefon"].ToString();
                            string email = rand["Email"].ToString();
                            string grupa = rand["Grupa"].ToString();

                            string initiale = "";
                            if (!string.IsNullOrEmpty(nume) && !string.IsNullOrEmpty(prenume))
                            {
                                initiale = $"{nume[0]}{prenume[0]}".ToUpper();
                            }

                            ElevModel elev = new ElevModel(
                            Convert.ToInt32(rand["ID_elev"]),
                            numeComplet,
                            initiale,
                            "#4483EC",
                            telefon,
                            email,
                            grupa
                            );

                            listaElevi.Add(elev);
                        }

                        StudentsGrid.ItemsSource = listaElevi;
                    }
                }
            }
            catch (Exception ex)
            {
                {
                    MessageBox.Show("Eroare la încărcarea elevilor: " + ex.Message);
                }
            }
        }

        private async Task SelectStudentsAsync()
        {
            try
            {
                List<ElevModel> listaElevi = await Task.Run(async () =>
                {
                    List<ElevModel> rezultat = new List<ElevModel>();

                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        await conn.OpenAsync();

                        using (MySqlCommand cmd = new MySqlCommand("sp_selecteaza_studenti", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                            {
                                int idOrdinal = reader.GetOrdinal("ID_elev");
                                int numeOrdinal = reader.GetOrdinal("Nume");
                                int prenumeOrdinal = reader.GetOrdinal("Prenume");
                                int telefonOrdinal = reader.GetOrdinal("Telefon");
                                int emailOrdinal = reader.GetOrdinal("Email");
                                int grupaOrdinal = reader.GetOrdinal("Grupa");

                                while (await reader.ReadAsync())
                                {
                                    string nume = reader.GetString(numeOrdinal);
                                    string prenume = reader.GetString(prenumeOrdinal);
                                    string numeComplet = $"{nume} {prenume}";

                                    string telefon = reader.IsDBNull(telefonOrdinal) ? "" : reader.GetString(telefonOrdinal);
                                    string email = reader.IsDBNull(emailOrdinal) ? "" : reader.GetString(emailOrdinal);
                                    string grupa = reader.IsDBNull(grupaOrdinal) ? "" : reader.GetString(grupaOrdinal);

                                    string initiale = "";
                                    if (!string.IsNullOrEmpty(nume) && !string.IsNullOrEmpty(prenume))
                                    {
                                        initiale = $"{nume[0]}{prenume[0]}".ToUpper();
                                    }

                                    ElevModel elev = new ElevModel(
                                        reader.GetInt32(idOrdinal),
                                        numeComplet,
                                        initiale,
                                        "#4483EC",
                                        telefon,
                                        email,
                                        grupa
                                    );

                                    rezultat.Add(elev);
                                }
                            }
                        }
                    }
                    return rezultat;
                });

                StudentsGrid.ItemsSource = listaElevi;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea elevilor: " + ex.Message);
            }
        }

        private async Task Search_StudentsAsync(string elev, string grupa)
        {
            try
            {
                List<ElevModel> listaElevi = await Task.Run(async () =>
                {
                    List<ElevModel> rezultat = new List<ElevModel>();

                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        await conn.OpenAsync();

                        using (MySqlCommand cmd = new MySqlCommand("sp_filtrare_studenti", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@p_elev", MySqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(elev) ? DBNull.Value : elev;
                            cmd.Parameters.Add("@p_grupa", MySqlDbType.VarChar).Value = string.IsNullOrWhiteSpace(grupa) ? DBNull.Value : grupa;

                            using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                            {
                                int idOrdinal = reader.GetOrdinal("ID_elev");
                                int numeOrdinal = reader.GetOrdinal("Nume");
                                int prenumeOrdinal = reader.GetOrdinal("Prenume");
                                int telefonOrdinal = reader.GetOrdinal("Telefon");
                                int emailOrdinal = reader.GetOrdinal("Email");
                                int grupaOrdinal = reader.GetOrdinal("Grupa");

                                while (await reader.ReadAsync())
                                {
                                    string nume = reader.GetString(numeOrdinal);
                                    string prenume = reader.GetString(prenumeOrdinal);
                                    string numeComplet = $"{nume} {prenume}";

                                    string telefon = reader.IsDBNull(telefonOrdinal) ? "" : reader.GetString(telefonOrdinal);
                                    string email = reader.IsDBNull(emailOrdinal) ? "" : reader.GetString(emailOrdinal);
                                    string grupa = reader.IsDBNull(grupaOrdinal) ? "" : reader.GetString(grupaOrdinal);

                                    string initiale = "";
                                    if (!string.IsNullOrEmpty(nume) && !string.IsNullOrEmpty(prenume))
                                    {
                                        initiale = $"{nume[0]}{prenume[0]}".ToUpper();
                                    }

                                    ElevModel elev = new ElevModel(
                                        reader.GetInt32(idOrdinal),
                                        numeComplet,
                                        initiale,
                                        "#4483EC",
                                        telefon,
                                        email,
                                        grupa
                                    );

                                    rezultat.Add(elev);
                                }
                            }
                        }
                    }
                    return rezultat;
                });

                StudentsGrid.ItemsSource = listaElevi;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea elevilor: " + ex.Message);
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
                if (string.IsNullOrWhiteSpace(textCautat))
                {
                    await SelectStudentsAsync();
                }
                else
                {
                    await Search_StudentsAsync(textCautat, textCautat);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        private int id_elevSelectat;
        private string elev, telefon, email, grupa, initiale;

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SelectStudentsAsync();
        }

        private bool activ;

        private void StudentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is ElevModel row)
            {
                id_elevSelectat = row.Id;
                elev = row.NumeElev;
                telefon = row.Telefon;
                email = row.Email;
                grupa = row.Grupa;

                nume_elev.Text = elev;
                nume.Text = elev;
                grupa_e.Text = grupa;

                try
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_returneaza_numarul_de_carti_imprumutate_per_elev", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id_elev", id_elevSelectat == 0 ? (object)DBNull.Value : id_elevSelectat);

                            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable();
                                da.Fill(dt);
                                nr_carti_imprumutate.Text = dt.Rows.Count > 0 ? dt.Rows[0]["Imprumuturi"].ToString() : "0";
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    nr_carti_imprumutate.Text = "0";
                }
            }
        }

    }
}
