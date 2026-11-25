using MasterMind_Client.AvatarManagement;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ServerEndpoints;
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

        private readonly PlayerServiceClient playerService = ServiceClientFactory.CreatePlayerClient();

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
            try
            {
                var records = playerService.GetPlayerRecords(Username);
                CurrentPlayer.Instance.SetCachedRecords(records);
                FillRecords(records);
            }
            catch (FaultException<ErrorMessage> ex)
            {
                logger.WarnFormat("The server rejected the records request: {0}", ex.Detail.Message);
                dialogService.ShowServerError(ex.Detail.Message);
            }
            catch (FaultException ex)
            {
                logger.Error(ex);
                dialogService.ShowServerError(ex.Message);
            }
            catch (CommunicationException ex)
            {
                logger.Error(ex);
                dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }
            catch (TimeoutException ex)
            {
                logger.Error(ex);
                dialogService.ShowError(Properties.Resources.ErrorNotification_Timeout);
            }
        }

        private void FillRecords(PlayerRecords records)
        {
            FillRecordList(BestTimes, records.TimeTrialRecords);
            FillRecordList(LeastTries, records.TriesRecords);
        }

        private static void FillRecordList(ObservableCollection<string> target, int[] values)
        {
            if (values == null)
            {
                return;
            }

            for (int index = 0; index < values.Length; index++)
            {
                target.Add(string.Concat((index + 1).ToString(), ") ", values[index].ToString()));
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
            catch (FaultException<ErrorMessage> ex)
            {
                logger.Warn(ex);
                ShowReportError(ex.Detail.Message);
            }
            catch (FaultException ex)
            {
                logger.Error(ex);
                dialogService.ShowError(Properties.Resources.ErrorNotification_PlayerReportedError);
            }
            catch (CommunicationException ex)
            {
                logger.Error(ex);
                dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }
            catch (TimeoutException ex)
            {
                logger.Error(ex);
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
                case "DbServerError":
                    dialogService.ShowServerError(detailMessage);
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
