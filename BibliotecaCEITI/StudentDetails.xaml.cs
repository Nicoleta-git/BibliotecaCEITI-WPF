using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for StudentDetails.xaml
    /// </summary>
    public partial class StudentDetails : UserControl
    {
        private int _idElev;
        public StudentDetails(int id_elevSelectat = 0)
        {
            InitializeComponent();
            _idElev = id_elevSelectat;
            PopuleazaGrupe();
            LoadDataStudent();
            LoadBorrowedBooks();
            LoadReservedBooks();

            if (string.IsNullOrEmpty(txtObservatii.Text))
            {
                txtObservatii.Text = "Scrie aici observații generale despre elev...";
                txtObservatii.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void PopuleazaGrupe()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand("sp_populeaza_grupe", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        conn.Open();

                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            List<ComboItem> lista = new List<ComboItem>();
                            lista.Add(new ComboItem { Id = -1, Denumire = "Selectează grupa..." });

                            foreach (DataRow rand in dt.Rows)
                            {
                                ComboItem item = new ComboItem
                                {
                                    Id = Convert.ToInt32(rand["ID_grupa"]),
                                    Denumire = rand["Grupa"].ToString()
                                };
                                lista.Add(item);
                            }

                            cbGrupe.ItemsSource = lista;
                            cbGrupe.DisplayMemberPath = "Denumire";
                            cbGrupe.SelectedValuePath = "Id";

                            if (lista.Count > 0)
                            {
                                cbGrupe.SelectedIndex = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la popularea grupelor: " + ex.Message, "Eroare SQL");
            }
        }

        private async void btnSalveazaModificari_Click(object sender, RoutedEventArgs e)
        {
            btnEditStudent.IsEnabled = false;

            try
            {
                if (dpDataNasterii.SelectedDate == null)
                {
                    MessageBox.Show("Te rog să selectezi data nașterii!", "Atenție");
                    return;
                }
                DateTime data_nasterii = dpDataNasterii.SelectedDate.Value;

                if (cbGrupe.SelectedValue == null || Convert.ToInt32(cbGrupe.SelectedValue) == -1)
                {
                    MessageBox.Show("Te rog să selectezi o grupă validă din listă!", "Atenție");
                    return;
                }
                int idGrupa = Convert.ToInt32(cbGrupe.SelectedValue);

                await UpdateStdentData(_idElev, txtNume.Text, txtPrenume.Text, txtTelefon.Text, txtEmail.Text, txtIdnp.Text, txtObservatii.Text, idGrupa, data_nasterii);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare la salvare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnEditStudent.IsEnabled = true;
            }
        }

        private async Task UpdateStdentData(int idElev, string nume, string prenume, string telefon, string email, string idnp, string observatii, int idGrupa, DateTime data_nasterii)
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
                        using (MySqlCommand cmd = new MySqlCommand("sp_edit_student", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_id", idElev);
                            cmd.Parameters.AddWithValue("@p_nume", nume);
                            cmd.Parameters.AddWithValue("@p_prenume", prenume);
                            cmd.Parameters.AddWithValue("@p_telefon", telefon);
                            cmd.Parameters.AddWithValue("@p_email", email);
                            cmd.Parameters.AddWithValue("@p_idnp", idnp);
                            cmd.Parameters.AddWithValue("@p_observatii", observatii);
                            cmd.Parameters.AddWithValue("@p_id_grupa", idGrupa);
                            cmd.Parameters.AddWithValue("@p_data_nasterii", data_nasterii);

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
                    mainWindow?.ChangeView(new Students());
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

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new Students();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new Students();
            }
        }

        private void LoadDataStudent()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_selecteaza_elev", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", _idElev);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtNume.Text = reader["Nume"].ToString();
                                txtPrenume.Text = reader["Prenume"].ToString();
                                txtIdnp.Text = reader["IDNP"].ToString();
                                txtObservatii.Text = reader["Observatii"] != DBNull.Value ? reader["Observatii"].ToString() : "";
                                txtTelefon.Text = reader["Telefon"].ToString();
                                txtEmail.Text = reader["Email"].ToString();
                                cbGrupe.SelectedValue = reader["ID_grupa"] != DBNull.Value ? Convert.ToInt32(reader["ID_grupa"]) : -1;
                                dpDataNasterii.Text = reader["Data_nasterii"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea detaliilor studentului: " + ex.Message);
            }
        }

        private async Task LoadBorrowedBooks()
        {
            try
            {
                DataTable dt = await Task.Run(() =>
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("sp_borrowed_books", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", _idElev);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable tempDt = new DataTable();
                        da.Fill(tempDt);
                        return tempDt;
                    }
                });

                BorrowedBooks.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Eroare la încărcarea împrumuturilor: " + ex.Message);
            }
        }

        private async Task LoadReservedBooks()
        {
            try
            {
                DataTable dt = await Task.Run(() =>
                {
                    using (MySqlConnection conn = DatabaseConfig.GetConnection())
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("sp_reserved_books", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", _idElev);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable tempDt = new DataTable();
                        da.Fill(tempDt);
                        return tempDt;
                    }
                });

                ReservedBooks.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Eroare la încărcarea rezervărilor: " + ex.Message);
            }
        }

        private void txtIdnp_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Ex: 1234567890123...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtIdnp_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Ex: 1234567890123...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void txtNume_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Ex: Moraru...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtNume_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Ex: Moraru...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void txtPrenume_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Ex: Vasile...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtPrenume_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Ex: Vasile...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void txtTelefon_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Ex: 012345678...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtTelefon_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Ex: 012345678...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void txtEmail_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Ex: moraru.vasile@gmail.com...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Ex: moraru.vasile@gmail.com...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void txtObservatii_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.Text == "Scrie aici observații generale despre elev...")
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void txtObservatii_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = "Scrie aici observații generale despre elev...";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }

        private void txtIdnp_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtTelefon_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
