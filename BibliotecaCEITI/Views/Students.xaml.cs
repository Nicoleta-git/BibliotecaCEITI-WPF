using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QRCoder;

namespace BibliotecaCEITI
{
    public partial class Students : UserControl
    {
        private CancellationTokenSource _cancellationTokenSource;
        public Students()
        {
            InitializeComponent();
            LoadData();

            SearchTextBox.Text = "Caută un elev...";
            SearchTextBox.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private async void LoadData()
        {
            await SelectStudentsAsync();
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (id_elevSelectat <= 0)
            {
                MessageBox.Show("Vă rugăm să selectați un student din listă pentru a șterge datele.", "Atenționare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Sigur doriți să ștergeți definitiv elevul? Această acțiune este ireversibilă.", "Atenție", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            int succes = 0;
            string mesaj = "";

            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_delete_elev", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("p_id", id_elevSelectat);

                        MySqlParameter paramSucces = new MySqlParameter("p_succes", MySqlDbType.Byte);
                        paramSucces.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(paramSucces);

                        MySqlParameter paramMesaj = new MySqlParameter("p_mesaj", MySqlDbType.VarChar, 255);
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
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Eroare: " + mesaj, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare la baza de date: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditareBtn_Click(object sender, RoutedEventArgs e)
        {
            if (id_elevSelectat <= 0)
            {
                MessageBox.Show("Vă rugăm să selectați un student din listă pentru a edita datele.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            StudentDetails updateControl = new StudentDetails(id_elevSelectat);
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.ChangeView(updateControl);
            }
        }

        private BitmapImage GenereazaCodQR(string textPentruQR)
        {
            if (string.IsNullOrEmpty(textPentruQR)) return null;

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(textPentruQR, QRCodeGenerator.ECCLevel.Q);

                using (BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData))
                {
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);

                    using (MemoryStream stream = new MemoryStream(qrCodeBytes))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = stream;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        return bitmapImage;
                    }
                }
            }
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
                MessageBox.Show("Eroare la încărcarea elevilor: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
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

                        cmd.Parameters.AddWithValue("@p_elev", string.IsNullOrWhiteSpace(elev) ? (object)DBNull.Value : elev);
                        cmd.Parameters.AddWithValue("@p_grupa", string.IsNullOrWhiteSpace(grupa) ? (object)DBNull.Value : grupa);

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
                MessageBox.Show("Eroare la filtrarea elevilor: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string textCautat = SearchTextBox.Text.Trim();
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

        private int id_elevSelectat;
        private string elev, telefon, email, grupa, initiale;

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.GotFocus(sender, "Caută un elev...");
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.LostFocus(sender, "Caută un elev...");
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await SelectStudentsAsync();
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
                nume.Text = elev;
                grupa_e.Text = grupa;

                string textDeScanat = $"--- DETALII ELEV ---\n" +
                                      $"Nume: {elev}\n" +
                                      $"Grupa: {grupa}\n" +
                                      $"Telefon: {telefon}\n" +
                                      $"Email: {email}";
                if (qr_code_elev_detalii != null)
                {
                    qr_code_elev_detalii.Source = GenereazaCodQR(textDeScanat);
                }

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
            else
            {
                if (qr_code_elev_detalii != null)
                {
                    qr_code_elev_detalii.Source = null;
                }
            }
        }
    }
}