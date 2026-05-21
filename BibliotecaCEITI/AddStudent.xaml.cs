using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for AddStudent.xaml
    /// </summary>
    public partial class AddStudent : UserControl
    {
        public AddStudent()
        {
            InitializeComponent();
            PopuleazaGrupe();
        }

        private async void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new Students();
            }
        }

        private async void btnSalveazaModificari_Click(object sender, RoutedEventArgs e)
        {
            btnAddStudent.IsEnabled = false;
            int idBibliotecar = 1;
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

                await AddStudentAsync(txtNume.Text, txtPrenume.Text, txtTelefon.Text, txtEmail.Text, txtIdnp.Text, txtObservatii.Text, idGrupa, data_nasterii, idBibliotecar);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A apărut o eroare la salvare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnAddStudent.IsEnabled = true;
            }
        }

        private async Task AddStudentAsync(string nume, string prenume, string telefon, string email, string idnp, string observatii, int idGrupa, DateTime data_nasterii, int idBibliotecar)
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
                        using (MySqlCommand cmd = new MySqlCommand("sp_add_student", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_nume", nume);
                            cmd.Parameters.AddWithValue("@p_prenume", prenume);
                            cmd.Parameters.AddWithValue("@p_telefon", telefon);
                            cmd.Parameters.AddWithValue("@p_email", email);
                            cmd.Parameters.AddWithValue("@p_idnp", idnp);
                            cmd.Parameters.AddWithValue("@p_observatii", observatii);
                            cmd.Parameters.AddWithValue("@p_id_grupa", idGrupa);
                            cmd.Parameters.AddWithValue("@p_data_nasterii", data_nasterii);
                            cmd.Parameters.AddWithValue("@p_creat_de", idBibliotecar);

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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new Students();
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


    }
}
