using System.Configuration;
using MySql.Data.MySqlClient;

namespace BibliotecaCEITI
{
    public static class DatabaseConfig
    {
        public static string ConnectionString => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}