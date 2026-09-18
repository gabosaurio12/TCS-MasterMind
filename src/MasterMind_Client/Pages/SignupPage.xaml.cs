using MasterMind_Client.Assets;
using MasterMind_Client.Modals;
using MasterMind_Client.TempData;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MasterMind_Client.TempData.Enum;
using System.Windows.Media;

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Lógica de interacción para SignupPage.xaml
    /// </summary>
    public partial class SignupPage : Page
    {
        private readonly Regex passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
        private readonly Regex emailRegex = new Regex(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public SignupPage()
        {
            InitializeComponent();
        }

        private TempPlayer GetFormsData()
        {
            return new TempPlayer
            {
                Username = UsernameTxt.Text,
                Password = PasswordTxt.Password,
                Email = EmailTxt.Text
            };
        }

        private bool CheckForEmptyFields()
        {
            if (string.IsNullOrWhiteSpace(UsernameTxt.Text))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WhiteInput).Show();
                UsernameTxt.Foreground = Brushes.Red;
                return false;
            }
            if (string.IsNullOrWhiteSpace(EmailTxt.Text))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WhiteInput).Show();
                EmailTxt.Foreground = Brushes.Red;
                return false;
            }
            if (string.IsNullOrWhiteSpace(PasswordTxt.Password))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WhiteInput).Show();
                PasswordLbl.Foreground = Brushes.Red;
                return false;
            }
            return true;
        }

        private bool ValidateFormsData()
        {
            if (!CheckForEmptyFields())
            {
                return false;
            }

            var email = EmailTxt.Text;
            var substringEmail = email.Split('@');
            if (!emailRegex.IsMatch(email))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_InvalidEmail).Show();
                return false;
            }
            if (substringEmail.Length > 2)
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_InvalidEmail).Show();
                return false;
            }

            if (!passwordRegex.IsMatch(PasswordTxt.Password))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_InvalidPassword).Show();
                return false;
            }

            return true;
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateFormsData())
            {
                var player = GetFormsData();
                var result = TempData.TempAuthService.RegisterPlayer(player);
                switch (result)
                {
                    case RegistrationResult.Success:
                        new SuccessNotificationModal(Properties.Resources.SuccessNotification_Register).Show();
                        new VerificationCodeModal(player.Username, NavigationService).Show();
                        break;
                    case RegistrationResult.UsernameTaken:
                        new ErrorNotificationModal(Properties.Resources.ErrorNotification_UsernameTaken).Show();
                        break;
                    case RegistrationResult.EmailTaken:
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
