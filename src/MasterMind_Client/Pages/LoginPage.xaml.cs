using MasterMind_Client.Assets;
using MasterMind_Client.Data;
using MasterMind_Client.Modals;
using MasterMind_Client.TempData;
using System;
using System.Windows;
using System.Windows.Controls;
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

        private Player GetFormsData()
        {
            return new Player
            {
                username = UsernameTxt.Text,
                password = PasswordTxt.Password
            };
        }

        private bool ValidateFormsData(Player player)
        {
            bool isValid = true;
            if (string.IsNullOrWhiteSpace(player.username) || string.IsNullOrWhiteSpace(player.password))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WhiteInput).Show();
                isValid = false;
            }
            if (!TempAuthService.AuthPlayer(player))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WrongCredentials).Show();
                isValid = false;
            }
            return isValid;
        }

        private void ModalVerificationSucceded(object sender, Player verifiedPlayer)
        {
            CurrentPlayer.Instance.SetCurrentPlayer(verifiedPlayer);

            if (RememberLoginCheck.IsChecked == true)
            {
                Properties.Settings.Default.RememberLogin = true;
                Properties.Settings.Default.SavedUsername = verifiedPlayer.username;
            }
            else
            {
                Properties.Settings.Default.RememberLogin = false;
                Properties.Settings.Default.SavedUsername = "";
            }

            Properties.Settings.Default.Save();
            NavigationService.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));

        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var player = GetFormsData();
            if (ValidateFormsData(player))
            {
                var modal = new VerificationCodeModal(player.username);
                modal.VerificationSucceded += ModalVerificationSucceded;
                modal.Show();
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
