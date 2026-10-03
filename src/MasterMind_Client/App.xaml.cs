using log4net.Config;
using System.Globalization;
using System.Windows;

namespace MasterMind_Client
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CultureInfo cultureToUse = DetermineStartupCulture();

            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureToUse;
            CultureInfo.DefaultThreadCurrentUICulture = cultureToUse;

            XmlConfigurator.Configure();

            var mainWindow = new MainWindow();

            if (MasterMind_Client.Properties.Settings.Default.RememberLogin &&
                !string.IsNullOrEmpty(MasterMind_Client.Properties.Settings.Default.SavedUsername))
            {
                var savedPlayer = TempData.TempPlayerService.GetPlayerByUsername(
                    MasterMind_Client.Properties.Settings.Default.SavedUsername);

                if (savedPlayer != null)
                {
                    CurrentPlayer.Instance.SetCurrentPlayer(savedPlayer);
                    mainWindow.Source = new System.Uri("Pages/MainPage.xaml", System.UriKind.Relative);
                }
            }

            mainWindow.Show();
        }

        private static CultureInfo DetermineStartupCulture()
        {
            var systemCulture = CultureInfo.CurrentUICulture;

            if (systemCulture.TwoLetterISOLanguageName == "es")
            {
                return new CultureInfo("es-ES");
            }
            else
            {
                return new CultureInfo("en-US");
            }
        }
    }
}
