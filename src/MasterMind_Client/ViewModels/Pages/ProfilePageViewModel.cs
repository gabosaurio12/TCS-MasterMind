using MasterMind_Client.AvatarManagement;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MasterMind_Client.ViewModels.Pages
{
    public class ProfilePageViewModel : ViewModelBase
    {
        private const string InappropriateLanguageReason = "InappropriateLanguage";
        private const string CheatingReason = "Cheating";

        private static readonly ILog logger = LogManager.GetLogger(typeof(ProfilePageViewModel));

        private ImageSource avatar = AvatarLoader.GetDefaultAvatar();

        private string reportedPlayer;

        private string selectedReason = InappropriateLanguageReason;

        private bool isReporting;

        public string Username { get; }

        public ImageSource Avatar
        {
            get { return this.avatar; }
            private set { SetProperty(ref this.avatar, value); }
        }

        public string Email { get; }

        public string ReportedPlayer
        {
            get { return this.reportedPlayer; }
            set { SetProperty(ref this.reportedPlayer, value); }
        }

        public string SelectedReason
        {
            get { return this.selectedReason; }
            set { SetProperty(ref this.selectedReason, value); }
        }

        public IReadOnlyList<KeyValuePair<string, string>> ReportReasons { get; } = new[]
        {
            new KeyValuePair<string, string>(
                InappropriateLanguageReason, Properties.Resources.ProfilePage_ReportReasonInappropriateLanguage),
            new KeyValuePair<string, string>(CheatingReason, Properties.Resources.ProfilePage_ReportReasonCheating)
        };

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
            ReportCommand = new RelayCommand(async _ => await ReportPlayerAsync(), _ => !isReporting);
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

        private async Task ReportPlayerAsync()
        {
            var reportedUsername = (ReportedPlayer ?? string.Empty).Trim();

            if (!IsReportTargetValid(reportedUsername))
            {
                return;
            }

            SetReporting(true);

            try
            {
                await playerService.ReportPlayerAsync(BuildReport(reportedUsername));
                dialogService.ShowSuccess(Properties.Resources.SuccessNotification_PlayerReported);
                ReportedPlayer = Properties.Resources.ProfilePage_ReportPlayer;
            }
            catch (FaultException<PlayerService.ErrorMessage> ex)
            {
                logger.Warn("The server rejected the report: " + ex.Detail.Message, ex);
                ShowReportError(ex.Detail.Message);
            }
            catch (FaultException ex)
            {
                logger.Error("Server fault while reporting a player", ex);
                dialogService.ShowError(Properties.Resources.ErrorNotification_PlayerReportedError);
            }
            catch (CommunicationException ex)
            {
                logger.Error("Communication failure while reporting a player", ex);
                dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }
            catch (TimeoutException ex)
            {
                logger.Error("Timeout while reporting a player", ex);
                dialogService.ShowError(Properties.Resources.ErrorNotification_Timeout);
            }
            finally
            {
                SetReporting(false);
            }
        }

        private bool IsReportTargetValid(string reportedUsername)
        {
            if (reportedUsername.Length == 0
                || reportedUsername.Equals(Properties.Resources.ProfilePage_ReportPlayer.Trim()))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                return false;
            }

            if (reportedUsername.Equals(Username, StringComparison.OrdinalIgnoreCase))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_SelfReport);
                return false;
            }

            return true;
        }

        private Report BuildReport(string reportedUsername)
        {
            return new Report
            {
                ReportedPlayerUsername = reportedUsername,
                ReportedByPlayerUsername = Username,
                Reason = SelectedReason
            };
        }

        private void ShowReportError(string detailMessage)
        {
            switch (detailMessage)
            {
                case "PlayerNotFound":
                    dialogService.ShowError(Properties.Resources.ErrorNotification_UsernameNotFound);
                    break;
                case "SelfReport":
                    dialogService.ShowError(Properties.Resources.ErrorNotification_SelfReport);
                    break;
                case "AlreadyReported":
                    dialogService.ShowError(Properties.Resources.ErrorNotification_AlreadyReported);
                    break;
                case "DataBaseError":
                    dialogService.ShowError(Properties.Resources.ErrorNotification_DatabaseError);
                    break;
                default:
                    dialogService.ShowError(Properties.Resources.ErrorNotification_PlayerReportedError);
                    break;
            }
        }

        private void SetReporting(bool reporting)
        {
            isReporting = reporting;
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
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
