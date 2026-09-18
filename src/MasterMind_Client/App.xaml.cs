using System.Windows;
using System.Globalization;
using MasterMind_Client.TempData;

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

            TempAuthService.MockRegisterPlayer(
                new TempPlayer
                {
                    Username = "testuser",
                    Password = "password123",
                    Email = "testuser@example.com"
                });

            TempAuthService.MockRegisterPlayer(
                new TempPlayer
                {
                    Username = "johndoe",
                    Password = "securepass",
                    Email = "johndoe@example.com"
                });
        }

        private CultureInfo DetermineStartupCulture()
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
