using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BibliotecaCEITI
{
    public static class UsefulFunctions
    {
        internal static void LoadImage(int idExemplar, Border imgCoperta)
        {
            throw new NotImplementedException();
        }

        private static void LoadImage(int id = 0, System.Windows.Controls.Image imgCoperta = null)
        {
            try
            {
                using (MySqlConnection conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_imagine_carte", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_id", id);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && !reader.IsDBNull(0))
                            {
                                byte[] copertaBytes = (byte[])reader["Imagine"];
                                using (MemoryStream ms = new MemoryStream(copertaBytes))
                                {
                                    BitmapImage bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.StreamSource = ms;
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.EndInit();
                                    bitmap.Freeze();
                                    if (imgCoperta != null)
                                        imgCoperta.Source = bitmap;
                                }
                            }
                            else
                            {
                                if (imgCoperta != null)
                                    imgCoperta.Source = null;
                            }
                        }
                    }
                }
            }
            catch
            {
                if (imgCoperta != null)
                    imgCoperta.Source = null;
            }
        }
    }
}
