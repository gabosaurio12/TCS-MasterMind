using MasterMind_Client.Assets;
using MasterMind_Client.Modals;
using MasterMind_Client.TempData;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Lógica de interacción para LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private TempPlayer GetFormsData()
        {
            return new TempPlayer
            {
                Username = UsernameTxt.Text,
                Password = PasswordTxt.Password
            };
        }

        private bool ValidateFormsData(TempPlayer player)
        {
            if (string.IsNullOrWhiteSpace(player.Username) || string.IsNullOrWhiteSpace(player.Password))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WhiteInput).Show();
                return false;
            }
            if (!TempAuthService.AuthPlayer(player))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WrongCredentials).Show();
                return false;
            }
            return true;
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var player = GetFormsData();
            if (ValidateFormsData(player))
            {
                new VerificationCodeModal(player.Username, NavigationService).Show();
            }
        }

        private void SignupLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/SignupPage.xaml", UriKind.Relative));
        }


        private void LanguageBtn_Click(object sender, RoutedEventArgs e)
        {
            UtilsUI.ChangeLanguage(NavigationService);
        }
    }
}
