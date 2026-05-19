using MySql.Data.MySqlClient;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BCrypt.Net;

namespace BibliotecaCEITI
{
    public partial class LoginWindow : Window
    {
        // ── Token de anulare — anulat la închiderea ferestrei ─────────────
        private CancellationTokenSource _cts = new();

        // ── Flag: credențialele OAuth au fost încărcate cu succes ─────────
        private bool _oauthConfigurat = false;

        public LoginWindow()
        {
            InitializeComponent();

            googleBtn.IsEnabled = false;

            _ = GoogleAuthService.Instance.LogoutAsync();

            _ = InitializeOAuthAsync();
        }

        // ─────────────────────────────────────────────────────────────────
        //  Inițializare OAuth — citește ClientId și ClientSecret din BD
        // ─────────────────────────────────────────────────────────────────
        private async Task InitializeOAuthAsync()
        {
            try
            {
                string? clientId = null, clientSecret = null;

                using (var conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();

                    using var cmd = new MySqlCommand(
                        "SELECT client_id, client_secret FROM configurare_oauth WHERE provider = 'google' AND activ = 1 LIMIT 1", conn);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        clientId = reader.IsDBNull(0) ? null : reader.GetString(0);
                        clientSecret = reader.IsDBNull(1) ? null : reader.GetString(1);
                    }
                }

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Configurarea OAuth Google nu a fost găsită în baza de date.\n" +
                            "Autentificarea Google nu va fi disponibilă.",
                            "Avertisment", MessageBoxButton.OK, MessageBoxImage.Warning);
                        googleBtn.IsEnabled = false;
                    });
                    return;
                }

                GoogleAuthService.Instance.ClientId = clientId;
                GoogleAuthService.Instance.ClientSecret = clientSecret;
                _oauthConfigurat = true;

                Dispatcher.Invoke(() => googleBtn.IsEnabled = true);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        $"Eroare la încărcarea configurației OAuth din baza de date:\n{ex.Message}\n\n" +
                        "Autentificarea Google nu va fi disponibilă.",
                        "Eroare configurare", MessageBoxButton.OK, MessageBoxImage.Error);
                    googleBtn.IsEnabled = false;
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  Autentificare clasică — email + parolă
        // ─────────────────────────────────────────────────────────────────
        private async void ConectareBtn_Click(object sender, RoutedEventArgs e)
        {
            string utilizator = utlizatorTxt.Text.Trim();
            string parola = parolaTxt.Password;

            if (string.IsNullOrWhiteSpace(utilizator) || string.IsNullOrWhiteSpace(parola))
            {
                MessageBox.Show("Completați toate câmpurile.", "Atenție",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            conectareBtn.IsEnabled = false;
            conectareBtn.Content = "Se verifică...";

            try
            {
                int cod;
                int? id;
                string? hash, rol, mesaj;

                using (var conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();
                    using var cmd = new MySqlCommand("sp_login_parola", conn)
                    { CommandType = System.Data.CommandType.StoredProcedure };

                    cmd.Parameters.AddWithValue("p_email", utilizator);
                    AddOut(cmd, "p_id", MySqlDbType.Int32);
                    AddOut(cmd, "p_nume", MySqlDbType.VarChar, 60);
                    AddOut(cmd, "p_prenume", MySqlDbType.VarChar, 60);
                    AddOut(cmd, "p_rol", MySqlDbType.VarChar, 20);
                    AddOut(cmd, "p_hash", MySqlDbType.VarChar, 255);
                    AddOut(cmd, "p_auth_metoda", MySqlDbType.VarChar, 20);
                    AddOut(cmd, "p_cod", MySqlDbType.Int32);
                    AddOut(cmd, "p_mesaj", MySqlDbType.VarChar, 255);

                    await cmd.ExecuteNonQueryAsync();

                    cod = Convert.ToInt32(cmd.Parameters["p_cod"].Value);
                    id = cmd.Parameters["p_id"].Value is DBNull ? null
                              : Convert.ToInt32(cmd.Parameters["p_id"].Value);
                    hash = cmd.Parameters["p_hash"].Value is DBNull ? null
                              : cmd.Parameters["p_hash"].Value?.ToString();
                    rol = cmd.Parameters["p_rol"].Value?.ToString();
                    mesaj = cmd.Parameters["p_mesaj"].Value?.ToString();
                }

                switch (cod)
                {
                    case 1:
                        ShowLoginError("Email sau parolă incorectă.");
                        return;
                    case 2:
                        ShowLoginError(mesaj ?? "Contul este dezactivat.");
                        return;
                    case 3:
                        MessageBox.Show(
                            "Acest cont folosește autentificarea Google.\n" +
                            "Apăsați [Continuă cu Google] pentru a vă conecta.",
                            "Informație", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    case 4:
                        ShowLoginError("Parola nu este configurată. Contactați administratorul.");
                        return;
                }

                bool parolaValida = await Task.Run(() =>
                    BCrypt.Net.BCrypt.Verify(parola, hash));

                if (!parolaValida)
                {
                    ShowLoginError("Email sau parolă incorectă.");
                    return;
                }

                string token = Guid.NewGuid().ToString("N");
                await CreeazaSesiuneAsync(id!.Value, "parola", token);

                DeschideMainWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                conectareBtn.IsEnabled = true;
                conectareBtn.Content = "CONECTARE";
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  Autentificare Google OAuth
        // ─────────────────────────────────────────────────────────────────
        private async void GoogleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_oauthConfigurat)
            {
                MessageBox.Show(
                    "Configurarea Google OAuth nu este gata încă.\n" +
                    "Așteptați câteva secunde și încercați din nou.",
                    "Avertisment", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            googleBtn.IsEnabled = false;

            try
            {
                var result = await GoogleAuthService.Instance.LoginAsync(_cts.Token);

                if (!result.Success)
                {
                    if (!_cts.IsCancellationRequested)
                        MessageBox.Show(result.ErrorMsg ?? "Autentificare eșuată.",
                            "Eroare Google", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int cod; int? idBib; string? mesaj;

                using (var conn = DatabaseConfig.GetConnection())
                {
                    await conn.OpenAsync();
                    using var cmd = new MySqlCommand("sp_login_google", conn)
                    { CommandType = System.Data.CommandType.StoredProcedure };

                    var (numeDb, prenumeDb) = SplitNume(result.NumeFull);

                    cmd.Parameters.AddWithValue("p_google_id", result.GoogleId);
                    cmd.Parameters.AddWithValue("p_email", result.Email);
                    cmd.Parameters.AddWithValue("p_nume", numeDb);
                    cmd.Parameters.AddWithValue("p_prenume", prenumeDb);
                    AddOut(cmd, "p_id", MySqlDbType.Int32);
                    AddOut(cmd, "p_rol", MySqlDbType.VarChar, 20);
                    AddOut(cmd, "p_este_nou", MySqlDbType.Byte);
                    AddOut(cmd, "p_cod", MySqlDbType.Int32);
                    AddOut(cmd, "p_mesaj", MySqlDbType.VarChar, 255);

                    await cmd.ExecuteNonQueryAsync();

                    cod = Convert.ToInt32(cmd.Parameters["p_cod"].Value);
                    idBib = cmd.Parameters["p_id"].Value is DBNull ? null
                              : Convert.ToInt32(cmd.Parameters["p_id"].Value);
                    mesaj = cmd.Parameters["p_mesaj"].Value?.ToString();
                }

                switch (cod)
                {
                    case 2:
                        await GoogleAuthService.Instance.LogoutAsync();
                        MessageBox.Show(mesaj ?? "Contul este dezactivat.",
                            "Acces refuzat", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    case 5:
                        await GoogleAuthService.Instance.LogoutAsync();
                        MessageBox.Show(
                            "Nu există niciun cont de bibliotecar asociat acestui email Google.\n" +
                            "Contactați administratorul.",
                            "Acces refuzat", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    case 0:
                        break;

                    default:
                        await GoogleAuthService.Instance.LogoutAsync();
                        MessageBox.Show(mesaj ?? "Eroare necunoscută.",
                            "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }

                string token = Guid.NewGuid().ToString("N");
                await CreeazaSesiuneAsync(idBib!.Value, "google", token);

                await GoogleAuthService.Instance.LogoutAsync();

                DeschideMainWindow();
            }
            catch (OperationCanceledException) { /* fereastra s-a închis */ }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare: {ex.Message}", "Eroare",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (_oauthConfigurat)
                    googleBtn.IsEnabled = true;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  Butoane existente
        // ─────────────────────────────────────────────────────────────────
        private void Registrare_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Registrare registrareWindow = new Registrare();
                registrareWindow.Show();
                this.Close();
            }
        }

        private async void ExitBtn(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();
            await GoogleAuthService.Instance.LogoutAsync();
            Application.Current.Shutdown();
        }

        // ─────────────────────────────────────────────────────────────────
        //  Curățare garantată la orice mod de închidere a ferestrei
        // ─────────────────────────────────────────────────────────────────
        protected override void OnClosed(EventArgs e)
        {
            _cts.Cancel();
            _cts.Dispose();
            base.OnClosed(e);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Helper: creează sesiunea în BD via sp_creeaza_sesiune
        // ─────────────────────────────────────────────────────────────────
        private async Task CreeazaSesiuneAsync(int idBib, string metoda, string token)
        {
            using var conn = DatabaseConfig.GetConnection();
            await conn.OpenAsync();
            using var cmd = new MySqlCommand("sp_creeaza_sesiune", conn)
            { CommandType = System.Data.CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("p_id_bibliotecar", idBib);
            cmd.Parameters.AddWithValue("p_metoda", metoda);
            cmd.Parameters.AddWithValue("p_token", token);
            cmd.Parameters.AddWithValue("p_ip", "127.0.0.1");
            AddOut(cmd, "p_cod", MySqlDbType.Int32);
            AddOut(cmd, "p_mesaj", MySqlDbType.VarChar, 255);

            await cmd.ExecuteNonQueryAsync();
        }

        // ─────────────────────────────────────────────────────────────────
        //  Helpers statice
        // ─────────────────────────────────────────────────────────────────
        private void DeschideMainWindow()
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void ShowLoginError(string mesaj)
        {
            MessageBox.Show(mesaj, "Autentificare eșuată",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            parolaTxt.Clear();
            parolaTxt.Focus();
        }

        private static (string nume, string prenume) SplitNume(string? full)
        {
            if (string.IsNullOrWhiteSpace(full)) return ("Necunoscut", "");
            var p = full.Trim().Split(' ', 2);
            return p.Length >= 2 ? (p[1], p[0]) : (p[0], "");
        }

        private static void AddOut(MySqlCommand cmd, string name,
            MySqlDbType type, int size = 0)
        {
            var p = new MySqlParameter(name, type)
            { Direction = System.Data.ParameterDirection.Output };
            if (size > 0) p.Size = size;
            cmd.Parameters.Add(p);
        }
    }
}