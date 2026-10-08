using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;

namespace MasterMind_Client.ViewModels.Pages
{
    public class ProfilePageViewModel : ViewModelBase
    {
        public ProfilePageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            UpdateCommand = new RelayCommand(_ => NavigateTo("Pages/UpdateProfilePage.xaml"));
            BackCommand = new RelayCommand(_ => NavigateTo("Pages/MainPage.xaml"));
            ReportCommand = new RelayCommand(_ => { });
            ChangePasswordCommand = new RelayCommand(_ => { });
            Username = CurrentPlayer.Instance.Username;
            Email = CurrentPlayer.Instance.Email;
        }

        public string Username { get; }

        public string Email { get; }

        public string BestTime { get; } = "15";

        public string LeastTries { get; } = "3";

        public RelayCommand UpdateCommand { get; }

        public RelayCommand BackCommand { get; }

        public RelayCommand ReportCommand { get; }

        public RelayCommand ChangePasswordCommand { get; }

        private readonly INavigationService _navigationService;

        private void NavigateTo(string uri)
        {
            _navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }
    }
}
