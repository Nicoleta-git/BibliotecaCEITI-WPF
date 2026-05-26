using Microsoft.Win32;
using MySql.Data.MySqlClient;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        private int idBibliotecar = 0;
        public Settings()
        {
            InitializeComponent();
            idBibliotecar = SesiuneBibliotecar.IdBibliotecarCurent;
            DisplayData(idBibliotecar);
        }

        private async Task DisplayData(int id)
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();
                    using (MySqlCommand cmd = new MySqlCommand("sp_select_data_bibliotecar", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("p_id", id);

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                SesiuneBibliotecar.NumeBibliotecar = reader["NumePorfil_Bibliotecar"].ToString();
                                SesiuneBibliotecar.Email = reader["Email"].ToString();
                                SesiuneBibliotecar.Telefon = reader["Telefon"].ToString();
                                SesiuneBibliotecar.RolBibliotecar = reader["Rol"].ToString();

                                if (reader["UltimaAutentificare"] != DBNull.Value)
                                    SesiuneBibliotecar.ultimaAutentificare = Convert.ToDateTime(reader["UltimaAutentificare"]);

                                if (reader["UltimaDataModificare"] != DBNull.Value)
                                    SesiuneBibliotecar.ultimaModificare = Convert.ToDateTime(reader["UltimaDataModificare"]);

                                if (reader["DataCreareCont"] != DBNull.Value)
                                    SesiuneBibliotecar.dataCreare = Convert.ToDateTime(reader["DataCreareCont"]);

                                if (!reader.IsDBNull(reader.GetOrdinal("Poza_de_pofil")))
                                {
                                    SesiuneBibliotecar.PozaProfil = (byte[])reader["Poza_de_pofil"];
                                    using (MemoryStream ms = new MemoryStream(SesiuneBibliotecar.PozaProfil))
                                    {
                                        BitmapImage bitmap = new BitmapImage();
                                        bitmap.BeginInit();
                                        bitmap.StreamSource = ms;
                                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                        bitmap.EndInit();
                                        bitmap.Freeze();
                                        imgSettingsLogo.Source = bitmap;
                                    }
                                }
                                else
                                {
                                    imgSettingsLogo.Source = null;
                                }

                                int zile = (int)(DateTime.Now - SesiuneBibliotecar.dataCreare).TotalDays;
                                lblFine.Text = zile + " zile";
                                lblCreatedAt.Text = SesiuneBibliotecar.dataCreare.ToString("dd.MM.yyyy HH:mm");
                                lblLastLogin.Text = SesiuneBibliotecar.ultimaAutentificare.ToString("dd.MM.yyyy HH:mm");
                                txtTelefon.Text = SesiuneBibliotecar.Telefon;
                                txtEmail.Text = SesiuneBibliotecar.Email;
                                txtInstitutie.Text = SesiuneBibliotecar.NumeBibliotecar;
                            }
                        }
                    }

                    using (MySqlCommand cmd = new MySqlCommand("sp_imprumuturi_confirmate_per_bibliotecar", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("p_id", id);

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int imprumuturi = int.TryParse(reader["ÎmprumuturiConfirmate"].ToString(), out int i) ? i : 0;
                                lblLoanCount.Text = imprumuturi.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea datelor bibliotecarului: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSys_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new SystemSettings();
            }
        }

        private async void btnSalveazaSetari_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] pozaBytes = null;
                if (imgSettingsLogo.Source is BitmapImage bitmap && bitmap.UriSource != null)
                {
                    pozaBytes = File.ReadAllBytes(bitmap.UriSource.LocalPath);
                }

                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();
                    using (MySqlCommand cmd = new MySqlCommand("sp_update_data_bibliotecar", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_id", idBibliotecar);
                        cmd.Parameters.AddWithValue("@p_email", string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@p_telefon", string.IsNullOrWhiteSpace(txtTelefon.Text) ? (object)DBNull.Value : txtTelefon.Text.Trim());
                        cmd.Parameters.AddWithValue("@p_poza", pozaBytes ?? (object)DBNull.Value);

                        MySqlParameter pSucces = new MySqlParameter("@p_succes", MySqlDbType.Byte) { Direction = ParameterDirection.Output };
                        MySqlParameter pMesaj = new MySqlParameter("@p_mesaj", MySqlDbType.VarChar, 255) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(pSucces);
                        cmd.Parameters.Add(pMesaj);

                        await cmd.ExecuteNonQueryAsync();

                        string mesaj = pMesaj.Value?.ToString() ?? "";
                        if (Convert.ToByte(pSucces.Value) == 1)
                        {
                            MessageBox.Show(mesaj, "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

                            SesiuneBibliotecar.Email = txtEmail.Text.Trim();
                            SesiuneBibliotecar.Telefon = txtTelefon.Text.Trim();
                            if (pozaBytes != null)
                                SesiuneBibliotecar.PozaProfil = pozaBytes;

                            var mainWindow = Window.GetWindow(this) as MainWindow;
                            mainWindow?.ActualizeazaHeader(
                                SesiuneBibliotecar.NumeBibliotecar,
                                SesiuneBibliotecar.RolBibliotecar,
                                SesiuneBibliotecar.PozaProfil);
                        }
                        else
                        {
                            MessageBox.Show(mesaj, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la salvare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUploadImage_Click(object sender, RoutedEventArgs e)
        {
            var rezultat = UsefulFunction.AlegeImagineDinFisier();

            if (rezultat.Imagine != null)
            {
                imgSettingsLogo.Source = rezultat.Imagine;
            }
        }

    }
}
