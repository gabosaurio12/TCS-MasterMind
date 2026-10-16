using MasterMind_Client.Helpers;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;

namespace MasterMind_Client.ViewModels.Pages
{
    public class LoginPageViewModel : ViewModelBase
    {
        public LoginPageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
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

        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;

        private void Login()
        {
            var player = new Player
            {
                Username = Username,
                Password = Password
            };

            if (ValidateFormsData(player))
            {
                dialogService.ShowVerificationCode(player.Username, OnVerificationSucceded);
            }
        }

        private bool ValidateFormsData(Player player)
        {
            var isValid = true;
            if (string.IsNullOrWhiteSpace(player.Username) || string.IsNullOrWhiteSpace(player.Password))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                isValid = false;
            }

            var service = new PlayerServiceClient();
            if (!service.LoginPlayer(player.Username, player.Password))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WrongCredentials);
                isValid = false;
            }

            return isValid;
        }

        private void OnVerificationSucceded(Player verifiedPlayer)
        {
            var player = new PlayerServiceClient().GetPlayerById(verifiedPlayer.PlayerId);
            CurrentPlayer.Instance.SetCurrentPlayer(player);

            if (RememberLogin)
            {
                Properties.Settings.Default.RememberLogin = true;
                Properties.Settings.Default.SavedUsername = player.Username;
            }
            else
            {
                Properties.Settings.Default.RememberLogin = false;
                Properties.Settings.Default.SavedUsername = "";
            }

            Properties.Settings.Default.Save();
            navigationService.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));
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
