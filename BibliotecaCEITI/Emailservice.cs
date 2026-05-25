using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace BibliotecaCEITI
{

    public static class EmailService
    {
        // ── Punct de intrare ────────────────────────────────────────────
        public static async Task NotificaRezervariAsync(int idCarte, string titluCarte)
        {
            if (idCarte <= 0) return;

            try
            {
                // 1. Config SMTP + template din BD
                string server = GetSetare("smtp_server");
                string user = GetSetare("smtp_user");
                string password = GetSetare("smtp_password");
                int port = int.TryParse(GetSetare("smtp_port"), out int p) ? p : 587;
                string template = GetSetare("email_template_disponibilitate");

                if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(template))
                {
                    System.Diagnostics.Debug.WriteLine("[EmailService] Configurație incompletă în tabelul `setari`.");
                    return;
                }

                // 2. Rezervări active nenotificate pentru această carte
                DataTable rezervari = await GetRezervariActiveAsync(idCarte);
                if (rezervari.Rows.Count == 0) return;

                // 3. Conectare SMTP o singură dată pentru toate emailurile
                await Task.Run(() =>
                {
                    using var smtp = new SmtpClient();
                    smtp.Connect(server, port, SecureSocketOptions.StartTls);
                    smtp.Authenticate(user, password);

                    foreach (DataRow rand in rezervari.Rows)
                    {
                        string email = rand["Email"]?.ToString() ?? "";
                        string numeElev = rand["NumeElev"]?.ToString() ?? "Elev";
                        int idRez = rand["Id_rezervare"] == DBNull.Value ? 0
                                         : Convert.ToInt32(rand["Id_rezervare"]);

                        if (string.IsNullOrWhiteSpace(email)) continue;

                        try
                        {
                            // Înlocuiește placeholder-ele din templateul HTML
                            string corp = template
                                .Replace("{{NUME_ELEV}}", numeElev)
                                .Replace("{{TITLU_CARTE}}", titluCarte)
                                .Replace("{{DATA}}", DateTime.Now.ToString("dd MMMM yyyy"));

                            var mesaj = new MimeMessage();
                            mesaj.From.Add(new MailboxAddress("Biblioteca CEITI", user));
                            mesaj.To.Add(new MailboxAddress(numeElev, email));
                            mesaj.Subject = $"Cartea {titluCarte} este acum disponibilă!";
                            mesaj.Body = new TextPart("html") { Text = corp };

                            smtp.Send(mesaj);

                            // Marchează rezervarea ca notificată (evită duplicate)
                            if (idRez > 0) MarcheazaNotificat(idRez);

                            System.Diagnostics.Debug.WriteLine($"[EmailService] Email trimis → {email}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[EmailService] Eroare la {email}: {ex.Message}");
                        }
                    }

                    smtp.Disconnect(true);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EmailService] Eroare generală: {ex.Message}");
            }
        }

     
        private static async Task<DataTable> GetRezervariActiveAsync(int idCarte)
        {
            return await Task.Run(() =>
            {
                using var conn = DatabaseConfig.GetConnection();
                using var cmd = new MySqlCommand("sp_rezervari_active_carte", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@p_id_carte", idCarte);
                conn.Open();
                var dt = new DataTable();
                new MySqlDataAdapter(cmd).Fill(dt);
                return dt;
            });
        }

        
        private static void MarcheazaNotificat(int idRezervare)
        {
            try
            {
                using var conn = DatabaseConfig.GetConnection();
                using var cmd = new MySqlCommand("sp_marcheaza_rezervare_notificata", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@p_id", idRezervare);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EmailService] MarcheazaNotificat: {ex.Message}");
            }
        }

        // ── Citește o valoare din tabelul `setari` ──────────────────────
        private static string GetSetare(string cheie)
        {
            try
            {
                using var conn = DatabaseConfig.GetConnection();
                using var cmd = new MySqlCommand(
                    "SELECT valoare FROM setari WHERE cheie = @cheie", conn);
                cmd.Parameters.AddWithValue("@cheie", cheie);
                conn.Open();
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
            catch { return ""; }
        }
    }
}