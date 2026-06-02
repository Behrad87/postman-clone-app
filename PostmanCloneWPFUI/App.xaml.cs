using PostmanCloneWPFUI.Views;

using System.Configuration;
using System.Data;
using System.Windows;

namespace PostmanCloneWPFUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                ServiceLocator.Build();

                var window = ServiceLocator.Get<MainWindow>();

                window.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString() +
                    "\n\nINNER:\n" +
                    ex.InnerException?.ToString(),
                    "Startup Error");
            }
        }
    }

}
