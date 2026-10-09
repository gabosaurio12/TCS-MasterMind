using MasterMind_Client.Data;
using MasterMind_Client.TempData;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;
using System.Text.RegularExpressions;

namespace MasterMind_Client.ViewModels.Pages
{
    public class UpdateProfilePageViewModel : ViewModelBase
    {
        public UpdateProfilePageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => NavigateTo("Pages/ProfilePage.xaml"));
            Username = CurrentPlayer.Instance.Username;
            Email = CurrentPlayer.Instance.Email;
        }

        public string Username { get; set; }

        public string Email { get; set; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand CancelCommand { get; }

        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;

        private void Save()
        {
            if (!ValidateInput())
            {
                return;
            }

            var player = new Player
            {
                player_id = CurrentPlayer.Instance.Id,
                username = Username,
                email = Email,
            };

            TempPlayerService.UpdatePlayer(player);
            NavigateTo("Pages/ProfilePage.xaml");
        }

        private bool ValidateInput()
        {
            var isValid = true;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                isValid = false;
            }

            if (Regex.IsMatch(Username, @"\s"))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_NoSpaces);
                isValid = false;
            }

            return isValid;
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }
    }
}
