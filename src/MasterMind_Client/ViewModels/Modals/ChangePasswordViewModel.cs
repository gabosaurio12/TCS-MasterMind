using MasterMind_Client.CodeService;
using MasterMind_Client.Helpers;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using log4net;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;

namespace MasterMind_Client.ViewModels.Modals
{
    public class ChangePasswordViewModel : ModalViewModelBase
    {
        private const string SuccessResult = "Success";

        private readonly static ILog logger = LogManager.GetLogger(typeof(ChangePasswordViewModel));

        private readonly IDialogService dialogService;
        private readonly PlayerServiceClient playerService;
        private readonly CodeServiceClient codeService;
        private readonly string username;

        private string password;
        private string pendingPassword;
        private bool isSaving;

        public ChangePasswordViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            playerService = new PlayerServiceClient();
            codeService = new CodeServiceClient();
            username = CurrentPlayer.Instance.Username;
            SaveCommand = new RelayCommand(_ => Save(), _ => !isSaving);
            CancelCommand = new RelayCommand(_ => RequestClose());
        }

        public string Password
        {
            get { return password; }
            set { SetProperty(ref password, value); }
        }

        public RelayCommand SaveCommand { get; }

        public RelayCommand CancelCommand { get; }

        private void Save()
        {
            if (isSaving)
            {
                return;
            }

            if (!ValidatePassword())
            {
                return;
            }

            pendingPassword = Password;
            isSaving = true;
            CommandManager.InvalidateRequerySuggested();

            if (!SendVerificationCode())
            {
                pendingPassword = null;
                isSaving = false;
                CommandManager.InvalidateRequerySuggested();
                return;
            }

            RequestClose();
            dialogService.ShowVerificationCode(username, OnCodeVerified);
        }

        private bool ValidatePassword()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                return false;
            }

            if (!PasswordValidator.IsValid(Password))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_InvalidPassword);
                return false;
            }

            return true;
        }

        private bool SendVerificationCode()
        {
            try
            {
                var player = playerService.GetPlayerByUsername(username);

                if (player == null)
                {
                    ShowError(Properties.Resources.ErrorNotification_UsernameNotFound);
                    return false;
                }

                codeService.SendVerificationCode(player.PlayerId);
                return true;
            }
            catch (FaultException<PlayerService.ErrorMessage> faultException)
            {
                logger.WarnFormat("The server rejected the verification code request: {0}",
                    faultException.Detail.Message);
                ShowServerError(faultException.Detail.Message);
                return false;
            }
            catch (FaultException<CodeService.ErrorMessage> faultException)
            {
                logger.WarnFormat("The server rejected the verification code request: {0}",
                    faultException.Detail.Message);
                ShowServerError(faultException.Detail.Message);
                return false;
            }
            catch (FaultException faultException)
            {
                logger.Error("Server fault while sending the verification code", faultException);
                ShowServerError(faultException.Message);
                return false;
            }
            catch (CommunicationException communicationException)
            {
                logger.Error("Communication failure while sending the verification code", communicationException);
                ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
                return false;
            }
        }

        private void OnCodeVerified(Player verifiedPlayer)
        {
            try
            {
                logger.InfoFormat("The verification code of the player {0} was confirmed", verifiedPlayer.PlayerId);
                var result = playerService.ChangePassword(username, pendingPassword);

                if (result.Equals(SuccessResult))
                {
                    ShowSuccess(Properties.Resources.SuccessNotification_PasswordChanged);
                }
                else
                {
                    ShowError(Properties.Resources.ErrorNotification_UnexpectedError);
                }
            }
            catch (FaultException<PlayerService.ErrorMessage> faultException)
            {
                logger.WarnFormat("The server rejected the password change: {0}", faultException.Detail.Message);
                ShowServerError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error("Server fault during the password change", faultException);
                ShowServerError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error("Communication failure during the password change", communicationException);
                ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }
            finally
            {
                pendingPassword = null;
            }
        }

        private void ShowServerError(string errorDetail)
        {
            switch (errorDetail)
            {
                case "DataBaseError":
                    ShowError(Properties.Resources.ErrorNotification_DatabaseError);
                    break;
                default:
                    logger.WarnFormat("Unknown server error: {0}", errorDetail);
                    ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
                    break;
            }
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => dialogService.ShowError(message)));
        }

        private void ShowSuccess(string message)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => dialogService.ShowSuccess(message)));
        }
    }
}
