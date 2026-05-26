using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for CheckInBorrowStep_3.xaml
    /// </summary>
    public partial class CheckInBorrowStep_3 : UserControl
    {
        private int _idExemplar;
        private DateTime _data_imprumut, _data_returnare;
        public CheckInBorrowStep_3(int idExemplarSelectat, DateTime? data_impr = null, DateTime? data_retur = null)
        {
            InitializeComponent();
            _idExemplar = idExemplarSelectat;
            LoadDataBook();
            atentionare.Visibility = Visibility.Hidden;

            if (data_impr.HasValue)
            {
                data_imprumut.SelectedDate = data_impr.Value;
            }
            else
            {
                data_imprumut.SelectedDate = DateTime.Now;
            }
        }

        public void InitDates()
        {
            cbDurataImprumut_SelectionChanged(cbDurataImprumut, null);
        }

        private void LoadDataBook()
        {
            int _idCategorie = 10, _idCarte = 0;
            string stare;

            using (MySqlConnection conn = DatabaseConfig.GetConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand("CALL sp_getData_exemplar_carte(@p_id);", conn))
                {
                    cmd.Parameters.AddWithValue("@p_id", _idExemplar);

                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            titlu_carte.Text = reader["Titlu"].ToString();
                            autor_carte.Text = reader["Autor"].ToString();
                            isbn_carte.Text = reader["ISBN"].ToString();
                            cod_inventar.Text = reader["Cod_Inventar"].ToString();
                            stare = reader["Stare"].ToString();

                            if (!int.TryParse(reader["ID_categorie"].ToString(), out _idCategorie))
                            {
                                _idCategorie = 10;
                            }
                            if (!int.TryParse(reader["Id_carte"].ToString(), out _idCarte))
                            {
                                _idCarte = 0;
                            }

                            if (string.IsNullOrEmpty(stare))
                            {
                                disponibilitate_exemplar.Text = "Nespecificat";
                                disponibilitate_exemplar.Foreground = Brushes.Orange;
                                fon_disponibilitate.Background = new SolidColorBrush(Color.FromArgb(30, 255, 165, 0));
                            }
                            else if (stare == "imprumutat" || stare == "împrumutat")
                            {
                                disponibilitate_exemplar.Text = "Împrumutat";
                                disponibilitate_exemplar.Foreground = Brushes.Red;
                                fon_disponibilitate.Background = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0));
                            }
                            else if (stare == "disponibil")
                            {
                                disponibilitate_exemplar.Text = "Disponibilă";
                                disponibilitate_exemplar.Foreground = Brushes.Green;
                                fon_disponibilitate.Background = new SolidColorBrush(Color.FromArgb(30, 34, 197, 94));
                            }
                            else
                            {
                                disponibilitate_exemplar.Text = stare;
                                disponibilitate_exemplar.Foreground = Brushes.Gray;
                                fon_disponibilitate.Background = new SolidColorBrush(Color.FromArgb(30, 128, 128, 128));
                            }
                        }
                    }
                }
            }

            using (MySqlConnection conn = DatabaseConfig.GetConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand("CALL sp_get_categorie(@p_id);", conn))
                {
                    cmd.Parameters.AddWithValue("@p_id", _idCategorie);

                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            denumire_categorie.Text = reader["Nume_Categorie"].ToString();
                        }
                    }
                }
            }
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_imagine_carte", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", _idCarte);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && !reader.IsDBNull(0))
                            {
                                byte[] copertaBytes = (byte[])reader["Imagine"];
                                using (MemoryStream ms = new MemoryStream(copertaBytes))
                                {
                                    BitmapImage bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.StreamSource = ms;
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.EndInit();
                                    bitmap.Freeze();
                                    imgCoperta.Background = new System.Windows.Media.ImageBrush(bitmap);
                                }
                            }
                            else
                            {
                                imgCoperta.Background = null;
                            }
                        }
                    }
                }
            }
            catch
            {
                imgCoperta.Background = null;
            }
        }

        public event Action<DateTime, DateTime> d_imprumut;

        private void cbDurataImprumut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDurataImprumut == null || atentionare == null || atentionare_elev == null || data_imprumut == null)
                return;

            if (!data_imprumut.SelectedDate.HasValue)
                return;

            DateTime _data_imprumut = data_imprumut.SelectedDate.Value;
            DateTime _data_returnare = _data_imprumut;

            if (cbDurataImprumut.SelectedItem != null)
            {
                string selectedText = "";

                if (cbDurataImprumut.SelectedItem is ComboBoxItem item)
                {
                    selectedText = item.Content?.ToString() ?? "";
                }
                else
                {
                    selectedText = cbDurataImprumut.SelectedItem.ToString();
                }

                if (int.TryParse(selectedText.Split(' ')[0], out int zile))
                {
                    _data_returnare = _data_imprumut.AddDays(zile);
                }
            }
            d_imprumut?.Invoke(_data_imprumut, _data_returnare);

            atentionare.Visibility = Visibility.Visible;
            atentionare_elev.Text = $"Elevul este responsabil pentru carte până la data de {_data_returnare:dd.MM.yyyy}";
        }

    }
}
