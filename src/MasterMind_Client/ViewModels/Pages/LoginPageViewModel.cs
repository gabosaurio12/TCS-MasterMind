using MasterMind_Client.Assets;
using MasterMind_Client.Data;
using MasterMind_Client.TempData;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;

namespace MasterMind_Client.ViewModels.Pages
{
    public class LoginPageViewModel : ViewModelBase
    {
        public LoginPageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            LoginCommand = new RelayCommand(_ => Login());
            SignupCommand = new RelayCommand(_ => NavigateTo("Pages/SignupPage.xaml"));
            LanguageCommand = new RelayCommand(_ => ChangeLanguage());
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool RememberLogin { get; set; }

        public RelayCommand LoginCommand { get; }

        public RelayCommand SignupCommand { get; }

        public RelayCommand LanguageCommand { get; }

        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        private void Login()
        {
            var player = new Player
            {
                username = Username,
                password = Password
            };

            if (!ValidateFormsData(player))
            {
                return;
            }

            _dialogService.ShowVerificationCode(player.username, OnVerificationSucceded);
        }

        private bool ValidateFormsData(Player player)
        {
            var isValid = true;
            if (string.IsNullOrWhiteSpace(player.username) || string.IsNullOrWhiteSpace(player.password))
            {
                _dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                isValid = false;
            }

            if (!TempAuthService.AuthPlayer(player))
            {
                _dialogService.ShowError(Properties.Resources.ErrorNotification_WrongCredentials);
                isValid = false;
            }

            return isValid;
        }

        private void OnVerificationSucceded(Player verifiedPlayer)
        {
            CurrentPlayer.Instance.SetCurrentPlayer(verifiedPlayer);

            if (RememberLogin)
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
            _navigationService.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));
        }

        private void NavigateTo(string uri)
        {
            _navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }

        private void ChangeLanguage()
        {
            UtilsUI.ToggleLanguage();
            _navigationService.Refresh();
        }
    }
}
