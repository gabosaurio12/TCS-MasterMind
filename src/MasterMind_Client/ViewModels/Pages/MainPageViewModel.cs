using System;
using MasterMind_Client.Assets;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;

namespace MasterMind_Client.ViewModels.Pages
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            PlayCommand = new RelayCommand(_ => NavigateTo("Pages/RoomsPages/RoomsPage.xaml"));
            ProfileCommand = new RelayCommand(_ => NavigateTo("Pages/ProfilePage.xaml"));
            FriendsCommand = new RelayCommand(_ => dialogService.ShowFriends());
            ExitCommand = new RelayCommand(_ => dialogService.ShowLogOut());
            LanguageCommand = new RelayCommand(_ => ChangeLanguage());
        }

        public RelayCommand PlayCommand { get; }

        public RelayCommand ProfileCommand { get; }

        public RelayCommand FriendsCommand { get; }

        public RelayCommand ExitCommand { get; }

        public RelayCommand LanguageCommand { get; }

        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

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
