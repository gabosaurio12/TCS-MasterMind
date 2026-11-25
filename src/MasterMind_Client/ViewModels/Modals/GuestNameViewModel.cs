using MasterMind_Client.Helpers;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ServerEndpoints;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MasterMind_Client.ViewModels.Modals
{
    public class GuestNameViewModel : ModalViewModelBase
    {
        private const string SuccessResult = "Success";
        private const string UsernameTakenResult = "UsernameTaken";

        private static readonly ILog logger = LogManager.GetLogger(typeof(GuestNameViewModel));

        private readonly IDialogService dialogService;

        private string username;
        private bool isEntering;

        public GuestNameViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            PlayCommand = new RelayCommand(async _ => await PlayAsync(), _ => !isEntering);
            CancelCommand = new RelayCommand(_ => RequestClose());
        }

        public event EventHandler GuestEntered;

        public string Username
        {
            get { return username; }
            set { SetProperty(ref username, value); }
        }

        public RelayCommand PlayCommand { get; }

        public RelayCommand CancelCommand { get; }

        private async Task PlayAsync()
        {
            if (isEntering || !ValidateUsername())
            {
                return;
            }

            isEntering = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                var guest = await CreateGuestAsync(Username.Trim());
                if (guest != null)
                {
                    CurrentPlayer.Instance.SetGuest(guest);
                    GuestEntered?.Invoke(this, EventArgs.Empty);
                }
            }
            finally
            {
                isEntering = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool ValidateUsername()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                return false;
            }

            if (UsernameValidator.HasWhitespace(Username.Trim()))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_NoSpaces);
                return false;
            }

            return true;
        }

        private async Task<Player> CreateGuestAsync(string guestName)
        {
            try
            {
                var client = ServiceClientFactory.CreatePlayerClient();
                var result = await client.CreateGuestAsync(guestName);
                if (result != SuccessResult)
                {
                    ShowRejection(result);
                    return null;
                }

                return await client.GetPlayerByUsernameAsync(guestName);
            }
            catch (FaultException<ErrorMessage> faultException)
            {
                logger.WarnFormat("The server rejected the guest creation: {0}", faultException.Detail.Message);
                dialogService.ShowServerError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error("Server fault while creating the guest", faultException);
                dialogService.ShowServerError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error("Communication failure while creating the guest", communicationException);
                dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }

            return null;
        }

        private void ShowRejection(string result)
        {
            if (result == UsernameTakenResult)
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_UsernameTaken);
            }
            else
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_UnexpectedError);
            }
        }
    }
}
