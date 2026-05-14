using System.Configuration;
using MySql.Data.MySqlClient;

namespace BibliotecaCEITI
{
    public static class DatabaseConfig
    {
        public static string ConnectionString = $"Server=localhost; Port=3306; Database=biblioteca_ceiti_go; Uid=root; Pwd={Environment.GetEnvironmentVariable("DB_password")}; CharSet=utf8mb4; SslMode=Disabled; AllowPublicKeyRetrieval=True; ConnectionTimeout=10;";
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}