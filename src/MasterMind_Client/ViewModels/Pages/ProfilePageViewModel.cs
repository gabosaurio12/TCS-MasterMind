using MasterMind_Client.AvatarManagement;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MasterMind_Client.ViewModels.Pages
{
    public class ProfilePageViewModel : ViewModelBase
    {

        private ImageSource avatar = AvatarLoader.GetDefaultAvatar();

        public string Username { get; }

        public ImageSource Avatar
        {
            get { return this.avatar; }
            private set { SetProperty(ref this.avatar, value); }
        }

        public string Email { get; }

        public string ReportedPlayer { get; set; }

        public ObservableCollection<string> BestTimes { get; }
        public ObservableCollection<string> LeastTries { get; }

        public RelayCommand UpdateCommand { get; }

        public RelayCommand BackCommand { get; }

        public RelayCommand ReportCommand { get; }

        public RelayCommand ChangePasswordCommand { get; }

        private readonly INavigationService navigationService;

        private readonly IDialogService dialogService;

        private readonly PlayerServiceClient playerService = new PlayerServiceClient();

        public ProfilePageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            UpdateCommand = new RelayCommand(_ => NavigateTo("Pages/UpdateProfilePage.xaml"));
            BackCommand = new RelayCommand(_ => NavigateTo("Pages/MainPage.xaml"));
            ReportCommand = new RelayCommand(_ => ReportPlayer());
            ChangePasswordCommand = new RelayCommand(_ => ChangePassword());
            Username = CurrentPlayer.Instance.Username;
            Email = CurrentPlayer.Instance.Email;

            BestTimes = new ObservableCollection<string>();
            LeastTries = new ObservableCollection<string>();
            ReportedPlayer = Properties.Resources.ProfilePage_ReportPlayer;

            SetRecords();
            _ = LoadAvatarAsync();
        }

        private async Task LoadAvatarAsync()
        {
            Avatar = await AvatarLoader.GetCurrentPlayerAvatarAsync();
        }

        private void SetRecords()
        {
            var records = playerService.GetPlayerRecords(Username);
            int rankCounter = 1;
            foreach (var tries in records.TimeTrialRecords)
            {
                BestTimes.Add(string.Concat(rankCounter.ToString(), ") ", tries.ToString()));
                rankCounter++;
            }

            rankCounter = 1;
            foreach (var tries in records.TriesRecords)
            {
                LeastTries.Add(string.Concat(rankCounter.ToString(), ") ", tries.ToString()));
                rankCounter++;
            }
        }

        private void ReportPlayer()
        {
            var reportedPlayer = playerService.GetPlayerByUsername(ReportedPlayer);
            var report = new Report
            {
                ReportedPlayerUsername = Username,
                ReportedByPlayerUsername = CurrentPlayer.Instance.Username,
                Reason = "Cheating"
            };

            //*** TO DO ***//
            /*
            var result = playerService.ReportPlayer(report);
            if (result)
            {
                dialogService.ShowSuccess(Properties.Resources.SuccessNotification_PlayerReported);
            }
            else
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_PlayerReportedError);
            }*/
        }

        private void ChangePassword()
        {
            dialogService.ShowChangePassword();
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }
    }
}
