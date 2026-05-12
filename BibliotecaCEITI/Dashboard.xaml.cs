using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
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

            this.DataContext = this;

            LoadLoanStatistics();
        }

        private void LoadLoanStatistics()
        {
            string connString = "server=localhost;database=biblioteca_ceiti_go;uid=root;pwd=;";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    conn.Open();

                    string query = @"
                        SELECT DATE_FORMAT(data_imprumut, '%M') as MonthLabel, COUNT(*) as TotalLoans 
                        FROM imprumuturi 
                        GROUP BY MONTH(data_imprumut) 
                        ORDER BY data_imprumut ASC";

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        MonthlyStats.Clear();
                        while (reader.Read())
                        {
                            string month = reader["MonthLabel"].ToString();
                            double total = Convert.ToDouble(reader["TotalLoans"]);

                            MonthlyStats.Add(new MonthStat(month, total));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Database Error: " + ex.Message);
            }
        }
    }
}