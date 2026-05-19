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
            MessageBox.Show("Am preluat ID_carte = " + _idExemplarSelectat);
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
            } else if (_idElevCurent != 0 && step == 1)
            {
                CheckInBorrowStep_2 checkIn_2 = new CheckInBorrowStep_2();
                checkIn_2.IdSelected += TakeIdBook;
                ActiveBorrowContent.Content = checkIn_2;
                step = 2;
            } else if (_idExemplarSelectat == 0 && step == 2)
            {
                MessageBox.Show("Te rog să selectezi un exemplar mai întâi!");
                return;
            } else if (_idExemplarSelectat != 0 && step == 2)
            {
                //// CheckInBorrowStep_3 checkIn_3 = new CheckInBorrowStep_3(_idExemplarSelectat);
                //ActiveBorrowContent.Content = checkIn_3;
                //step = 3;
            }
            MessageBox.Show("Suntem la pasul " + step);
        }


    }
}
