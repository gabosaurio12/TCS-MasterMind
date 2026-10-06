using MasterMind_Client.Assets;
using MasterMind_Client.Data;
using MasterMind_Client.Modals;
using MasterMind_Client.TempData;
using MasterMind_Client.TempData.Enum;
using System;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Lógica de interacción para SignupPage.xaml
    /// </summary>
    public partial class SignupPage : Page
    {
        private readonly Regex passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            RegexOptions.None, TimeSpan.FromMilliseconds(100));
        private readonly Regex emailRegex = new Regex(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
            RegexOptions.None, TimeSpan.FromMilliseconds(100));
        private string pendingPlayerUsername;

        public SignupPage()
        {
            InitializeComponent();
        }

        private Player GetFormsData()
        {
            return new Player
            {
                username = UsernameTxt.Text.Trim(),
                password = PasswordTxt.Password,
                email = EmailTxt.Text
            };
        }

        private bool CheckForEmptyFields()
        {
            bool isValid = true;
            if (string.IsNullOrWhiteSpace(UsernameTxt.Text))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WhiteInput).Show();
                UsernameTxt.Foreground = Brushes.Red;
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(EmailTxt.Text))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WhiteInput).Show();
                EmailTxt.Foreground = Brushes.Red;
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(PasswordTxt.Password))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WhiteInput).Show();
                PasswordLbl.Foreground = Brushes.Red;
                isValid = false;
            }
            return isValid;
        }

        private bool ValidateFormsData()
        {
            bool isValid = true;
            if (!CheckForEmptyFields())
            {
                isValid = false;
            }

            if (Regex.IsMatch(UsernameTxt.Text, @"\s"))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_NoSpaces).Show();
                isValid = false;
            }

            var email = EmailTxt.Text;
            var substringEmail = email.Split('@');
            if (!emailRegex.IsMatch(email))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_InvalidEmail).Show();
                isValid = false;
            }
            if (substringEmail.Length > 2)
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_InvalidEmail).Show();
                isValid = false;
            }

            if (!passwordRegex.IsMatch(PasswordTxt.Password))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_InvalidPassword).Show();
                isValid = false;
            }

            return isValid;
        }

        private void ModalVerificationSucceded(object sender, Player verifiedPlayer)
        {
            CurrentPlayer.Instance.SetCurrentPlayer(verifiedPlayer);

            Properties.Settings.Default.RememberLogin = false;
            Properties.Settings.Default.SavedUsername = "";

            Properties.Settings.Default.Save();
            NavigationService.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));

        }

        private void ModalClosed(object sender, EventArgs e)
        {
            var modal = new VerificationCodeModal(pendingPlayerUsername);
            modal.VerificationSucceded += ModalVerificationSucceded;
            modal.Show();
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateFormsData())
            {
                var player = GetFormsData();
                var result = TempAuthService.RegisterPlayer(player);
                switch (result)
                {
                    case PlayerRegistrationResultEnum.Success:
                        pendingPlayerUsername = player.username;
                        var successModal = new SuccessNotificationModal(Properties.Resources.SuccessNotification_Register, true);
                        successModal.ModalClosed += ModalClosed;
                        successModal.Show();
                        break;
                        
                    case PlayerRegistrationResultEnum.UsernameTaken:
                        new ErrorNotificationModal(Properties.Resources.ErrorNotification_UsernameTaken).Show();
                        break;

                    case PlayerRegistrationResultEnum.EmailTaken:
                        new ErrorNotificationModal(Properties.Resources.ErrorNotification_EmailTaken).Show();
                        break;
                }
            }
        }

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/LoginPage.xaml", UriKind.Relative));
        }

        private void LanguageBtn_Click(object sender, RoutedEventArgs e)
        {
            UtilsUI.ChangeLanguage(NavigationService);
        }
    }
}
