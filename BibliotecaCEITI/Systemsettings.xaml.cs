using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Setări avansate sistem — deschise din butonul SYS din Settings.xaml.
    /// Toate operațiunile pe BD se fac exclusiv prin proceduri stocate:
    ///   sp_get_setari          — încărcarea inițială a tuturor setărilor
    ///   sp_salveaza_general    — salvare bloc General
    ///   sp_salveaza_smtp       — salvare bloc SMTP
    ///   sp_salveaza_app        — salvare bloc Setări aplicație
    ///   sp_salveaza_template   — salvare template HTML (din TemplateViewerWindow)
    /// </summary>
    public partial class SystemSettings : UserControl
    {
        // ── Stare editare per secțiune ──────────────────────────────────────
        private bool _editGeneral = false;
        private bool _editSmtp = false;
        private bool _editApp = false;

        // ── Cache setări încărcate din BD ───────────────────────────────────
        private Dictionary<string, string> _setari = new Dictionary<string, string>();

        public SystemSettings()
        {
            InitializeComponent();
            IncarcaSetari();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ÎNCĂRCARE DATE — sp_get_setari
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Apelează sp_get_setari și populează cache-ul local _setari.
        /// </summary>
        private void IncarcaSetari()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand("sp_get_setari", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            _setari.Clear();
                            while (reader.Read())
                                _setari[reader.GetString(0)] = reader.GetString(1);
                        }
                    }
                }

                AfiseazaDate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea setărilor: " + ex.Message,
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Helper rapid pentru citire din cache ───────────────────────────
        private string Get(string cheie, string fallback = "—")
            => _setari.TryGetValue(cheie, out string val) ? val : fallback;

        private void AfiseazaDate()
        {
            // ── General ────────────────────────────────────────────────────
            lblDenumireInstitutie.Text = Get("denumire_institutie");
            lblAdresa.Text = Get("adresa_bibliotecii");
            lblTelefon.Text = Get("telefon_contact");
            lblEmailBiblioteca.Text = Get("email_biblioteca");
            lblWebsite.Text = Get("website");

            // ── SMTP ───────────────────────────────────────────────────────
            string port = Get("smtp_port", "587");
            string ssl = Get("smtp_ssl", "TLS");
            lblSmtpServer.Text = Get("smtp_server");
            lblSmtpPort.Text = $"{port} ({ssl})";
            lblSmtpEmail.Text = Get("smtp_email_expeditor");
            // Parola rămâne mascată în view mode

            // ── Setări aplicație ───────────────────────────────────────────
            lblDurataImprumut.Text = Get("durata_imprumut_zile");
            lblMaxImprumuturi.Text = Get("max_imprumuturi_per_elev");
            lblDurataRezervare.Text = Get("durata_rezervare_zile");
            lblDurataManual.Text = Get("durata_manual_luni");
            lblPenalizare.Text = Get("penalizare_per_zi_mdl");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  TOGGLE EDIT — GENERAL
        // ═══════════════════════════════════════════════════════════════════
        private void BtnEditGeneral_Click(object sender, RoutedEventArgs e)
        {
            _editGeneral = !_editGeneral;

            if (_editGeneral)
            {
                txtDenumireInstitutie.Text = Get("denumire_institutie");
                txtAdresa.Text = Get("adresa_bibliotecii");
                txtTelefonContact.Text = Get("telefon_contact");
                txtEmailBiblioteca.Text = Get("email_biblioteca");
                txtWebsite.Text = Get("website");

                pnlGeneralView.Visibility = Visibility.Collapsed;
                pnlGeneralEdit.Visibility = Visibility.Visible;
                btnEditGeneral.Content = BuildButtonContent("Solid_Times", "Anulează", "#E53E3E");
            }
            else
            {
                pnlGeneralView.Visibility = Visibility.Visible;
                pnlGeneralEdit.Visibility = Visibility.Collapsed;
                btnEditGeneral.Content = BuildButtonContent("Solid_Pen", "Editează", "#4483EC");
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  TOGGLE EDIT — SMTP
        // ═══════════════════════════════════════════════════════════════════
        private void BtnEditSmtp_Click(object sender, RoutedEventArgs e)
        {
            _editSmtp = !_editSmtp;

            if (_editSmtp)
            {
                txtSmtpServer.Text = Get("smtp_server");
                txtSmtpPort.Text = Get("smtp_port", "587");
                txtSmtpEmail.Text = Get("smtp_email_expeditor");
                // Nu precompletăm parola pentru securitate

                pnlSmtpView.Visibility = Visibility.Collapsed;
                pnlSmtpEdit.Visibility = Visibility.Visible;
                btnEditSmtp.Content = BuildButtonContent("Solid_Times", "Anulează", "#E53E3E");
            }
            else
            {
                pnlSmtpView.Visibility = Visibility.Visible;
                pnlSmtpEdit.Visibility = Visibility.Collapsed;
                btnEditSmtp.Content = BuildButtonContent("Solid_Pen", "Editează", "#4483EC");
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  TOGGLE EDIT — APP SETTINGS
        // ═══════════════════════════════════════════════════════════════════
        private void BtnEditApp_Click(object sender, RoutedEventArgs e)
        {
            _editApp = !_editApp;

            if (_editApp)
            {
                txtDurataImprumut.Text = Get("durata_imprumut_zile");
                txtMaxImprumuturi.Text = Get("max_imprumuturi_per_elev");
                txtDurataRezervare.Text = Get("durata_rezervare_zile");
                txtDurataManual.Text = Get("durata_manual_luni");
                txtPenalizare.Text = Get("penalizare_per_zi_mdl");

                pnlAppView.Visibility = Visibility.Collapsed;
                pnlAppEdit.Visibility = Visibility.Visible;
                btnEditApp.Content = BuildButtonContent("Solid_Times", "Anulează", "#E53E3E");
            }
            else
            {
                pnlAppView.Visibility = Visibility.Visible;
                pnlAppEdit.Visibility = Visibility.Collapsed;
                btnEditApp.Content = BuildButtonContent("Solid_Pen", "Editează", "#4483EC");
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  SALVARE GLOBALĂ
        //  Apelează câte o procedură stocată per secțiune activă.
        //  Dacă oricare procedură returnează p_cod != 0, salvarea se oprește
        //  și utilizatorul vede mesajul de eroare returnat de BD.
        // ═══════════════════════════════════════════════════════════════════
        private void BtnSalveazaSistem_Click(object sender, RoutedEventArgs e)
        {
            // ── Validări C# înainte de apelul la BD ────────────────────────
            if (_editApp)
            {
                if (!int.TryParse(txtDurataImprumut.Text, out int durataImp) || durataImp <= 0)
                {
                    MessageBox.Show("Durata împrumutului trebuie să fie un număr întreg pozitiv.",
                        "Validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!int.TryParse(txtMaxImprumuturi.Text, out int maxImp) || maxImp <= 0)
                {
                    MessageBox.Show("Max. împrumuturi trebuie să fie un număr întreg pozitiv.",
                        "Validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!decimal.TryParse(txtPenalizare.Text,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal pen) || pen < 0)
                {
                    MessageBox.Show("Penalizarea trebuie să fie un număr zecimal pozitiv (ex: 1.00).",
                        "Validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();

                    // ── Bloc General ───────────────────────────────────────
                    if (_editGeneral)
                    {
                        if (!ApeleazaSalvare(conn,
                            "sp_salveaza_general",
                            new (string, object)[]
                            {
                                ("p_denumire_institutie", txtDenumireInstitutie.Text.Trim()),
                                ("p_adresa_bibliotecii",  txtAdresa.Text.Trim()),
                                ("p_telefon_contact",     txtTelefonContact.Text.Trim()),
                                ("p_email_biblioteca",    txtEmailBiblioteca.Text.Trim()),
                                ("p_website",             txtWebsite.Text.Trim()),
                            }))
                            return; // mesajul de eroare deja afișat în ApeleazaSalvare
                    }

                    // ── Bloc SMTP ──────────────────────────────────────────
                    if (_editSmtp)
                    {
                        string parola = txtSmtpParola.Password; // poate fi gol = nu suprascrie
                        if (!ApeleazaSalvare(conn,
                            "sp_salveaza_smtp",
                            new (string, object)[]
                            {
                                ("p_smtp_server",          txtSmtpServer.Text.Trim()),
                                ("p_smtp_port",            txtSmtpPort.Text.Trim()),
                                ("p_smtp_email_expeditor", txtSmtpEmail.Text.Trim()),
                                ("p_smtp_parola",          parola),
                            }))
                            return;
                    }

                    // ── Bloc Setări aplicație ──────────────────────────────
                    if (_editApp)
                    {
                        // Normalizăm separatorul zecimal la punct înainte de trimitere
                        string penalizare = txtPenalizare.Text.Trim().Replace(',', '.');

                        if (!ApeleazaSalvare(conn,
                            "sp_salveaza_app",
                            new (string, object)[]
                            {
                                ("p_durata_imprumut_zile",  txtDurataImprumut.Text.Trim()),
                                ("p_max_imprumuturi",        txtMaxImprumuturi.Text.Trim()),
                                ("p_durata_rezervare_zile", txtDurataRezervare.Text.Trim()),
                                ("p_durata_manual_luni",    txtDurataManual.Text.Trim()),
                                ("p_penalizare_per_zi_mdl", penalizare),
                            }))
                            return;
                    }
                }

                // Resetează toate secțiunile la view mode
                ResetEditMode();
                // Reîncarcă datele proaspete din BD prin sp_get_setari
                IncarcaSetari();

                MessageBox.Show("Setările au fost salvate cu succes!",
                    "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare neașteptată la salvarea setărilor: " + ex.Message,
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  TEMPLATE-URI — Butoane „Vezi cod"
        // ═══════════════════════════════════════════════════════════════════
        private void BtnVezuCodIntarziere_Click(object sender, RoutedEventArgs e)
            => DeschideTemplateViewer("email_corp_html", "Atenționare întârziere");

        private void BtnVezuCodPreventiv_Click(object sender, RoutedEventArgs e)
            => DeschideTemplateViewer("email_notificare_preventiva_html", "Notificare preventivă");

        private void BtnVezuCodPierduta_Click(object sender, RoutedEventArgs e)
            => DeschideTemplateViewer("email_penalizare_pierduta_html", "Penalizare carte pierdută");

        private void BtnVezuCodRezervare_Click(object sender, RoutedEventArgs e)
            => DeschideTemplateViewer("email_rezervare_confirmata_html", "Rezervare confirmată");

        private void DeschideTemplateViewer(string cheie, string titlu)
        {
            string continut = Get(cheie, "<!-- Template-ul nu a fost găsit -->");
            var win = new TemplateViewerWindow(titlu, continut, cheie);
            win.ShowDialog();
            // Reîncărcăm cache-ul după ce fereastra se închide, în caz că
            // utilizatorul a salvat modificări la template
            IncarcaSetari();
        }

        // ═══════════════════════════════════════════════════════════════════
        //  ÎNAPOI la Settings
        // ═══════════════════════════════════════════════════════════════════
        private void BtnInapoi_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new Settings();
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        //  HELPERS PRIVATE
        // ═══════════════════════════════════════════════════════════════════
        private static bool ApeleazaSalvare(
            MySqlConnection conn,
            string numeProcedura,
            (string Nume, object Valoare)[] parametriIn)
        {
            using (MySqlCommand cmd = new MySqlCommand(numeProcedura, conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Adaugă parametrii IN
                foreach (var (nume, valoare) in parametriIn)
                    cmd.Parameters.AddWithValue(nume, valoare ?? (object)DBNull.Value);

                // Parametrii OUT comuni tuturor procedurilor de salvare
                var pCod = new MySqlParameter("p_cod", MySqlDbType.Int32)
                { Direction = System.Data.ParameterDirection.Output };
                var pMesaj = new MySqlParameter("p_mesaj", MySqlDbType.VarChar, 255)
                { Direction = System.Data.ParameterDirection.Output };

                cmd.Parameters.Add(pCod);
                cmd.Parameters.Add(pMesaj);

                cmd.ExecuteNonQuery();

                int cod = pCod.Value != DBNull.Value ? Convert.ToInt32(pCod.Value) : -1;

                if (cod != 0)
                {
                    string mesaj = pMesaj.Value?.ToString() ?? "Eroare necunoscută returnată de server.";
                    MessageBox.Show(mesaj, "Eroare validare", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }
        }
        private void ResetEditMode()
        {
            _editGeneral = false;
            pnlGeneralView.Visibility = Visibility.Visible;
            pnlGeneralEdit.Visibility = Visibility.Collapsed;
            btnEditGeneral.Content = BuildButtonContent("Solid_Pen", "Editează", "#4483EC");

            _editSmtp = false;
            pnlSmtpView.Visibility = Visibility.Visible;
            pnlSmtpEdit.Visibility = Visibility.Collapsed;
            btnEditSmtp.Content = BuildButtonContent("Solid_Pen", "Editează", "#4483EC");

            _editApp = false;
            pnlAppView.Visibility = Visibility.Visible;
            pnlAppEdit.Visibility = Visibility.Collapsed;
            btnEditApp.Content = BuildButtonContent("Solid_Pen", "Editează", "#4483EC");
        }

 
        private static StackPanel BuildButtonContent(string icon, string text, string colorHex)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            var color = (System.Windows.Media.Color)
                System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
            var brush = new System.Windows.Media.SolidColorBrush(color);

            sp.Children.Add(new TextBlock
            {
                Text = text,
                Foreground = brush,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            });
            return sp;
        }

        /// <summary>
        /// Găsește ScrollViewer-ul intern al unui TextBox prin VisualTree.
        /// </summary>
        private static System.Windows.Controls.ScrollViewer FindScrollViewer(DependencyObject obj)
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                if (child is System.Windows.Controls.ScrollViewer sv) return sv;
                var result = FindScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}