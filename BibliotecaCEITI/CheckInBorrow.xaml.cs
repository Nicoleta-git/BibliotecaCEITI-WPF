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
    /// Interaction logic for CheckInBorrow.xaml
    /// </summary>
    ///

    public partial class CheckInBorrow : UserControl
    {
        private int _idElevCurent = 0, _idExemplarSelectat = 0;
        private int step = 1;
        private DateTime? _data_imprumut = null, _data_returnarii = null;
        public CheckInBorrow()
        {
            InitializeComponent();

            CheckInBorrowStep_1 checkIn_1 = new CheckInBorrowStep_1();
            checkIn_1.IdSelected += LoadStudentData;

            ActiveBorrowContent.Content = checkIn_1;
        }

        private void TakeIdBook(int idSelectat)
        {
            _idExemplarSelectat = idSelectat;
        }

        private void TakeDates(DateTime data_imprumut, DateTime data_returnarii)
        {
            _data_imprumut = data_imprumut;
            _data_returnarii = data_returnarii;
        }

        private async void LoadStudentData(int idSelectat)
        {
            _idElevCurent = idSelectat;
            try
            {
                DataTable dt = await Task.Run(() =>
                {
                    DataTable tempTable = new DataTable();
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_selecteaza_imprumuturi", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id", idSelectat == 0 ? (object)DBNull.Value : idSelectat);

                            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                            {
                                da.Fill(tempTable);
                            }
                        }
                    }
                    return tempTable;
                });

                BooksGrid.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea datelor: " + ex.Message);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.ChangeView(new Borrow());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.ChangeView(new Borrow());
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_idElevCurent == 0 && step == 1)
            {
                MessageBox.Show("Te rog să selectezi un elev mai întâi!");
                return;
            }
            else if (_idElevCurent != 0 && step == 1)
            {
                CheckInBorrowStep_2 checkIn_2 = new CheckInBorrowStep_2();
                checkIn_2.IdSelected += TakeIdBook;
                ActiveBorrowContent.Content = checkIn_2;
                step = 2;
            }
            else if (_idExemplarSelectat == 0 && step == 2)
            {
                MessageBox.Show("Te rog să selectezi un exemplar mai întâi!");
                return;
            }
            else if (_idExemplarSelectat != 0 && step == 2)
            {
                CheckInBorrowStep_3 checkIn_3 = new CheckInBorrowStep_3(_idExemplarSelectat);
                checkIn_3.d_imprumut += TakeDates;
                checkIn_3.InitDates();
                ActiveBorrowContent.Content = checkIn_3;
                step = 3;
            }
            else if ((_data_imprumut == null || _data_returnarii == null) && step == 3)
            {
                MessageBox.Show("Nu ai selectat data de împrumut sau termenul de returnare.");
                return;
            }
            else if (_data_imprumut != null && _data_returnarii != null && step == 3)
            {
                CheckInBorrowStep_4 checkIn_4 = new CheckInBorrowStep_4(_idElevCurent, _idExemplarSelectat, _data_imprumut.Value, _data_returnarii.Value);

                ActiveBorrowContent.Content = checkIn_4;
                step = 4;
                salveaza.Text = "Salvează";
            } else if (step == 4 && salveaza.Text == "Salvează")
            {
                SalveazaImprumut();
            }
        }

        private async void SalveazaImprumut()
        {
            try
            {
                int succes = 0;
                string mesaj = "";

                await Task.Run(() =>
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_imprumut_carte", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id_elev", _idElevCurent);
                            cmd.Parameters.AddWithValue("@p_id_exemplar", _idExemplarSelectat);
                            cmd.Parameters.AddWithValue("@p_data_imprumut", _data_imprumut.Value.Date);
                            cmd.Parameters.AddWithValue("@p_termen", _data_returnarii.Value.Date);
                            cmd.Parameters.AddWithValue("@p_id_bibliotecar", 1);

                            MySqlParameter paramSucces = new MySqlParameter("@p_succes", MySqlDbType.Byte);
                            paramSucces.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramSucces);

                            MySqlParameter paramMesaj = new MySqlParameter("@p_mesaj", MySqlDbType.VarChar, 255);
                            paramMesaj.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(paramMesaj);

                            cmd.ExecuteNonQuery();

                            succes = Convert.ToInt32(paramSucces.Value);
                            mesaj = paramMesaj.Value?.ToString() ?? "";
                        }
                    }
                });

                if (succes == 1)
                {
                    MessageBox.Show(mesaj);

                    var mainWindow = Window.GetWindow(this) as MainWindow;
                    mainWindow?.ChangeView(new Borrow());
                }
                else
                {
                    MessageBox.Show("Eroare: " + mesaj);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare: " + ex.Message);
            }
        }
    }
}
