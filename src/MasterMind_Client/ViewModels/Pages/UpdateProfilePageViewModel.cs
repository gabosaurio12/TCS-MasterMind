using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;
using System.Text.RegularExpressions;

namespace MasterMind_Client.ViewModels.Pages
{
    public class UpdateProfilePageViewModel : ViewModelBase
    {
        private const string DefaultBestTime = "15";
        private const string DefaultLeastTries = "3";

        private string username;
        private string email;

        public UpdateProfilePageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => NavigateTo("Pages/ProfilePage.xaml"));
            username = CurrentPlayer.Instance.Username;
            email = CurrentPlayer.Instance.Email;
        }

        public string Username
        {
            get { return this.username; }
            set { SetProperty(ref this.username, value); }
        }

        public string Email
        {
            get { return this.email; }
            set { SetProperty(ref this.email, value); }
        }

        public string BestTimeText { get; } = DefaultBestTime;

        public string LeastTriesText { get; } = DefaultLeastTries;

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

            if (Regex.IsMatch(Username, @"\s", RegexOptions.None, TimeSpan.FromMilliseconds(500)))
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
