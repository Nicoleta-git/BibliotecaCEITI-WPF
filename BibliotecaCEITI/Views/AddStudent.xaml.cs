using System;
using System.Collections.Generic;
using System.Data;
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
using MySql.Data.MySqlClient;

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

            txtIdnp.Text = (string)Application.Current.FindResource("Placeholder_IDNP");
            txtIdnp.Foreground = new SolidColorBrush(Colors.Gray);
            txtNume.Text = (string)Application.Current.FindResource("Placeholder_LastName");
            txtNume.Foreground = new SolidColorBrush(Colors.Gray);
            txtPrenume.Text = (string)Application.Current.FindResource("Placeholder_FirstName");
            txtPrenume.Foreground = new SolidColorBrush(Colors.Gray);
            txtTelefon.Text = (string)Application.Current.FindResource("Placeholder_PhoneShort");
            txtTelefon.Foreground = new SolidColorBrush(Colors.Gray);
            txtEmail.Text = (string)Application.Current.FindResource("Placeholder_Email");
            txtEmail.Foreground = new SolidColorBrush(Colors.Gray);
            txtObservatii.Text = (string)Application.Current.FindResource("Placeholder_StudentNotes");
            txtObservatii.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private async void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContentContainer.Content = new Students();
            }
        }

        private bool ValidateTextBox(TextBox textBox, string placeholder, string errorMessage)
        {
            if (textBox.Text == placeholder || string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show(errorMessage, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                textBox.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                return false;
            }
            textBox.ClearValue(TextBox.BorderBrushProperty);
            return true;
        }

        private bool ValidateComboBox(ComboBox comboBox, string errorMessage)
        {
            if (comboBox.SelectedIndex == 0)
            {
                MessageBox.Show(errorMessage, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                comboBox.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                return false;
            }
            comboBox.ClearValue(ComboBox.BorderBrushProperty);
            return true;
        }

        private bool ValidateDateTimePicker(DatePicker datePicker, string errorMesage)
        {
            if (datePicker.SelectedDate == null)
            {
                MessageBox.Show(errorMesage, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                datePicker.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
                return false;
            }
            datePicker.ClearValue(DatePicker.BorderBrushProperty);
            return true;
        }

        private async void btnSalveazaModificari_Click(object sender, RoutedEventArgs e)
        {
            btnAddStudent.IsEnabled = false;
            int idBibliotecar = SesiuneBibliotecar.IdBibliotecarCurent;
            try
            {
                if (!ValidateTextBox(txtIdnp, (string)Application.Current.FindResource("Placeholder_IDNP"), "Vă rugăm să introduceți IDNP-ul cărții.")) return;
                if (!ValidateComboBox(cbGrupe, "Vă rugăm să selectați grupa")) return;
                if (!ValidateTextBox(txtNume, (string)Application.Current.FindResource("Placeholder_LastName"), "Vă rugăm să introduceți numele elevului.")) return;
                if (!ValidateTextBox(txtPrenume, (string)Application.Current.FindResource("Placeholder_FirstName"), "Vă rugăm să introduceți prenumele elevului.")) return;
                if (!ValidateTextBox(txtTelefon, (string)Application.Current.FindResource("Placeholder_PhoneShort"), "Vă rugăm să introduceți un număr de telefon.")) return;
                if (!ValidateTextBox(txtEmail, (string)Application.Current.FindResource("Placeholder_Email"), "Vă rugăm să introduceți o adresă de email.")) return;
                if (!ValidateDateTimePicker(dpDataNasterii, "Vă rugăm să selectați data nașterii.")) return;

                DateTime data_nasterii = dpDataNasterii.SelectedDate.Value;
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
                            lista.Add(new ComboItem { Id = -1, Denumire = (string)Application.Current.FindResource("Loans_SelectGroupPlaceholder") });

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

        private void txtIdnp_GotFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.GotFocus(sender, (string)Application.Current.FindResource("Placeholder_IDNP"));
        }

        private void txtIdnp_LostFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.LostFocus(sender, (string)Application.Current.FindResource("Placeholder_IDNP"));
        }

        private void txtNume_GotFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.GotFocus(sender, (string)Application.Current.FindResource("Placeholder_LastName"));
        }

        private void txtNume_LostFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.LostFocus(sender, (string)Application.Current.FindResource("Placeholder_LastName"));
        }

        private void txtPrenume_GotFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.GotFocus(sender, (string)Application.Current.FindResource("Placeholder_FirstName"));
        }

        private void txtPrenume_LostFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.LostFocus(sender, (string)Application.Current.FindResource("Placeholder_FirstName"));
        }

        private void txtTelefon_GotFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.GotFocus(sender, (string)Application.Current.FindResource("Placeholder_PhoneShort"));
        }

        private void txtTelefon_LostFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.LostFocus(sender, (string)Application.Current.FindResource("Placeholder_PhoneShort"));
        }

        private void txtEmail_GotFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.GotFocus(sender, (string)Application.Current.FindResource("Placeholder_Email"));
        }

        private void txtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.LostFocus(sender, (string)Application.Current.FindResource("Placeholder_Email"));
        }

        private void txtObservatii_GotFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.GotFocus(sender, (string)Application.Current.FindResource("Placeholder_StudentNotes"));
        }

        private void txtObservatii_LostFocus(object sender, RoutedEventArgs e)
        {
            UsefulFunction.LostFocus(sender, (string)Application.Current.FindResource("Placeholder_StudentNotes"));
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
