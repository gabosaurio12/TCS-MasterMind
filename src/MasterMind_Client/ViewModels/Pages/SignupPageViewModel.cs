using MasterMind_Client.Assets;
using MasterMind_Client.Data;
using MasterMind_Client.TempData;
using MasterMind_Client.TempData.Enum;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;
using System.Text.RegularExpressions;

namespace MasterMind_Client.ViewModels.Pages
{
    public class SignupPageViewModel : ViewModelBase
    {
        private static readonly Regex PasswordRegex = new Regex(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            RegexOptions.None,
            TimeSpan.FromMilliseconds(100));

        private static readonly Regex EmailRegex = new Regex(
            @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
            RegexOptions.None,
            TimeSpan.FromMilliseconds(100));

        public SignupPageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            RegisterCommand = new RelayCommand(_ => Register());
            LoginLinkCommand = new RelayCommand(_ => NavigateTo("Pages/LoginPage.xaml"));
            LanguageCommand = new RelayCommand(_ => ChangeLanguage());
        }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public bool IsUsernameEmpty
        {
            get => _isUsernameEmpty;
            private set => SetProperty(ref _isUsernameEmpty, value);
        }

        public bool IsEmailEmpty
        {
            get => _isEmailEmpty;
            private set => SetProperty(ref _isEmailEmpty, value);
        }

        public bool IsPasswordEmpty
        {
            get => _isPasswordEmpty;
            private set => SetProperty(ref _isPasswordEmpty, value);
        }

        public RelayCommand RegisterCommand { get; }

        public RelayCommand LoginLinkCommand { get; }

        public RelayCommand LanguageCommand { get; }

        private bool _isUsernameEmpty;
        private bool _isEmailEmpty;
        private bool _isPasswordEmpty;

        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;

        private void Register()
        {
            if (!ValidateFormsData())
            {
                return;
            }

            var player = new Player
            {
                username = Username.Trim(),
                password = Password,
                email = Email
            };

            var result = TempAuthService.RegisterPlayer(player);
            switch (result)
            {
                case PlayerRegistrationResultEnum.Success:
                    dialogService.ShowSuccess(
                        Properties.Resources.SuccessNotification_Register,
                        () => ShowVerificationCode(player.username));
                    break;
                case PlayerRegistrationResultEnum.UsernameTaken:
                    dialogService.ShowError(Properties.Resources.ErrorNotification_UsernameTaken);
                    break;
                case PlayerRegistrationResultEnum.EmailTaken:
                    dialogService.ShowError(Properties.Resources.ErrorNotification_EmailTaken);
                    break;
            }
        }

        private void ShowVerificationCode(string username)
        {
            dialogService.ShowVerificationCode(username, OnVerificationSucceded);
        }

        private void OnVerificationSucceded(Player verifiedPlayer)
        {
            CurrentPlayer.Instance.SetCurrentPlayer(verifiedPlayer);

            Properties.Settings.Default.RememberLogin = false;
            Properties.Settings.Default.SavedUsername = "";

            Properties.Settings.Default.Save();
            navigationService.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));
        }

        private bool ValidateFormsData()
        {
            var isValid = true;
            if (!CheckForEmptyFields())
            {
                isValid = false;
            }

            if (Regex.IsMatch(Username, @"\s"))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_NoSpaces);
                isValid = false;
            }

            var email = Email;
            var substringEmail = email.Split('@');
            if (!EmailRegex.IsMatch(email))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_InvalidEmail);
                isValid = false;
            }

            if (substringEmail.Length > 2)
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_InvalidEmail);
                isValid = false;
            }

            if (!PasswordRegex.IsMatch(Password))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_InvalidPassword);
                isValid = false;
            }

            return isValid;
        }

        private bool CheckForEmptyFields()
        {
            var isValid = true;
            if (string.IsNullOrWhiteSpace(Username))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                IsUsernameEmpty = true;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                IsEmailEmpty = true;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                IsPasswordEmpty = true;
                isValid = false;
            }

            return isValid;
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }

        private void ChangeLanguage()
        {
            UtilsUI.ToggleLanguage();
            navigationService.Refresh();
        }
    }
}
