using BibliotecaCEITI.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BibliotecaCEITI
{
    public partial class Attention : UserControl
    {
        // ── Date ──────────────────────────────────────────────────────────────
        private List<AttentionItemViewModel> _toateIntarzierile = new ();
        private List<AttentionItemViewModel> _filtrate = new ();

        // ── Paginare ──────────────────────────────────────────────────────────
        private const int ItemsPerPage = 7;
        private int _paginaCurenta = 1;
        private int _totalPagini = 1;

        // ── Paletă avatare ────────────────────────────────────────────────────
        private static readonly (SolidColorBrush bg, SolidColorBrush fg)[] PaletaAvatare =
        {
            (new SolidColorBrush(Color.FromRgb(0xE0,0xE7,0xFF)), new SolidColorBrush(Color.FromRgb(0x62,0x10,0xCC))),
            (new SolidColorBrush(Color.FromRgb(0xDB,0xEA,0xFE)), new SolidColorBrush(Color.FromRgb(0x31,0x82,0xCE))),
            (new SolidColorBrush(Color.FromRgb(0xFC,0xE7,0xF3)), new SolidColorBrush(Color.FromRgb(0xDB,0x27,0x77))),
            (new SolidColorBrush(Color.FromRgb(0xFE,0xF3,0xC7)), new SolidColorBrush(Color.FromRgb(0xD9,0x77,0x06))),
            (new SolidColorBrush(Color.FromRgb(0xDC,0xFC,0xE7)), new SolidColorBrush(Color.FromRgb(0x05,0xCD,0x99))),
        };

        public Attention()
        {
            InitializeComponent();
            Loaded += (s, e) => IncarcaDate();
        }

        // ══════════════════════════════════════════════════════════════════════
        // INCARCARE DATE
        // ══════════════════════════════════════════════════════════════════════
        private void IncarcaDate()
        {
            try
            {
                _toateIntarzierile = CitesteIntarzieri();
                IncarcaClase();
                ActualizeazaStatCarduri();
                AplicaFiltre();
                ActualizeazaPrevizualizare();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea datelor:\n{ex.Message}",
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private List<AttentionItemViewModel> CitesteIntarzieri()
        {
            var lista = new List<AttentionItemViewModel>();
            const string sql = @"
                SELECT
                    i.id                                        AS id_imprumut,
                    i.id_elev,
                    i.id_exemplar,
                    CONCAT(e.prenume, ' ', e.nume)             AS nume_elev,
                    e.email,
                    g.cod                                      AS clasa,
                    CONCAT(LEFT(e.prenume,1), LEFT(e.nume,1))  AS initiale,
                    c.titlu                                    AS titlu_carte,
                    a.nume                                     AS autor_carte,
                    i.termen_returnare,
                    DATEDIFF(CURDATE(), i.termen_returnare)    AS zile_intarziere,
                    i.stare
                FROM imprumuturi i
                JOIN elevi e       ON i.id_elev     = e.id
                JOIN grupe g       ON e.id_grupa    = g.id
                JOIN exemplare ex  ON i.id_exemplar = ex.id
                JOIN carti c       ON ex.id_carte   = c.id
                LEFT JOIN autori a ON c.id_autor    = a.id
                WHERE i.stare IN ('activ','intarziat')
                  AND i.termen_returnare < CURDATE()
                ORDER BY zile_intarziere DESC";

            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                lista.Add(new AttentionItemViewModel
                {
                    IdImprumut = rdr.GetInt32("id_imprumut"),
                    IdElev = rdr.GetInt32("id_elev"),
                    IdExemplar = rdr.GetInt32("id_exemplar"),
                    NumeElev = rdr["nume_elev"]?.ToString() ?? "",
                    Email = rdr["email"]?.ToString() ?? "",
                    Clasa = rdr["clasa"]?.ToString() ?? "",
                    Initiale = rdr["initiale"]?.ToString() ?? "??",
                    TitluCarte = rdr["titlu_carte"]?.ToString() ?? "",
                    AutorCarte = rdr["autor_carte"]?.ToString() ?? "",
                    TermenReturnare = rdr.GetDateTime("termen_returnare"),
                    ZileIntarziere = rdr.GetInt32("zile_intarziere"),
                    Stare = rdr["stare"]?.ToString() ?? "activ",
                    Selectat = true,
                });
            }
            return lista;
        }

        private void IncarcaClase()
        {
            if (cbxClasa == null) return;

            var clase = _toateIntarzierile
                .Select(x => x.Clasa)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            cbxClasa.SelectionChanged -= CbxClasa_SelectionChanged;
            cbxClasa.Items.Clear();
            cbxClasa.Items.Add("Toate clasele");
            foreach (var c in clase)
                cbxClasa.Items.Add(c);
            cbxClasa.SelectedIndex = 0;
            cbxClasa.SelectionChanged += CbxClasa_SelectionChanged;
        }

        private void ActualizeazaStatCarduri()
        {
            if (txtIntarzieri == null || txtBadgeIntarzieri == null ||
                txtNotifTrimise == null || txtRezolvate == null || txtInAsteptare == null)
                return;

            int intarzieri = _toateIntarzierile.Count;
            txtIntarzieri.Text = intarzieri.ToString();
            txtBadgeIntarzieri.Text = $"{intarzieri} întârzieri active";

            int notifTrimise = 0, rezolvate = 0;
            try
            {
                using var conn = DatabaseConfig.GetConnection();
                conn.Open();

                using (var cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM notificari WHERE tip='intarziere'", conn))
                    notifTrimise = Convert.ToInt32(cmd.ExecuteScalar());

                using (var cmd2 = new MySqlCommand(
                    "SELECT COUNT(*) FROM imprumuturi WHERE stare='returnat'", conn))
                    rezolvate = Convert.ToInt32(cmd2.ExecuteScalar());
            }
            catch { /* tabelele pot să nu existe în dev */ }

            txtNotifTrimise.Text = notifTrimise.ToString();
            txtRezolvate.Text = rezolvate.ToString();
            txtInAsteptare.Text = _filtrate.Count(x => x.Selectat).ToString();
        }

        // ══════════════════════════════════════════════════════════════════════
        // FILTRARE
        // ══════════════════════════════════════════════════════════════════════
        private void AplicaFiltre()
        {
            if (txtSearch == null || cbxClasa == null || cbxStatus == null) return;

            string cautare = txtSearch.Text.Trim().ToLower();
            string clasa = cbxClasa.SelectedIndex > 0
                ? cbxClasa.SelectedItem?.ToString() ?? ""
                : "";
            int statusIdx = cbxStatus.SelectedIndex;

            _filtrate = _toateIntarzierile.Where(item =>
            {
                bool matchText = string.IsNullOrEmpty(cautare) ||
                    item.NumeElev.ToLower().Contains(cautare) ||
                    item.TitluCarte.ToLower().Contains(cautare) ||
                    item.Email.ToLower().Contains(cautare);

                bool matchClasa = string.IsNullOrEmpty(clasa) || item.Clasa == clasa;

                bool matchStatus = statusIdx switch
                {
                    1 => item.ZileIntarziere >= 5,
                    2 => item.ZileIntarziere is >= 1 and <= 4,
                    _ => true,
                };

                return matchText && matchClasa && matchStatus;
            }).ToList();

            _paginaCurenta = 1;
            _totalPagini = Math.Max(1, (int)Math.Ceiling(_filtrate.Count / (double)ItemsPerPage));

            RandariazaPagina();
            ActualizeazaPaginator();
            ActualizeazaSelectAll();
            ActualizeazaBtnTrimite();

            if (txtInAsteptare != null)
                txtInAsteptare.Text = _filtrate.Count(x => x.Selectat).ToString();
        }

        // ══════════════════════════════════════════════════════════════════════
        // RANDARE RÂNDURI
        // ══════════════════════════════════════════════════════════════════════
        private void RandariazaPagina()
        {
            if (spRanduri == null || bdPlaceholder == null || txtPaginareInfo == null) return;

            spRanduri.Children.Clear();

            var pagina = _filtrate
                .Skip((_paginaCurenta - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToList();

            bdPlaceholder.Visibility = pagina.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;

            bool alternativ = false;
            foreach (var item in pagina)
            {
                var rand = CreeazaRand(item, alternativ);
                spRanduri.Children.Add(rand);
                alternativ = !alternativ;
            }

            txtPaginareInfo.Text =
                $"Pagina {_paginaCurenta} din {_totalPagini} · {_filtrate.Count} înregistrări";
        }

        private Border CreeazaRand(AttentionItemViewModel item, bool alternativ)
        {
            var rand = new Border
            {
                Background = alternativ ? new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC))
                                         : Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(16, 12, 16, 12),
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Col 0 — CheckBox
            var chk = new CheckBox
            {
                IsChecked = item.Selectat,
                VerticalAlignment = VerticalAlignment.Center,
            };
            chk.Checked += (_, _) => { item.Selectat = true; ActualizeazaBtnTrimite(); };
            chk.Unchecked += (_, _) => { item.Selectat = false; ActualizeazaBtnTrimite(); };
            Grid.SetColumn(chk, 0);

            // Col 1 — Avatar + Nume + Clasă
            var avatarBorder = CreeazaAvatar(item.Initiale);
            var elevPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            elevPanel.Children.Add(avatarBorder);
            var elevInfo = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
            elevInfo.Children.Add(new TextBlock
            {
                Text = item.NumeElev,
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x2D, 0x37, 0x48)),
            });
            elevInfo.Children.Add(new TextBlock
            {
                Text = item.Clasa,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x8A, 0x92, 0xA6)),
            });
            elevPanel.Children.Add(elevInfo);
            Grid.SetColumn(elevPanel, 1);

            // Col 2 — Titlu + Autor
            var cartePanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            cartePanel.Children.Add(new TextBlock
            {
                Text = item.TitluCarte,
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x2D, 0x37, 0x48)),
                TextTrimming = TextTrimming.CharacterEllipsis,
            });
            cartePanel.Children.Add(new TextBlock
            {
                Text = item.AutorCarte,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x8A, 0x92, 0xA6)),
            });
            Grid.SetColumn(cartePanel, 2);

            // Col 3 — Termen returnare
            var txtTermen = new TextBlock
            {
                Text = item.TermenReturnareText,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(0x2D, 0x37, 0x48)),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(txtTermen, 3);

            // Col 4 — Zile întârziere (colorat)
            var zileColor = item.EsteGrav
                ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
                : new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
            var txtZile = new TextBlock
            {
                Text = item.ZileIntarziereText,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = zileColor,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(txtZile, 4);

            // Col 5 — Email
            var txtEmail = new TextBlock
            {
                Text = item.Email,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x8A, 0x92, 0xA6)),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            Grid.SetColumn(txtEmail, 5);

            // Col 6 — Badge status
            var badge = CreeazaBadgeStatus(item.EsteGrav);
            Grid.SetColumn(badge, 6);

            grid.Children.Add(chk);
            grid.Children.Add(elevPanel);
            grid.Children.Add(cartePanel);
            grid.Children.Add(txtTermen);
            grid.Children.Add(txtZile);
            grid.Children.Add(txtEmail);
            grid.Children.Add(badge);

            rand.Child = grid;

            // Hover
            rand.MouseEnter += (_, _) =>
            {
                if (!alternativ) rand.Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF4, 0xFF));
            };
            rand.MouseLeave += (_, _) =>
            {
                rand.Background = alternativ
                    ? new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC))
                    : Brushes.White;
            };

            // Click pe rand selectează item în previzualizare
            rand.MouseLeftButtonUp += (_, _) => ActualizeazaPrevizualizarePentru(item);

            return rand;
        }

        private Border CreeazaAvatar(string initiale)
        {
            int hash = initiale.Length > 0
                ? Math.Abs(initiale[0] * 31 + (initiale.Length > 1 ? initiale[1] : 0))
                : 0;
            var (bg, fg) = PaletaAvatare[hash % PaletaAvatare.Length];

            return new Border
            {
                Width = 34,
                Height = 34,
                CornerRadius = new CornerRadius(17),
                Background = bg,
                Child = new TextBlock
                {
                    Text = initiale.ToUpper(),
                    Foreground = fg,
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
        }

        private Border CreeazaBadgeStatus(bool esteGrav)
        {
            return new Border
            {
                Background = esteGrav
                    ? new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2))
                    : new SolidColorBrush(Color.FromRgb(0xFF, 0xF7, 0xED)),
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(10, 4, 10, 4),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = esteGrav ? "🔴 Critic" : "🟠 Întârziere",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = esteGrav
                        ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
                        : new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)),
                },
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        // PAGINARE
        // ══════════════════════════════════════════════════════════════════════
        private void ActualizeazaPaginator()
        {
            if (spPaginator == null) return;

            spPaginator.Children.Clear();

            var btnPrev = new Button
            {
                Content = "‹",
                Style = (Style)FindResource("PageButtonStyle"),
                IsEnabled = _paginaCurenta > 1,
            };
            btnPrev.Click += (_, _) => { _paginaCurenta--; RandariazaPagina(); ActualizeazaPaginator(); };
            spPaginator.Children.Add(btnPrev);

            for (int p = 1; p <= _totalPagini; p++)
            {
                int pagina = p;
                var btn = new Button
                {
                    Content = p.ToString(),
                    Style = p == _paginaCurenta
                        ? (Style)FindResource("ActivePageButtonStyle")
                        : (Style)FindResource("PageButtonStyle"),
                };
                btn.Click += (_, _) => { _paginaCurenta = pagina; RandariazaPagina(); ActualizeazaPaginator(); };
                spPaginator.Children.Add(btn);
            }

            var btnNext = new Button
            {
                Content = "›",
                Style = (Style)FindResource("PageButtonStyle"),
                IsEnabled = _paginaCurenta < _totalPagini,
            };
            btnNext.Click += (_, _) => { _paginaCurenta++; RandariazaPagina(); ActualizeazaPaginator(); };
            spPaginator.Children.Add(btnNext);
        }

        // ══════════════════════════════════════════════════════════════════════
        // SELECT ALL / BUTON TRIMITE
        // ══════════════════════════════════════════════════════════════════════
        private void ActualizeazaSelectAll()
        {
            if (chkSelectAll == null || txtSelectAll == null) return;

            int total = _filtrate.Count;
            int selectate = _filtrate.Count(x => x.Selectat);

            chkSelectAll.Checked -= ChkSelectAll_Changed;
            chkSelectAll.Unchecked -= ChkSelectAll_Changed;

            chkSelectAll.IsChecked = total > 0 && selectate == total ? true
                                   : selectate == 0 ? false
                                   : (bool?)null;

            chkSelectAll.Checked += ChkSelectAll_Changed;
            chkSelectAll.Unchecked += ChkSelectAll_Changed;

            txtSelectAll.Text = $"Selectează toate ({total})";
        }

        private void ActualizeazaBtnTrimite()
        {
            if (txtBtnTrimite == null || txtInAsteptare == null) return;

            int sel = _filtrate.Count(x => x.Selectat);
            txtBtnTrimite.Text = $"Trimite notificări ({sel})";
            txtInAsteptare.Text = sel.ToString();
        }

        // ══════════════════════════════════════════════════════════════════════
        // PREVIZUALIZARE EMAIL
        // ══════════════════════════════════════════════════════════════════════
        private void ActualizeazaPrevizualizare()
        {
            if (rtbPrevizualizare == null) return;

            var primul = _filtrate.FirstOrDefault();
            if (primul != null)
                ActualizeazaPrevizualizarePentru(primul);
            else
                rtbPrevizualizare.Document.Blocks.Clear();
        }

        private void ActualizeazaPrevizualizarePentru(AttentionItemViewModel item)
        {
            if (rtbPrevizualizare == null) return;

            string subiect = GetSetare("email_subiect");
            string corpHtml = GetSetare("email_corp_html");

            if (string.IsNullOrEmpty(subiect))
                subiect = $"[Bibliotecă] Returnare carte: {item.TitluCarte}";
            if (string.IsNullOrEmpty(corpHtml))
                corpHtml = $"Stimate/ă {item.NumeElev},\n\n" +
                           $"Vă reamintim că ați împrumutat cartea \"{item.TitluCarte}\" de {item.AutorCarte}.\n" +
                           $"Termenul de returnare a fost {item.TermenReturnareText} " +
                           $"(acum {item.ZileIntarziereText}).\n\n" +
                           "Vă rugăm să returnați cartea cât mai curând.\n\nBiblioteca CEITI";

            string corp = corpHtml
                .Replace("{nume_elev}", item.NumeElev)
                .Replace("{titlu_carte}", item.TitluCarte)
                .Replace("{autor_carte}", item.AutorCarte)
                .Replace("{termen_returnare}", item.TermenReturnareText)
                .Replace("{zile_intarziere}", item.ZileIntarziereText);

            var doc = new FlowDocument();
            doc.Blocks.Add(new Paragraph(new Bold(new Run($"Subiect: {subiect}")))
            {
                Margin = new Thickness(0, 0, 0, 8),
            });
            doc.Blocks.Add(new Paragraph(new Run("─────────────────────────────────────"))
            {
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0)),
            });

            string textCurat = corp
                .Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n")
                .Replace("&nbsp;", " ");
            while (textCurat.Contains('<') && textCurat.Contains('>'))
            {
                int start = textCurat.IndexOf('<');
                int end = textCurat.IndexOf('>', start);
                if (end < 0) break;
                textCurat = textCurat.Remove(start, end - start + 1);
            }

            foreach (var linie in textCurat.Split('\n'))
            {
                doc.Blocks.Add(new Paragraph(new Run(linie))
                {
                    Margin = new Thickness(0, 1, 0, 1),
                    FontSize = 12,
                });
            }

            rtbPrevizualizare.Document = doc;
        }

        // ══════════════════════════════════════════════════════════════════════
        // TRIMITERE EMAIL
        // ══════════════════════════════════════════════════════════════════════
        private async void BtnTrimite_Click(object sender, RoutedEventArgs e)
        {
            if (overlayLoading == null || btnTrimite == null) return;

            var deTrimes = _filtrate.Where(x => x.Selectat).ToList();
            if (deTrimes.Count == 0)
            {
                MessageBox.Show("Selectați cel puțin o atenționare pentru trimitere.",
                    "Atenție", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmare = MessageBox.Show(
                $"Urmează să trimiteți {deTrimes.Count} email(uri). Continuați?",
                "Confirmare trimitere", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmare != MessageBoxResult.Yes) return;

            overlayLoading.Visibility = Visibility.Visible;
            btnTrimite.IsEnabled = false;

            int trimise = 0, erori = 0;

            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    string smtpServer = GetSetare("smtp_server");
                    string smtpUser = GetSetare("smtp_user");
                    string smtpPass = GetSetare("smtp_password");
                    int smtpPort = int.TryParse(GetSetare("smtp_port"), out int p) ? p : 587;

                    if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUser))
                    {
                        Dispatcher.Invoke(() =>
                            MessageBox.Show(
                                "Configurația SMTP nu este completă în tabelul 'setari'.\n" +
                                "Verificați cheile: smtp_server, smtp_port, smtp_user, smtp_password.",
                                "Configurație SMTP lipsă", MessageBoxButton.OK, MessageBoxImage.Warning));
                        return;
                    }

                    using var smtp = new SmtpClient();
                    smtp.Connect(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    smtp.Authenticate(smtpUser, smtpPass);

                    foreach (var item in deTrimes)
                    {
                        try
                        {
                            var mesaj = ConstruiesteEmail(item, smtpUser);
                            smtp.Send(mesaj);
                            SalveazaNotificareInDb(item, mesaj.Subject);
                            trimise++;
                        }
                        catch { erori++; }
                    }

                    smtp.Disconnect(true);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                        MessageBox.Show($"Eroare SMTP:\n{ex.Message}", "Eroare",
                            MessageBoxButton.OK, MessageBoxImage.Error));
                }
            });

            overlayLoading.Visibility = Visibility.Collapsed;
            btnTrimite.IsEnabled = true;

            if (trimise > 0 || erori > 0)
            {
                MessageBox.Show(
                    $"✔ {trimise} email(uri) trimise cu succes!" +
                    (erori > 0 ? $"\n✘ {erori} email(uri) nu au putut fi trimise." : ""),
                    "Rezultat trimitere", MessageBoxButton.OK, MessageBoxImage.Information);

                IncarcaDate();
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Înlocuiește metoda existentă ConstruiesteEmail din proiectul tău.
        // Nu sunt necesare alte modificări — logica de trimitere rămâne identică.
        // ─────────────────────────────────────────────────────────────────────────────

        private MimeMessage ConstruiesteEmail(AttentionItemViewModel item, string expeditorEmail)
        {
            string subiectTemplate = GetSetare("email_subiect");
            string corpTemplate = GetSetare("email_corp_html");

            if (string.IsNullOrEmpty(subiectTemplate))
                subiectTemplate = "Atenționare returnare: {titlu_carte}";

            string logoUrl = GetSetare("email_logo_url");
            if (string.IsNullOrEmpty(logoUrl))
                logoUrl = "https://ceiti.md/wp-content/uploads/2017/02/logo_light-1.png";

            string subiect = subiectTemplate
                .Replace("{titlu_carte}", item.TitluCarte)
                .Replace("{nume_elev}", item.NumeElev);

            string corp = corpTemplate
                .Replace("{logo_url}", logoUrl)
                .Replace("{nume_elev}", item.NumeElev)
                .Replace("{titlu_carte}", item.TitluCarte)
                .Replace("{autor_carte}", item.AutorCarte)
                .Replace("{termen_returnare}", item.TermenReturnareText)
                .Replace("{zile_intarziere}", item.ZileIntarziereText)
                .Replace("{an_curent}", DateTime.Now.Year.ToString());

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("Biblioteca CEITI", expeditorEmail));
            msg.To.Add(new MailboxAddress(item.NumeElev, item.Email));
            msg.Subject = subiect;
            msg.Body = new TextPart("html") { Text = corp };

            return msg;
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPER: Escapare HTML minimă (anti-XSS dacă numele conține caractere speciale)
        // ══════════════════════════════════════════════════════════════════════
        private static string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        private void SalveazaNotificareInDb(AttentionItemViewModel item, string mesaj)
        {
            try
            {
                using var conn = DatabaseConfig.GetConnection();
                conn.Open();
                using var cmd = new MySqlCommand(
                    "INSERT INTO notificari (id_elev, id_imprumut, tip, mesaj, citita) " +
                    "VALUES (@id_elev, @id_imprumut, 'intarziere', @mesaj, 0)", conn);
                cmd.Parameters.AddWithValue("@id_elev", item.IdElev);
                cmd.Parameters.AddWithValue("@id_imprumut", item.IdImprumut);
                cmd.Parameters.AddWithValue("@mesaj", mesaj);
                cmd.ExecuteNonQuery();
            }
            catch { /* log dacă există logger */ }
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPER — CITIRE SETARI DIN DB
        // ══════════════════════════════════════════════════════════════════════
        private string GetSetare(string cheie)
        {
            try
            {
                using var conn = DatabaseConfig.GetConnection();
                conn.Open();
                using var cmd = new MySqlCommand(
                    "SELECT valoare FROM setari WHERE cheie = @cheie", conn);
                cmd.Parameters.AddWithValue("@cheie", cheie);
                return cmd.ExecuteScalar()?.ToString() ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        // ══════════════════════════════════════════════════════════════════════
        // EVENT HANDLERS
        // ══════════════════════════════════════════════════════════════════════
        private void BtnReincarca_Click(object sender, RoutedEventArgs e) => IncarcaDate();

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => AplicaFiltre();

        private void CbxClasa_SelectionChanged(object sender, SelectionChangedEventArgs e) => AplicaFiltre();

        private void CbxStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) => AplicaFiltre();

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            cbxClasa.SelectionChanged -= CbxClasa_SelectionChanged;
            cbxStatus.SelectionChanged -= CbxStatus_SelectionChanged;

            txtSearch.Text = "";
            cbxClasa.SelectedIndex = 0;
            cbxStatus.SelectedIndex = 0;

            cbxClasa.SelectionChanged += CbxClasa_SelectionChanged;
            cbxStatus.SelectionChanged += CbxStatus_SelectionChanged;

            AplicaFiltre();
        }

        private void ChkSelectAll_Changed(object sender, RoutedEventArgs e)
        {
            bool selectat = chkSelectAll.IsChecked == true;
            foreach (var item in _filtrate)
                item.Selectat = selectat;
            RandariazaPagina();
            ActualizeazaBtnTrimite();
        }

        private void BtnPrevizualizeaza_Click(object sender, RoutedEventArgs e)
        {
            var primul = _filtrate.FirstOrDefault(x => x.Selectat) ?? _filtrate.FirstOrDefault();
            if (primul != null)
                ActualizeazaPrevizualizarePentru(primul);
        }

        private void BtnAnuleaza_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _filtrate)
                item.Selectat = false;
            RandariazaPagina();
            ActualizeazaSelectAll();
            ActualizeazaBtnTrimite();
        }
    }
}