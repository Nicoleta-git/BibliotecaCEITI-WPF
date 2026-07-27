using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Advanced system settings, opened from the SYS button in Settings.xaml.
    /// Every database call goes through a stored procedure: sp_get_setari to load,
    /// sp_salveaza_general / sp_salveaza_smtp / sp_salveaza_app to save each block,
    /// and sp_salveaza_template for the HTML template.
    /// </summary>
    public partial class SystemSettings : UserControl
    {
        private bool _editGeneral = false;
        private bool _editSmtp = false;
        private bool _editApp = false;

        private Dictionary<string, string> _setari = new Dictionary<string, string>();

        public SystemSettings()
        {
            InitializeComponent();
            IncarcaSetari();
        }


        /// <summary>
        /// Calls sp_get_setari and fills the local _setari cache.
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

        private string Get(string cheie, string fallback = "—")
            => _setari.TryGetValue(cheie, out string val) ? val : fallback;

        private void AfiseazaDate()
        {
            lblDenumireInstitutie.Text = Get("denumire_institutie");
            lblAdresa.Text = Get("adresa_bibliotecii");
            lblTelefon.Text = Get("telefon_contact");
            lblEmailBiblioteca.Text = Get("email_biblioteca");
            lblWebsite.Text = Get("website");

            string port = Get("smtp_port", "587");
            string ssl = Get("smtp_ssl", "TLS");
            lblSmtpServer.Text = Get("smtp_server");
            lblSmtpPort.Text = $"{port} ({ssl})";
            lblSmtpEmail.Text = Get("smtp_email_expeditor");
            // password stays masked in view mode

            lblDurataImprumut.Text = Get("durata_imprumut_zile");
            lblMaxImprumuturi.Text = Get("max_imprumuturi_per_elev");
            lblDurataRezervare.Text = Get("durata_rezervare_zile");
            lblDurataManual.Text = Get("durata_manual_luni");
            lblPenalizare.Text = Get("penalizare_per_zi_mdl");
        }

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

        private void BtnEditSmtp_Click(object sender, RoutedEventArgs e)
        {
            _editSmtp = !_editSmtp;

            if (_editSmtp)
            {
                txtSmtpServer.Text = Get("smtp_server");
                txtSmtpPort.Text = Get("smtp_port", "587");
                txtSmtpEmail.Text = Get("smtp_email_expeditor");
                // never prefill the password

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

        // one stored procedure per active section; stops on the first p_cod != 0
        private void BtnSalveazaSistem_Click(object sender, RoutedEventArgs e)
        {
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

                    if (_editApp)
                    {
                        // normalise the decimal separator to a dot
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

                ResetEditMode();
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

        private void BtnVezuCodIntarziere_Click(object sender, RoutedEventArgs e)
            => DeschideTemplateViewer("email_corp_html", "Atenționare întârziere");

        private void BtnEditTemplateDisponibilitate_Click(object sender, RoutedEventArgs e)
        {
            string continut = Get("email_template_disponibilitate", "<!-- Template-ul nu a fost găsit -->");
            var win = new TemplateViewerWindow("Disponibilitate", continut, "email_template_disponibilitate");
            win.ShowDialog();
        }





        private void DeschideTemplateViewer(string cheie, string titlu)
        {
            string continut = Get(cheie, "<!-- Template-ul nu a fost găsit -->");
            var win = new TemplateViewerWindow(titlu, continut, cheie);
            win.ShowDialog();
            // reload the cache in case the template was saved
            IncarcaSetari();
        }

        private void BtnInapoi_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new Settings();
            }
        }

        private static bool ApeleazaSalvare(
            MySqlConnection conn,
            string numeProcedura,
            (string Nume, object Valoare)[] parametriIn)
        {
            using (MySqlCommand cmd = new MySqlCommand(numeProcedura, conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                foreach (var (nume, valoare) in parametriIn)
                    cmd.Parameters.AddWithValue(nume, valoare ?? (object)DBNull.Value);

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
        /// Walks the visual tree to find the ScrollViewer inside a TextBox.
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

        private bool _editTheme = false;

        private void BtnEditTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _editTheme = !_editTheme;

                if (_editTheme)
                {
                    pnlThemeView.Visibility = Visibility.Collapsed;
                    pnlThemeEdit.Visibility = Visibility.Visible;

                    btnEditTheme.Content = BuildButtonContent("Solid_Times", "Anulează", "#E53E3E");

                    pnlThemeEdit.Children.Clear();

                    pnlThemeEdit.Children.Add(new TextBlock
                    {
                        Text = "Alege tematica vizuală direct din butoanele de mai jos:",
                        Margin = new Thickness(0, 0, 0, 10),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A5568")),
                        FontSize = 13,
                        FontWeight = FontWeights.Medium
                    });

                    WrapPanel containerButoane = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Left };

                    var tematici = new Dictionary<string, (string Path, string ColorHex)>
                    {
                        { "Dark Theme",    ("Themes/DarkTheme.xaml",    "#1E1E24") },
                        { "Light Theme",   ("Themes/LightTheme.xaml",   "#4483EC") },
                        { "Emerald Theme", ("Themes/EmeraldTheme.xaml", "#10B981") },
                        { "Red Blue",      ("Themes/RedBlueTheme.xaml", "#E53E3E") }
                    };

                    foreach (var tema in tematici)
                    {
                        Button btnNou = new Button
                        {
                            Width = 150,
                            Height = 38,
                            Margin = new Thickness(0, 0, 12, 8),
                            Cursor = System.Windows.Input.Cursors.Hand,
                            Background = Brushes.White,
                            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C3D0E8")),
                            BorderThickness = new Thickness(1),
                            Tag = tema.Value.Path // Salvăm calea XAML în Tag
                        };

                        btnNou.Content = BuildButtonContent("Solid_Palette", tema.Key, tema.Value.ColorHex);

                        btnNou.Click += ButonSchimbaTema_Click;

                        containerButoane.Children.Add(btnNou);
                    }

                    pnlThemeEdit.Children.Add(containerButoane);
                }
                else
                {
                    pnlThemeView.Visibility = Visibility.Visible;
                    pnlThemeEdit.Visibility = Visibility.Collapsed;
                    btnEditTheme.Content = BuildButtonContent("Solid_Pen", "Editează", "#8B5CF6");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la deschiderea panoului de tematici: {ex.Message}",
                    "Eroare Interfață", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButonSchimbaTema_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btnSelectat && btnSelectat.Tag != null)
            {
                try
                {
                    string themePath = btnSelectat.Tag.ToString(); // Ex: "EmeraldTheme.xaml"
                    var newDict = new ResourceDictionary { Source = new Uri(themePath, UriKind.Relative) };

                    var dictionaries = Application.Current.Resources.MergedDictionaries;
                    bool themeReplaced = false;

                    // find the currently loaded theme dictionary by name
                    for (int i = 0; i < dictionaries.Count; i++)
                    {
                        string sourceStr = dictionaries[i].Source?.OriginalString ?? "";

                        if (sourceStr.Contains("Theme"))
                        {
                            dictionaries[i] = newDict;
                            themeReplaced = true;
                            break;
                        }
                    }

                    if (!themeReplaced)
                    {
                        dictionaries.Add(newDict);
                    }

                    string themeName = themePath.Replace(".xaml", "");
                    Application.Current.Properties["Theme"] = themeName;

                    lblActiveTheme.Text = themeName.Contains("Light") ? "Light Theme" :
                                          themeName.Contains("Dark") ? "Dark Theme" :
                                          themeName.Contains("Emerald") ? "Emerald Theme" : "Red Blue Theme";

                    _editTheme = false;
                    pnlThemeView.Visibility = Visibility.Visible;
                    pnlThemeEdit.Visibility = Visibility.Collapsed;
                    btnEditTheme.Content = BuildButtonContent("Solid_Pen", "Editează", "#8B5CF6");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare la aplicarea fișierului de tematică: {ex.Message}",
                        "Eroare Resurse", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


    }
}
