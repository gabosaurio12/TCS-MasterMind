using MasterMind_Client.TempData;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;

namespace MasterMind_Client.ViewModels.Pages
{
    public class ProfilePageViewModel : ViewModelBase
    {
        public ProfilePageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            UpdateCommand = new RelayCommand(_ => NavigateTo("Pages/UpdateProfilePage.xaml"));
            BackCommand = new RelayCommand(_ => NavigateTo("Pages/MainPage.xaml"));
            ReportCommand = new RelayCommand(_ => { });
            ChangePasswordCommand = new RelayCommand(_ => { });
            Username = CurrentPlayer.Instance.Username;
            Email = CurrentPlayer.Instance.Email;
            SetRecords();
        }

        public string Username { get; }

        public string Email { get; }

        public string BestTime { get; }

        public string LeastTries { get; }

        public RelayCommand UpdateCommand { get; }

        public RelayCommand BackCommand { get; }

        public RelayCommand ReportCommand { get; }

        public RelayCommand ChangePasswordCommand { get; }

        private readonly INavigationService navigationService;

        private void SetRecords()
        {
            var records = TempPlayerService.GetPlayerRecords(Username);
            foreach (var tries in records.PlayerTimeTrialRecords)
            {
            }
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }
    }
}
