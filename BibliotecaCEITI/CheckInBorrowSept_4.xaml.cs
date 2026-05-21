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

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for CheckInBorrowSept_4.xaml
    /// </summary>
    public partial class CheckInBorrowSept_4 : UserControl
    {
        private int _idElev, _idEx;
        private DateTime _d_impr, _d_return;
        public CheckInBorrowSept_4(int _idElevCurent, int _idExemplarSelectat, DateTime _data_imprumut, DateTime _data_returnarii)
        {
            InitializeComponent();
            _idElev = _idElevCurent;
            _idEx = _idExemplarSelectat;
            _d_impr = _data_imprumut;
            _d_return = _data_returnarii;
            LoadData();
        }

        private void LoadData()
        {
            using (MySqlConnection conn = DatabaseConfig.GetConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand("CALL sp_getData_elev(@p_id);", conn))
                {
                    cmd.Parameters.AddWithValue("@p_id", _idElev);

                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nume_elev.Text = reader["Elev"].ToString();
                            clasa_elev.Text = reader["Grupa"].ToString();
                            id_elev.Text = reader["Id_elev"].ToString();
                            telefon_elev.Text = reader["Telefon"].ToString();
                        }
                    }
                }
            }
            using (MySqlConnection conn = DatabaseConfig.GetConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand("CALL sp_getData_exemplar_carte(@p_id);", conn))
                {
                    cmd.Parameters.AddWithValue("@p_id", _idEx);

                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            titlu_carte.Text = reader["Titlu"].ToString();
                            autor_carte.Text = reader["Autor"].ToString();
                            isbn_carte.Text = reader["ISBN"].ToString();
                        }
                    }
                }
            }
            int zile = (_d_return - _d_impr).Days;
            termen_returnare.Text = _d_return.ToString("dd.MM.yyyy") + " (" + zile + " zile)";
        }

    }
}
