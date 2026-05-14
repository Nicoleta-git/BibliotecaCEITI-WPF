using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System;

namespace BibliotecaCEITI
{
    public partial class ManualBorrow : UserControl
    {
        public ManualBorrow()
        {
            InitializeComponent();
            SelectCatagories_of_Books();
            PopuleazaGrupe();
        }

        private void BtnInapoi_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.ChangeView(new Borrow());
        }

        private void SelectCatagories_of_Books()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                using (MySqlCommand cmd = new MySqlCommand("sp_categorii_de_manuale", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        List<ComboItem> lista = new List<ComboItem>();
                        foreach (DataRow rand in dt.Rows)
                        {
                            lista.Add(new ComboItem { Denumire = rand["Categorie"].ToString() });
                        }

                        CmbCategorie.ItemsSource = lista;
                        CmbCategorie.DisplayMemberPath = "Denumire";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea categoriilor: " + ex.Message);
            }
        }

        private void CmbGrupa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedGrupa = (CmbGrupa.SelectedItem as ComboItem)?.Denumire;

            if (!string.IsNullOrEmpty(selectedGrupa))
            {
                SelectStudents(selectedGrupa);
                TxtGrupLabel.Text = "Elevi în grupa " + selectedGrupa;
            }
        }

        private void ChkBifeazaToti_Changed(object sender, RoutedEventArgs e)
        {
            var listaElevi = DgElevi.ItemsSource as List<ElevModel>;
            if (listaElevi == null) return;
            foreach (ElevModel elev in listaElevi)
            {
                elev.AreManual = true;
            }
            DgElevi.Items.Refresh();

            int nrSelectati = listaElevi.Count(e => e.AreManual);
            TxtEleviSelectati.Text = "Elevi selectați: " + nrSelectati;
        }

        private void BtnSalveaza_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void CmbCategorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedCategorie = (CmbCategorie.SelectedItem as ComboItem)?.Denumire;

            if (!string.IsNullOrEmpty(selectedCategorie))
            {
                SorteazaManuale_perCategorie(selectedCategorie);
            }
        }

        private void SorteazaManuale_perCategorie(string categorie)
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                using (MySqlCommand cmd = new MySqlCommand("sp_manuale_per_categorie", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_categorie", categorie);

                    conn.Open();

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        List<ComboItem> lista = new List<ComboItem>();
                        foreach (DataRow rand in dt.Rows)
                        {
                            lista.Add(new ComboItem
                            {
                                Id = Convert.ToInt32(rand["ID_carte"]),
                                Denumire = rand["Titlu"].ToString()
                            });
                        }

                        CmbManual.ItemsSource = lista;
                        CmbManual.DisplayMemberPath = "Denumire";
                        CmbManual.SelectedValuePath = "Id";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la sortarea manualelor: " + ex.Message);
            }
        }

        private void PopuleazaGrupe()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                using (MySqlCommand cmd = new MySqlCommand("sp_grupe", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        List<ComboItem> lista = new List<ComboItem>();
                        foreach (DataRow rand in dt.Rows)
                        {
                            lista.Add(new ComboItem { Denumire = rand["Grupa"].ToString() });
                        }

                        CmbGrupa.ItemsSource = lista;
                        CmbGrupa.DisplayMemberPath = "Denumire";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la popularea grupelor: " + ex.Message);
            }
        }

        private void SelectStudents(string grupa)
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                using (MySqlCommand cmd = new MySqlCommand("sp_elevi_per_grupa", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_cod_grupa", grupa);

                    conn.Open();
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        List<ElevModel> listaElevi = new List<ElevModel>();

                        foreach (DataRow rand in dt.Rows)
                        {
                            string nume = rand["Nume"].ToString();
                            string prenume = rand["Prenume"].ToString();
                            string numeComplet = nume + " " + prenume;

                            string initiale = "";
                            if (!string.IsNullOrEmpty(nume) && !string.IsNullOrEmpty(prenume))
                            {
                                initiale = $"{nume[0]}{prenume[0]}".ToUpper();
                            }

                            listaElevi.Add(new ElevModel
                            {
                                NumeElev = numeComplet,
                                Initiale = initiale,
                                AvatarColor = "#4483EC",
                                AreManual = false
                            });
                        }
                        DgElevi.ItemsSource = listaElevi;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea elevilor: " + ex.Message);
            }
        }

        private void areManual_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            var elev = radio?.DataContext as ElevModel;
            if (elev == null) return;

            elev.AreManual = true;

            var listaElevi = DgElevi.ItemsSource as List<ElevModel>;
            if (listaElevi != null)
            {
                int nrSelectati = listaElevi.Count(x => x.AreManual);
                TxtEleviSelectati.Text = "Elevi selectați: " + nrSelectati;
            }
        }


        private void SalveazaEleviManual()
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();

                    foreach (ElevModel elev in (List<ElevModel>)DgElevi.ItemsSource)
                    {
                        using (MySqlCommand cmd = new MySqlCommand("sp_update_elev_manual", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_nume_elev", elev.NumeElev);
                            cmd.Parameters.AddWithValue("@p_are_manual", elev.AreManual);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Elevii au fost actualizați cu succes!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la salvare: " + ex.Message);
            }
        }

    }
}