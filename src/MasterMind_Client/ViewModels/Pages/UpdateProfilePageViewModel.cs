using MasterMind_Client.ServerEndpoints;
using log4net;
using MasterMind_Client.AvatarService;
using MasterMind_Client.Enums;
using MasterMind_Client.AvatarManagement;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;
using System.IO;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MasterMind_Client.ViewModels.Pages
{
    public class UpdateProfilePageViewModel : ViewModelBase
    {
        private const string ProfilePageUri = "Pages/ProfilePage.xaml";
        private const int MaximumSourceBytes = 10485760;
        private const int RegexTimeoutMilliseconds = 500;

        private readonly static ILog logger = LogManager.GetLogger(typeof(UpdateProfilePageViewModel));

        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly PlayerServiceClient playerService = ServiceClientFactory.CreatePlayerClient();

        private string username;
        private string email;
        private ImageSource avatarPreview;
        private byte[] pendingAvatarBytes = Array.Empty<byte>();
        private bool isSaving;
        private string bestTimeText = string.Empty;
        private string leastTriesText = string.Empty;

        public UpdateProfilePageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !isSaving);
            CancelCommand = new RelayCommand(_ => NavigateTo(ProfilePageUri));
            ChooseAvatarCommand = new RelayCommand(_ => ChooseAvatar(), _ => !isSaving);
            username = CurrentPlayer.Instance.Username;
            email = CurrentPlayer.Instance.Email;
            avatarPreview = CurrentPlayer.Instance.Avatar ?? AvatarLoader.GetDefaultAvatar();
            SetRecords();
            _ = LoadCurrentAvatarAsync();
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

        public ImageSource AvatarPreview
        {
            get { return this.avatarPreview; }
            private set { SetProperty(ref this.avatarPreview, value); }
        }

        public string BestTimeText
        {
            get { return this.bestTimeText; }
            private set { SetProperty(ref this.bestTimeText, value); }
        }

        public string LeastTriesText
        {
            get { return this.leastTriesText; }
            private set { SetProperty(ref this.leastTriesText, value); }
        }

        public RelayCommand SaveCommand { get; }

        public RelayCommand CancelCommand { get; }

        public RelayCommand ChooseAvatarCommand { get; }

        private void SetRecords()
        {
            var records = CurrentPlayer.Instance.CachedRecords;
            if (records == null)
            {
                return;
            }

            BestTimeText = GetBestValue(records.TimeTrialRecords);
            LeastTriesText = GetBestValue(records.TriesRecords);
        }

        private static string GetBestValue(int[] values)
        {
            return values != null && values.Length > 0 ? values[0].ToString() : string.Empty;
        }

        private async Task LoadCurrentAvatarAsync()
        {
            var current = await AvatarLoader.GetCurrentPlayerAvatarAsync();

            if (pendingAvatarBytes.Length == 0)
            {
                AvatarPreview = current;
            }
        }

        private void ChooseAvatar()
        {
            var filePath = dialogService.PickImageFile();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            try
            {
                if (new FileInfo(filePath).Length > MaximumSourceBytes)
                {
                    dialogService.ShowError(Properties.Resources.ErrorNotification_ImageTooLarge);
                    return;
                }

                pendingAvatarBytes = AvatarImageCompressor.Compress(File.ReadAllBytes(filePath));
                AvatarPreview = CreatePreview(pendingAvatarBytes);
            }
            catch (IOException ex)
            {
                ReportInvalidImage(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                ReportInvalidImage(ex);
            }
            catch (Exception ex)
            {
                ReportInvalidImage(ex);
            }
        }

        private void ReportInvalidImage(Exception ex)
        {
            logger.Warn("The selected avatar image could not be loaded.", ex);
            dialogService.ShowError(Properties.Resources.ErrorNotification_InvalidImage);
        }

        private static BitmapImage CreatePreview(byte[] content)
        {
            var preview = new BitmapImage();

            using (var stream = new MemoryStream(content))
            {
                preview.BeginInit();
                preview.CacheOption = BitmapCacheOption.OnLoad;
                preview.StreamSource = stream;
                preview.EndInit();
            }

            preview.Freeze();

            return preview;
        }

        private async Task SaveAsync()
        {
            if (ValidateInput())
            {
                SetSaving(true);

                try
                {
                    var profileSaved = await SaveProfileAsync();
                    var avatarSaved = await SaveAvatarAsync();

                    if (profileSaved && avatarSaved)
                    {
                        NavigateTo(ProfilePageUri);
                    }
                }
                finally
                {
                    SetSaving(false);
                }
            }            
        }

        private void SetSaving(bool saving)
        {
            isSaving = saving;
            CommandManager.InvalidateRequerySuggested();
        }

        private bool HasProfileChanges()
        {
            return username != CurrentPlayer.Instance.Username || email != CurrentPlayer.Instance.Email;
        }

        private async Task<bool> SaveProfileAsync()
        {
            if (!HasProfileChanges())
            {
                return true;
            }

            try
            {
                var player = await playerService.GetPlayerByUsernameAsync(CurrentPlayer.Instance.Username);
                player.Username = username;
                player.Email = email;
                var result = await playerService.UpdatePlayerAsync(player);

                return HandleProfileResult(result, player);
            }
            catch (FaultException<PlayerService.ErrorMessage> ex)
            {
                logger.Warn("The server rejected the profile update: " + ex.Detail.Message, ex);
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

            return false;
        }

        private bool HandleProfileResult(string result, Player player)
        {
            if (result.Equals(ServerMessages.Success.ToString()))
            {
                CurrentPlayer.Instance.SetCurrentPlayer(player);
                dialogService.ShowSuccess(Properties.Resources.SuccessNotification_PlayerUpdated);

                return true;
            }

            if (result.Equals(ServerMessages.EmailTaken.ToString()))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_EmailTaken);
            }
            else if (result.Equals(ServerMessages.UsernameTaken.ToString()))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_UsernameTaken);
            }
            else
            {
                logger.WarnFormat("Unexpected profile update result: {0}", result);
                dialogService.ShowServerError(result);
            }

            return false;
        }

        private async Task<bool> SaveAvatarAsync()
        {
            if (pendingAvatarBytes.Length == 0)
            {
                return true;
            }

            try
            {
                var fileName = await SendAvatarAsync(pendingAvatarBytes);
                CompleteAvatarSave(fileName);

                return true;
            }
            catch (FaultException<AvatarService.ErrorMessage> ex)
            {
                logger.Warn("The server rejected the avatar: " + ex.Detail.Message, ex);
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

            return false;
        }

        private static async Task<string> SendAvatarAsync(byte[] content)
        {
            var avatarClient = ServiceClientFactory.CreateAvatarClient();

            try
            {
                return await avatarClient.SaveAvatarAsync(CurrentPlayer.Instance.Id, content);
            }
            finally
            {
                CloseClient(avatarClient);
            }
        }

        private static void CloseClient(AvatarServiceClient avatarClient)
        {
            try
            {
                avatarClient.Close();
            }
            catch (CommunicationException ex)
            {
                logger.Warn(ex);
                avatarClient.Abort();
            }
            catch (TimeoutException ex)
            {
                logger.Warn(ex);
                avatarClient.Abort();
            }
        }

        private void CompleteAvatarSave(string fileName)
        {
            AvatarCache.Save(AvatarZone.Own, fileName, pendingAvatarBytes);
            pendingAvatarBytes = Array.Empty<byte>();
            CurrentPlayer.Instance.SetAvatar(AvatarPreview, fileName);
            dialogService.ShowSuccess(Properties.Resources.SuccessNotification_AvatarUpdated);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);

                return false;
            }

            var timeout = TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds);
            if (Regex.IsMatch(Username, @"\s", RegexOptions.None, timeout))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_NoSpaces);

                return false;
            }

            return true;
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }
    }
}
