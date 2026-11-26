using MasterMind_Client.AvatarManagement;
using MasterMind_Client.ServerEndpoints;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Services;
using System.Globalization;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using MasterMind_Client.Helpers;

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
            AvatarCache.DeleteOrphanSessions();

            AppServices.NavigationService = new NavigationServiceAdapter();
            AppServices.DialogService = new DialogService();
            AppServices.MusicService = new MusicService();
            AppServices.MusicService.Play();
            AppServices.SoundEffectService = new SoundEffectService();
            EventManager.RegisterClassHandler(typeof(ButtonBase), ButtonBase.ClickEvent, new RoutedEventHandler(OnButtonClick));

            var mainWindow = new MainWindow();

            if (MasterMind_Client.Properties.Settings.Default.RememberLogin &&
                 !string.IsNullOrEmpty(MasterMind_Client.Properties.Settings.Default.SavedUsername))
            {
                try
                {
                    var savedPlayer = ServiceClientFactory.CreatePlayerClient().GetPlayerByUsername(
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

        protected override void OnExit(ExitEventArgs e)
        {
            AppServices.MusicService?.Stop();
            CurrentPlayer.Instance.RegisterOffline();
            AvatarCache.ClearSession();
            base.OnExit(e);
        }

        private static void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (!SoundEffects.GetIsSilent((DependencyObject)sender))
            {
                AppServices.SoundEffectService.Play(SoundEffect.ButtonClick);
            }
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
