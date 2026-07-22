using MySql.Data.MySqlClient;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Fereastră editor HTML pentru template-urile de email.
    /// Salvarea se face exclusiv prin procedura stocată sp_salveaza_template,
    /// care validează cheia pe o listă albă (whitelist) înainte de INSERT/UPDATE.
    /// </summary>
    public partial class TemplateViewerWindow : Window
    {
        private readonly string _cheie;
        private bool _modificat = false;

        // Flag care permite închiderea reală după ce animația termină
        private bool _closeAnimationDone = false;

        public TemplateViewerWindow(string titlu, string continut, string cheie)
        {
            InitializeComponent();

            _cheie = cheie;

            string numeFisier = cheie.Replace("_html", ".html").Replace("_", "-");
            txtTitluFereastra.Text = numeFisier;
            txtTabName.Text = numeFisier;
            txtTitluCentru.Text = $"Cod template: {titlu}";
            txtBreadcrumb.Text = titlu;

            codeBox.Text = continut;
            _modificat = false;

            ActualizeazaNumereLinii();
            ActualizeazaStatusBar();
        }

        // ══════════════════════════════════════════════════════════════
        // ANIMAȚIE ÎNCHIDERE
        // ══════════════════════════════════════════════════════════════

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_closeAnimationDone)
            {
                e.Cancel = true;
                RunCloseAnimation();
                return;
            }
            base.OnClosing(e);
        }

        private void RunCloseAnimation()
        {
            RootBorder.CacheMode = new BitmapCache { EnableClearType = true, RenderAtScale = 1 };

            var duration = new Duration(TimeSpan.FromMilliseconds(180));
            var ease = new QuarticEase { EasingMode = EasingMode.EaseIn };

            var transformGroup = RootBorder.RenderTransform as TransformGroup;
            var scale = transformGroup?.Children[0] as ScaleTransform;
            var translate = transformGroup?.Children[1] as TranslateTransform;

            var fadeOut = new DoubleAnimation(1, 0, duration) { EasingFunction = ease };
            var slideDown = new DoubleAnimation(0, 18, duration) { EasingFunction = ease };
            var scaleDownX = new DoubleAnimation(1, 0.96, duration) { EasingFunction = ease };
            var scaleDownY = new DoubleAnimation(1, 0.96, duration) { EasingFunction = ease };

            fadeOut.Completed += (s, args) =>
            {
                RootBorder.CacheMode = null;
                _closeAnimationDone = true;
                Close();
            };

            Timeline.SetDesiredFrameRate(fadeOut, 160);
            Timeline.SetDesiredFrameRate(slideDown, 160);
            Timeline.SetDesiredFrameRate(scaleDownX, 160);
            Timeline.SetDesiredFrameRate(scaleDownY, 160);

            this.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            translate?.BeginAnimation(TranslateTransform.YProperty, slideDown);
            scale?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleDownX);
            scale?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleDownY);
        }

        // ══════════════════════════════════════════════════════════════
        // TITLE BAR — drag + butoane fereastră
        // ══════════════════════════════════════════════════════════════
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                ToggleMaximize();
            else
                DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
            => ToggleMaximize();

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_modificat)
            {
                var result = MessageBox.Show(
                    "Ai modificări nesalvate. Vrei să închizi fără să salvezi?",
                    "Atenție", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }

            _closeAnimationDone = false;
            Close();
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        // ══════════════════════════════════════════════════════════════
        // EDITOR — events
        // ══════════════════════════════════════════════════════════════
        private void CodeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _modificat = true;
            dotUnsaved.Visibility = Visibility.Visible;
            ActualizeazaNumereLinii();
            ActualizeazaStatusBar();
        }

        private void CodeBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var sv = FindScrollViewer(lineNumberBox);
            sv?.ScrollToVerticalOffset(e.VerticalOffset);
        }

        // ══════════════════════════════════════════════════════════════
        // BUTOANE FOOTER
        // ══════════════════════════════════════════════════════════════
        private void BtnAnuleaza_Click(object sender, RoutedEventArgs e)
        {
            if (_modificat)
            {
                var result = MessageBox.Show(
                    "Ai modificări nesalvate. Vrei să anulezi?",
                    "Atenție", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }
            _closeAnimationDone = false;
            Close();
        }

        /// <summary>
        /// Salvează template-ul prin sp_salveaza_template.
        /// Procedura validează cheia pe o whitelist server-side
        /// și returnează p_cod / p_mesaj pentru tratarea erorilor.
        /// </summary>
        private void BtnSalveaza_Click(object sender, RoutedEventArgs e)
        {
            btnSalveaza.IsEnabled = false;
            btnSalveaza.Opacity = 0.7;

            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand("sp_salveaza_template", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        // Parametri IN
                        cmd.Parameters.AddWithValue("p_cheie", _cheie);
                        cmd.Parameters.AddWithValue("p_valoare", codeBox.Text);

                        // Parametri OUT
                        var pCod = new MySqlParameter("p_cod", MySqlDbType.Int32)
                        { Direction = System.Data.ParameterDirection.Output };
                        var pMesaj = new MySqlParameter("p_mesaj", MySqlDbType.VarChar, 255)
                        { Direction = System.Data.ParameterDirection.Output };

                        cmd.Parameters.Add(pCod);
                        cmd.Parameters.Add(pMesaj);

                        cmd.ExecuteNonQuery();

                        int cod = pCod.Value != DBNull.Value ? Convert.ToInt32(pCod.Value) : -1;
                        string mesaj = pMesaj.Value?.ToString() ?? "Eroare necunoscută.";

                        // Procedura a returnat o eroare de validare
                        if (cod != 0)
                        {
                            MessageBox.Show(mesaj, "Eroare validare",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }

                // Succes
                _modificat = false;
                dotUnsaved.Visibility = Visibility.Collapsed;

                MessageBox.Show("Template salvat cu succes!", "Succes",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Închide cu animație după salvare
                _closeAnimationDone = false;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la salvare: " + ex.Message,
                    "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnSalveaza.IsEnabled = true;
                btnSalveaza.Opacity = 1;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════
        private void ActualizeazaNumereLinii()
        {
            int linii = codeBox.LineCount > 0 ? codeBox.LineCount : 1;
            var sb = new StringBuilder();
            for (int i = 1; i <= linii; i++)
                sb.AppendLine(i.ToString());
            lineNumberBox.Text = sb.ToString().TrimEnd();
            txtTotalLinii.Text = $"{linii} linii";
        }

        private void ActualizeazaStatusBar()
        {
            int caretIndex = codeBox.CaretIndex;
            int linie = codeBox.GetLineIndexFromCharacterIndex(caretIndex) + 1;
            int coloana = caretIndex - codeBox.GetCharacterIndexFromLineIndex(linie - 1) + 1;
            txtLinie.Text = $"Ln {linie}, Col {coloana}";
        }

        private static ScrollViewer FindScrollViewer(DependencyObject obj)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is ScrollViewer sv) return sv;
                var result = FindScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}