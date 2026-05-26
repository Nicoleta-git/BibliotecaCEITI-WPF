using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BibliotecaCEITI
{
    public partial class ManualBorrow : UserControl
    {
        string mod_manuale = "împrumut";
        public ManualBorrow()
        {
            InitializeComponent();
            PopuleazaGrupe();
            SelectCatagories_of_Books();
        }

        private void BtnAdaugaExemplare_Click(object sender, RoutedEventArgs e)
        {
            int idCarte = (CmbManual.SelectedItem as ComboItem)?.Id ?? -1;
            if (CmbManual.SelectedValue == null || idCarte <= 0)
            {
                MessageBox.Show("Selectează mai întâi un manual valid din listă.", "Atenționare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string input = Microsoft.VisualBasic.Interaction.InputBox("Câte exemplare dorești să adaugi?", "Adaugă exemplare", "1");

            if (string.IsNullOrEmpty(input)) return;

            int cantitate = 0;
            try
            {
                cantitate = Convert.ToInt32(input);
            }
            catch
            {
                MessageBox.Show("Te rog să introduci doar numere!", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (cantitate <= 0)
            {
                MessageBox.Show("Cantitatea trebuie să fie mai mare ca 0.", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                using (MySqlCommand cmd = new MySqlCommand("sp_adauga_exemplare_manual", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_id_carte", idCarte);
                    cmd.Parameters.AddWithValue("@p_cantitate", cantitate);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show(cantitate + " exemplare au fost adăugate cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                PopuleazaExemplareDisponibile(idCarte);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la baza de date: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopuleazaExemplareDisponibile(int idCarte)
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                using (MySqlCommand cmd = new MySqlCommand("sp_numara_exemplare_disponibile", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_id_carte", idCarte);

                    conn.Open();
                    int total = Convert.ToInt32(cmd.ExecuteScalar());
                    TxtNumarExemplare.Text = "Exemplare disponibile: " + total;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nu am putut încărca numărul de exemplare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbManual_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = CmbManual.SelectedItem as ComboItem;
            if (item == null || item.Id <= 0)
            {
                TxtNumarExemplare.Text = "Exemplare disponibile: 0";
                return;
            }
            PopuleazaExemplareDisponibile(item.Id);
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
                        lista.Add(new ComboItem { Id = -1, Denumire = "Selectează manualul..." });
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
                        CmbManual.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la sortarea manualelor: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                        lista.Add(new ComboItem { Denumire = "Selectează categoria..." });

                        foreach (DataRow rand in dt.Rows)
                        {
                            lista.Add(new ComboItem { Denumire = rand["Categorie"].ToString() });
                        }

                        CmbCategorie.ItemsSource = lista;
                        CmbCategorie.DisplayMemberPath = "Denumire";
                        CmbCategorie.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea categoriilor: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnInapoi_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.ChangeView(new Borrow());
        }

        private void CmbCategorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedCategorie = (CmbCategorie.SelectedItem as ComboItem)?.Denumire;

            if (!string.IsNullOrEmpty(selectedCategorie) && selectedCategorie != "Selectează categoria...")
            {
                SorteazaManuale_perCategorie(selectedCategorie);
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
                        lista.Add(new ComboItem { Denumire = "Selectează grupa..." });
                        foreach (DataRow rand in dt.Rows)
                        {
                            lista.Add(new ComboItem { Denumire = rand["Grupa"].ToString() });
                        }

                        CmbGrupa.ItemsSource = lista;
                        CmbGrupa.DisplayMemberPath = "Denumire";
                        CmbGrupa.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la popularea grupelor: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                initiale = (nume[0].ToString() + prenume[0].ToString()).ToUpper();
                            }

                            ElevModel elev = new ElevModel(
                                Convert.ToInt32(rand["Id_elev"]),
                                numeComplet,
                                initiale,
                                "#4483EC",
                                false
                            );

                            listaElevi.Add(elev);
                        }

                        DgElevi.ItemsSource = listaElevi;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea elevilor: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbGrupa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string? selectedGrupa = (CmbGrupa.SelectedItem as ComboItem)?.Denumire;
            if (string.IsNullOrEmpty(selectedGrupa) || selectedGrupa == "Selectează grupa...") return;

            if (mod_manuale == "returnare")
            {
                if (CmbManual.SelectedValue == null || Convert.ToInt32(CmbManual.SelectedValue) <= 0)
                {
                    MessageBox.Show("Selectează mai întâi un manual pentru a vedea cine trebuie să returneze.", "Atenționare", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CmbGrupa.SelectedIndex = 0;
                    return;
                }
                int idCarte = -1;

                if (CmbManual.SelectedItem != null)
                {
                    ComboItem itemSelectat = (ComboItem)CmbManual.SelectedItem;
                    idCarte = itemSelectat.Id;
                }

                SelectStudentsWithManual(selectedGrupa, idCarte);
                TxtGrupLabel.Text = "Elevi care returnează — grupa " + selectedGrupa;
            }
            else
            {
                SelectStudents(selectedGrupa);
                TxtGrupLabel.Text = "Elevi în grupa " + selectedGrupa;
            }
        }

        private void areManual_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var elev = checkBox?.DataContext as ElevModel;

            if (elev == null) return;

            var listaElevi = DgElevi.ItemsSource as List<ElevModel>;
            if (listaElevi != null)
            {
                int nrSelectati = 0;
                foreach (ElevModel el in listaElevi)
                {
                    if (el.AreManual)
                    {
                        nrSelectati++;
                    }
                }

                TxtEleviSelectati.Text = "Elevi selectați: " + nrSelectati;
            }
        }

        private void ChkBifeazaToti_Changed(object sender, RoutedEventArgs e)
        {
            var chkToate = sender as CheckBox;
            if (chkToate == null) return;
            bool isChecked = chkToate.IsChecked ?? false;

            var listaElevi = DgElevi.ItemsSource as List<ElevModel>;
            if (listaElevi == null) return;

            foreach (ElevModel elev in listaElevi)
            {
                elev.AreManual = isChecked;
            }

            DgElevi.Items.Refresh();
            int nrSelectati = 0;
            foreach (ElevModel elev in listaElevi)
            {
                if (elev.AreManual)
                {
                    nrSelectati++;
                }
            }

            TxtEleviSelectati.Text = "Elevi selectați: " + nrSelectati;
        }

        private void BtnSalveaza_Click(object sender, RoutedEventArgs e)
        {
            var listaElevi = DgElevi.ItemsSource as List<ElevModel>;
            if (listaElevi == null || listaElevi.Count == 0)
            {
                MessageBox.Show("Nu există elevi încărcați.", "Atenționare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbManual.SelectedValue == null)
            {
                MessageBox.Show("Selectează mai întâi un manual din listă.", "Atenționare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int idCarte = Convert.ToInt32(CmbManual.SelectedValue);
            List<ElevModel> eleviBifati = new List<ElevModel>();
            foreach (ElevModel elev in listaElevi)
            {
                if (elev.AreManual)
                    eleviBifati.Add(elev);
            }

            if (eleviBifati.Count == 0)
            {
                MessageBox.Show("Niciun elev nu a fost selectat pentru a primi manualul.", "Atenționare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int idBibliotecar = SesiuneBibliotecar.IdBibliotecarCurent;
            int reusit = 0;
            int esuat = 0;
            System.Text.StringBuilder erori = new System.Text.StringBuilder();

            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    int idAnScolar = 0;
                    using (MySqlCommand cmdAn = new MySqlCommand("sp_an_scolar_activ", conn))
                    {
                        cmdAn.CommandType = CommandType.StoredProcedure;

                        MySqlParameter pAnScolar = new MySqlParameter("@p_id_an_scolar", MySqlDbType.Int32);
                        pAnScolar.Direction = ParameterDirection.Output;
                        cmdAn.Parameters.Add(pAnScolar);

                        cmdAn.ExecuteNonQuery();
                        idAnScolar = Convert.ToInt32(pAnScolar.Value);
                    }

                    if (idAnScolar == 0)
                    {
                        MessageBox.Show("Nu există un an școlar activ în baza de date.", "Atenționare", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    foreach (ElevModel elev in eleviBifati)
                    {
                        int idExemplarDisponibil = 0;
                        string queryExemplar = "SELECT id FROM exemplare WHERE id_carte = @idCarte AND stare = 'disponibil' AND arhivat = 0 LIMIT 1";

                        using (MySqlCommand cmdEx = new MySqlCommand(queryExemplar, conn))
                        {
                            cmdEx.Parameters.AddWithValue("@idCarte", idCarte);
                            object result = cmdEx.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                idExemplarDisponibil = Convert.ToInt32(result);
                            }
                        }

                        if (idExemplarDisponibil == 0)
                        {
                            esuat++;
                            erori.AppendLine("• " + elev.NumeElev + ": Stoc epuizat. Nu mai sunt exemplare disponibile.");
                            continue;
                        }

                        using (MySqlCommand cmd = new MySqlCommand("sp_distribuie_manual", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@p_id_exemplar", idExemplarDisponibil);
                            cmd.Parameters.AddWithValue("@p_id_elev", elev.Id);
                            cmd.Parameters.AddWithValue("@p_id_an_scolar", idAnScolar);
                            cmd.Parameters.AddWithValue("@p_id_bibliotecar", idBibliotecar);

                            MySqlParameter pIdImprumut = new MySqlParameter("@p_id_imprumut_manual", MySqlDbType.Int32);
                            pIdImprumut.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(pIdImprumut);

                            MySqlParameter pCod = new MySqlParameter("@p_cod", MySqlDbType.Int32);
                            pCod.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(pCod);

                            MySqlParameter pMesaj = new MySqlParameter("@p_mesaj", MySqlDbType.VarChar, 255);
                            pMesaj.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(pMesaj);

                            cmd.ExecuteNonQuery();

                            int cod = Convert.ToInt32(pCod.Value);
                            string mesaj = pMesaj.Value != null ? pMesaj.Value.ToString() : "";

                            if (cod == 0)
                            {
                                reusit++;
                            }
                            else
                            {
                                esuat++;
                                erori.AppendLine("• " + elev.NumeElev + ": " + mesaj);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la salvare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PopuleazaExemplareDisponibile(idCarte);
            string summary = "Salvat cu succes: " + reusit + " elevi.";
            if (esuat > 0)
                summary += "\nEșuat: " + esuat + " elevi:\n" + erori.ToString();

            MessageBox.Show(summary, "Rezultat Împrumut", MessageBoxButton.OK, MessageBoxImage.Information);

            foreach (ElevModel elev in listaElevi)
            {
                elev.AreManual = false;
            }
            reseteazaDataGrid();
        }

        private void BtnModImprumut_Click(object sender, RoutedEventArgs e)
        {
            if (mod_manuale == "împrumut")
            {
                return;
            } else
            {
                mod_manuale = "împrumut";
            }
            aplicaStilToggle(activ: BtnModImprumut, inactiv: BtnModReturnare);
            BtnSalveaza.Visibility = Visibility.Visible;
            BtnReturnare.Visibility = Visibility.Collapsed;
            reseteazaDataGrid();
            BtnAdaugaExemplare.IsEnabled = true;
            BtnAdaugaExemplare.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
            BtnAdaugaExemplare.Foreground = new SolidColorBrush(Colors.White);
        }

        private void BtnModReturnare_Click(object sender, RoutedEventArgs e)
        {
            if (mod_manuale == "returnare")
            {
                return;
            } else
            {
                mod_manuale = "returnare";
            }
            aplicaStilToggle(activ: BtnModReturnare, inactiv: BtnModImprumut);
            BtnSalveaza.Visibility = Visibility.Collapsed;
            BtnReturnare.Visibility = Visibility.Visible;
            reseteazaDataGrid();
            BtnAdaugaExemplare.IsEnabled = false;
            BtnAdaugaExemplare.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            BtnAdaugaExemplare.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
        }

        private void aplicaStilToggle(Button activ, Button inactiv)
        {
            activ.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
            activ.Foreground = new SolidColorBrush(Colors.White);
            inactiv.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            inactiv.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
        }

        private void reseteazaDataGrid()
        {
            DgElevi.ItemsSource = null;
            CmbGrupa.SelectedIndex = 0;
            TxtGrupLabel.Text = "";
            TxtEleviSelectati.Text = "Elevi selectați: 0";
            ChkBifeazaToti.IsChecked = false;
            CmbManual.SelectedIndex = 0;
            CmbCategorie.SelectedIndex = 0;
        }

        private void SelectStudentsWithManual(string grupa, int idCarte)
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                using (MySqlCommand cmd = new MySqlCommand("sp_elevi_cu_manual_nereturnat", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_cod_grupa", grupa);
                    cmd.Parameters.AddWithValue("@p_id_carte", idCarte);
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
                            string initiale = (!string.IsNullOrEmpty(nume) && !string.IsNullOrEmpty(prenume))? (nume[0].ToString() + prenume[0].ToString()).ToUpper() : "";

                            listaElevi.Add(new ElevModel(
                                Convert.ToInt32(rand["Id_elev"]),
                                nume + " " + prenume,
                                initiale, "#E05C00", false)
                            {
                                IdImprumut = Convert.ToInt32(rand["Id_imprumut"])
                            });
                        }

                        DgElevi.ItemsSource = listaElevi;

                        if (listaElevi.Count == 0)
                            MessageBox.Show("Niciun elev din această grupă nu are manualul de returnat.", "Informație", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la încărcarea elevilor: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnReturnare_Click(object sender, RoutedEventArgs e)
        {
            var listaElevi = DgElevi.ItemsSource as List<ElevModel>;
            if (listaElevi == null || listaElevi.Count == 0)
            {
                MessageBox.Show("Nu există elevi încărcați.", "Atenționare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<ElevModel> eleviBifati = new List<ElevModel>();
            foreach (ElevModel elev in listaElevi)
                if (elev.AreManual) eleviBifati.Add(elev);

            if (eleviBifati.Count == 0)
            {
                MessageBox.Show("Niciun elev nu a fost selectat pentru returnare.", "Atenționare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int idBibliotecar = SesiuneBibliotecar.IdBibliotecarCurent;
            int reusit = 0, esuat = 0;
            System.Text.StringBuilder erori = new System.Text.StringBuilder();

            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    foreach (ElevModel elev in eleviBifati)
                    {
                        using (MySqlCommand cmd = new MySqlCommand("sp_returneaza_manual", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@p_id_imprumut_manual", elev.IdImprumut);
                            cmd.Parameters.AddWithValue("@p_id_bibliotecar", idBibliotecar);

                            MySqlParameter pCod = new MySqlParameter("@p_cod", MySqlDbType.Int32)
                            { Direction = ParameterDirection.Output };
                            MySqlParameter pMesaj = new MySqlParameter("@p_mesaj", MySqlDbType.VarChar, 255)
                            { Direction = ParameterDirection.Output };

                            cmd.Parameters.Add(pCod);
                            cmd.Parameters.Add(pMesaj);
                            cmd.ExecuteNonQuery();

                            if (Convert.ToInt32(pCod.Value) == 0)
                                reusit++;
                            else
                            {
                                esuat++;
                                erori.AppendLine("• " + elev.NumeElev + ": " + pMesaj.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la returnare: " + ex.Message, "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CmbManual.SelectedValue != null && Convert.ToInt32(CmbManual.SelectedValue) > 0)
                PopuleazaExemplareDisponibile(Convert.ToInt32(CmbManual.SelectedValue));

            string summary = "Returnate cu succes: " + reusit + " elevi.";
            if (esuat > 0) summary += "\nEșuat: " + esuat + " elevi:\n" + erori;
            MessageBox.Show(summary, "Rezultat Returnare", MessageBoxButton.OK, MessageBoxImage.Information);

            reseteazaDataGrid();
        }
    }
}