using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MySql.Data.MySqlClient;


namespace BibliotecaCEITI
{
    public class MonthStat
    {
        public string MonthName { get; set; }
        public double[] Count { get; set; }

        public MonthStat(string name, double total)
        {
            MonthName = name;
            Count = new double[] { total };
        }
    }

    public partial class Dashboard : UserControl
    {
        public ObservableCollection<MonthStat> MonthlyStats { get; set; } = new();

        public Axis[] XAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                IsVisible = false
            }
        };

        public Dashboard()
        {
            InitializeComponent();
            Top3Carti();
            AfiseazaNumarTotal_Disponibile();
            AfiseazaNumarTotal_Imprumuturi();
            AfiseazaNumarTotal_Rezervari();
            AfiseazaNumarTotal_Carti();
        }

        private void AfiseazaNumarTotal_Carti()
        {
            string query = "CALL sp_total_carti();";

            try
            {
                MySqlConnection conn = DatabaseConfig.GetConnection();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                total_carti.Text = dt.Rows[0]["TotalCarti"].ToString();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        private void AfiseazaNumarTotal_Disponibile()
        {
            string query = "CALL sp_total_disponibile();";

            try
            {
                MySqlConnection conn = DatabaseConfig.GetConnection();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                numar_disponibile.Text = dt.Rows[0]["TotalDisponibile"].ToString();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        private void AfiseazaNumarTotal_Imprumuturi()
        {
            string query = "CALL sp_total_imprumuturi();";

            try
            {
                MySqlConnection conn = DatabaseConfig.GetConnection();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                imprumuturi.Text = dt.Rows[0]["TotalImprumuturi"].ToString();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }
        private void AfiseazaNumarTotal_Rezervari()
        {
            string query = "CALL sp_total_rezervari();";

            try
            {
                MySqlConnection conn = DatabaseConfig.GetConnection();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rezervari.Text = dt.Rows[0]["TotalRezervari"].ToString();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        private void Top3Carti()
        {
            using (MySqlConnection conn = DatabaseConfig.GetConnection())
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand("sp_returneaza_top3_carti_imprumutate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 0;
                        while (reader.Read() && index < 3)
                        {
                            string titlu = reader["Carte"].ToString();
                            string autor = reader["Autor"].ToString();
                            int imprumuturi = Convert.ToInt32(reader["Imprumuturi"]);
                            byte[] copertaBytes = reader["Coperta"] == DBNull.Value ? null : (byte[])reader["Coperta"];
                            ImageBrush brush = ConvertToImageBrush(copertaBytes);

                            if (index == 0)
                            {
                                titlu_1.Text = titlu;
                                autor_1.Text = autor;
                                imprumuturi_1.Text = imprumuturi.ToString();
                                coperta_1.Background = brush;
                            }
                            else if (index == 1)
                            {
                                titlu_2.Text = titlu;
                                autor_2.Text = autor;
                                imprumuturi_2.Text = imprumuturi.ToString();
                                coperta_2.Background = brush;
                            }
                            else if (index == 2)
                            {
                                titlu_3.Text = titlu;
                                autor_3.Text = autor;
                                imprumuturi_3.Text = imprumuturi.ToString();
                                coperta_3.Background = brush;
                            }

                            index++;
                        }
                    }
                }
            }
        }

        private ImageBrush ConvertToImageBrush(byte[] imageBytes)
        {
            if (imageBytes != null)
            {
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return new ImageBrush(bitmap);
                }
            } else
            {
                return null;
            }
        }

    }
}