using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Services;
using System.Globalization;
using System.ServiceModel;
using System.Windows;

namespace MasterMind_Client
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CultureInfo cultureToUse = DetermineStartupCulture();

            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureToUse;
            CultureInfo.DefaultThreadCurrentUICulture = cultureToUse;

            XmlConfigurator.Configure();

            AppServices.NavigationService = new NavigationServiceAdapter();
            AppServices.DialogService = new DialogService();

            var mainWindow = new MainWindow();

            if (MasterMind_Client.Properties.Settings.Default.RememberLogin &&
                 !string.IsNullOrEmpty(MasterMind_Client.Properties.Settings.Default.SavedUsername))
            {
                try
                {
                    var savedPlayer = new PlayerServiceClient().GetPlayerByUsername(
                    MasterMind_Client.Properties.Settings.Default.SavedUsername);

                    if (savedPlayer != null)
                    {
                        CurrentPlayer.Instance.SetCurrentPlayer(savedPlayer);
                        mainWindow.Source = new System.Uri("Pages/MainPage.xaml", System.UriKind.Relative);
                    }
                }
                catch (FaultException<ErrorMessage> faultException)
                {
                    logger.WarnFormat(faultException.Detail.Message);
                    MessageBox.Show(faultException.Detail.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (FaultException faultException)
                {
                    logger.Error(faultException);
                    MessageBox.Show("Unexpected error occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (CommunicationException communicationException)
                {
                    logger.Error(communicationException);
                    MessageBox.Show("Unexpected error occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
