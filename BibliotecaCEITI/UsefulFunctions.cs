using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BibliotecaCEITI
{
    public static class UsefulFunction
    {
        public static BitmapImage GetImagineCarte(int idCarte)
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_imagine_carte", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", idCarte);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && !reader.IsDBNull(reader.GetOrdinal("Imagine")))
                            {
                                byte[] copertaBytes = (byte[])reader["Imagine"];
                                return ConvertBytesToImage(copertaBytes);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                
            }

            return null;
        }

        public static BitmapImage ConvertBytesToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                return null;
            }

            try
            {
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static (BitmapImage Imagine, string CaleFisier) AlegeImagineDinFisier()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Fișiere Imagine (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|Toate fișierele (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string cale = openFileDialog.FileName;

                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(cale, UriKind.Absolute);
                    bitmap.EndInit();
                    bitmap.Freeze();

                    return (bitmap, cale);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Eroare la procesarea imaginii: " + ex.Message, "Eroare", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            return (null, null);
        }

        public static void GotFocus(object sender, string placeholder)
        {
            if (sender is TextBox txt && txt.Text == placeholder)
            {
                txt.Text = "";
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        public static void LostFocus(object sender, string placeholder)
        {
            if (sender is TextBox txt && string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = placeholder;
                txt.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }
    }
}