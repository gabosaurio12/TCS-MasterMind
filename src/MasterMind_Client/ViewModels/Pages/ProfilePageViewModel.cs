using MasterMind_Client.TempData;
using MasterMind_Client.TempData.Enum;
using MasterMind_Client.TempData.DTO;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;
using System.Collections.ObjectModel;

namespace MasterMind_Client.ViewModels.Pages
{
    public class ProfilePageViewModel : ViewModelBase
    {
        public ProfilePageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            UpdateCommand = new RelayCommand(_ => NavigateTo("Pages/UpdateProfilePage.xaml"));
            BackCommand = new RelayCommand(_ => NavigateTo("Pages/MainPage.xaml"));
            ReportCommand = new RelayCommand(_ => ReportPlayer());
            ChangePasswordCommand = new RelayCommand(_ => { });
            Username = CurrentPlayer.Instance.Username;
            Email = CurrentPlayer.Instance.Email;

            BestTimes = new ObservableCollection<string>();
            LeastTries = new ObservableCollection<string>();
            ReportedPlayer = Properties.Resources.ProfilePage_ReportPlayer;

            SetRecords();
        }

        public string Username { get; }

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

        private void SetRecords()
        {
            var records = TempPlayerService.GetPlayerRecords(Username);
            int rankCounter = 1;
            foreach (var tries in records.PlayerTimeTrialRecords)
            {
                BestTimes.Add(string.Concat(rankCounter.ToString(), ") ", tries.ToString()));
                rankCounter++;
            }

            rankCounter = 1;
            foreach (var tries in records.PlayerTriesRecords)
            {
                LeastTries.Add(string.Concat(rankCounter.ToString(), ") ", tries.ToString()));
                rankCounter++;
            }
        }

        private void ReportPlayer()
        {
            var reportedPlayer = TempPlayerService.GetPlayerByUsername(ReportedPlayer);
            var report = new PlayerReportDto
            {
                ReportedPlayerUsername = Username,
                ReportedByPlayerUsername = CurrentPlayer.Instance.Username,
                Reason = ReportReasonEnum.Cheating
            };
            var result = TempPlayerService.ReportPlayer(report);
            if (result)
            {
                dialogService.ShowSuccess(Properties.Resources.SuccessNotification_PlayerReported);
            }
            else
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_PlayerReportedError);
            }
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }
    }
}
