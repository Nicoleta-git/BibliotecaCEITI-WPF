using System;
using System.Collections.Generic;
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
using SkiaSharp;

namespace BibliotecaCEITI
{
    public partial class Dashboard : UserControl
    {
        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        public Dashboard()
        {
            InitializeComponent();
            this.DataContext = this;

            Top3Carti();
            AfiseazaNumarTotal_Disponibile();
            AfiseazaNumarTotal_Imprumuturi();
            AfiseazaNumarTotal_Rezervari();
            AfiseazaNumarTotal_Carti();

            InitializeMonthlyActivityChart();

            mesaj_de_intrare.Text = "Bun venit, " + SesiuneBibliotecar.NumeBibliotecar + "!";
        }

        private void InitializeMonthlyActivityChart()
        {
            List<double> loanCounts = new List<double>();
            List<string> monthLabels = new List<string>();

            string query = "CALL sp_get_monthly_loan_stats();";

            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    conn.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            loanCounts.Add(Convert.ToDouble(reader["TotalImprumuturi"]));
                            monthLabels.Add(reader["Luna"].ToString());
                        }
                    }
                }
            }
            catch (Exception)
            {
                loanCounts = new List<double> { 45, 82, 63, 94, 110, 78 };
                monthLabels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
            }

            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = loanCounts,
                    Name = "Împrumuturi",
                    Stroke = new SolidColorPaint(SKColor.Parse("#059669")) { StrokeThickness = 3 },
                    Fill = new SolidColorPaint(SKColor.Parse("#059669").WithAlpha(30)),
                    GeometrySize = 8,
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#059669")) { StrokeThickness = 2 },
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#1E293B"))
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = monthLabels,
                    LabelsPaint = new SolidColorPaint(SKColors.DarkGray),
                    SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(20)) { StrokeThickness = 1 }
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColors.DarkGray),
                    SeparatorsPaint = new SolidColorPaint(SKColors.Gray.WithAlpha(20)) { StrokeThickness = 1 }
                }
            };
        }

        private void AfiseazaNumarTotal_Carti()
        {
            string query = "CALL sp_total_carti();";
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    conn.Open();

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                        total_carti.Text = dt.Rows[0]["TotalCarti"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare Cărți Totale: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AfiseazaNumarTotal_Disponibile()
        {
            string query = "CALL sp_total_disponibile();";
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    conn.Open();

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                        numar_disponibile.Text = dt.Rows[0]["TotalDisponibile"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare Disponibile: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AfiseazaNumarTotal_Imprumuturi()
        {
            string query = "CALL sp_total_imprumuturi();";
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    conn.Open();

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                        imprumuturi.Text = dt.Rows[0]["TotalImprumuturi"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare Împrumuturi: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AfiseazaNumarTotal_Rezervari()
        {
            string query = "CALL sp_total_rezervari();";
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    conn.Open();

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                        rezervari.Text = dt.Rows[0]["TotalRezervari"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare Rezervări: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Top3Carti()
        {
            try
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
                                int imprumuturiCount = Convert.ToInt32(reader["Imprumuturi"]);
                                byte[] copertaBytes = reader["Coperta"] == DBNull.Value ? null : (byte[])reader["Coperta"];
                                BitmapImage imagineBitmap = UsefulFunction.ConvertBytesToImage(copertaBytes);
                                ImageBrush brush = imagineBitmap != null ? new ImageBrush(imagineBitmap) : null;

                                if (index == 0)
                                {
                                    titlu_1.Text = titlu;
                                    autor_1.Text = autor;
                                    imprumuturi_1.Text = imprumuturiCount.ToString();
                                    if (brush != null) coperta_1.Background = brush;
                                }
                                else if (index == 1)
                                {
                                    titlu_2.Text = titlu;
                                    autor_2.Text = autor;
                                    imprumuturi_2.Text = imprumuturiCount.ToString();
                                    if (brush != null) coperta_2.Background = brush;
                                }
                                else if (index == 2)
                                {
                                    titlu_3.Text = titlu;
                                    autor_3.Text = autor;
                                    imprumuturi_3.Text = imprumuturiCount.ToString();
                                    if (brush != null) coperta_3.Background = brush;
                                }

                                index++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare Top 3 Cărți: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}