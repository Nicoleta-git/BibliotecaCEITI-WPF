using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Process? _dbProcess;

        protected override void OnStartup(StartupEventArgs e)
        {
            StartBundledDatabase();
            base.OnStartup(e);
        }

        private void StartBundledDatabase()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string mysqldPath = Path.Combine(baseDir, "mysql", "bin", "mysqld.exe");
            string dataDir = Path.Combine(baseDir, "mysql", "data");

            _dbProcess = Process.Start(new ProcessStartInfo
            {
                FileName = mysqldPath,
                Arguments = $"--datadir=\"{dataDir}\" --port=3308",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            for (int attempt = 0; attempt < 50; attempt++)
            {
                try
                {
                    using var conn = new MySqlConnection(DatabaseConfig.ConnectionString);
                    conn.Open();
                    return;
                }
                catch
                {
                    Thread.Sleep(200);
                }
            }

            MessageBox.Show(
                "Nu s-a putut porni baza de date locala.",
                "Eroare",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            StopBundledDatabase();
            base.OnExit(e);
        }

        private void StopBundledDatabase()
        {
            if (_dbProcess == null || _dbProcess.HasExited)
            {
                return;
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string mysqladminPath = Path.Combine(baseDir, "mysql", "bin", "mysqladmin.exe");

            try
            {
                var shutdown = Process.Start(new ProcessStartInfo
                {
                    FileName = mysqladminPath,
                    Arguments = "-h 127.0.0.1 -P 3308 -u root shutdown",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                shutdown?.WaitForExit(5000);
            }
            catch
            {
            }

            if (!_dbProcess.HasExited)
            {
                _dbProcess.Kill();
            }
        }
    }

}
