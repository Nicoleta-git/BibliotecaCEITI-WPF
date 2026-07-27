using System.Configuration;
using MySql.Data.MySqlClient;

namespace BibliotecaCEITI
{
    public static class DatabaseConfig
    {
        public static string ConnectionString = "Server=127.0.0.1; Port=3308; Database=biblioteca_ceiti_go; Uid=root; Pwd=; CharSet=utf8mb4; SslMode=Disabled; AllowPublicKeyRetrieval=True; ConnectionTimeout=10;";
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}
