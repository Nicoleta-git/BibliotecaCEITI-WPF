using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace BibliotecaCEITI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += (s, ex) =>
            {
                if (ex.Exception is InvalidOperationException &&
                    ex.Exception.Message.Contains("not an ancestor"))
                {
                    ex.Handled = true;
                    return;
                }
            };

            EventManager.RegisterClassHandler(
                typeof(DataGrid),
                DataGrid.LoadedEvent,
                new RoutedEventHandler((s, ev) =>
                {
                    if (s is DataGrid dg)
                    {
                        dg.SetValue(VirtualizingPanel.IsVirtualizingProperty, false);
                        dg.SetValue(ScrollViewer.CanContentScrollProperty, false);
                    }
                }));
        }
    }

}
