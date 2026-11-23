using MasterMind_Client.CodeService;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ServerEndpoints;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using log4net;
using System;
using System.ServiceModel;

namespace MasterMind_Client.ViewModels.Modals
{
    public class VerificationCodeViewModel : ModalViewModelBase
    {
        public VerificationCodeViewModel(string username, IDialogService dialogService)
        {
            this.username = username;
            this.dialogService = dialogService;
            ContinueCommand = new RelayCommand(_ => Continue());
            CancelCommand = new RelayCommand(_ => RequestClose());
            ResendCodeCommand = new RelayCommand(_ => ResendCode());
        }

        public event EventHandler<Player> VerificationSucceded;

        public RelayCommand ContinueCommand { get; }

        public RelayCommand CancelCommand { get; }

        public RelayCommand ResendCodeCommand { get; }

        public string CodeDigit0 { get; set; }
        public string CodeDigit1 { get; set; }
        public string CodeDigit2 { get; set; }
        public string CodeDigit3 { get; set; }
        public string CodeDigit4 { get; set; }

        private const string ExpiredCodeFault = "VerificationCodeExpired";
        private static readonly ILog logger = LogManager.GetLogger(typeof(VerificationCodeViewModel));
        private readonly string username;
        private readonly IDialogService dialogService;

        private void ResendCode()
        {
            var playerService = ServiceClientFactory.CreatePlayerClient();
            var player = playerService.GetPlayerByUsername(username);
            if (player == null)
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_UsernameNotFound);
                return;
            }

            var codeService = ServiceClientFactory.CreateCodeClient();
            codeService.SendVerificationCode(player.PlayerId);
            dialogService.ShowSuccess(Properties.Resources.VerificationCodeModal_CodeResent);
        }

        private void Continue()
        {
            string code = string.Concat(CodeDigit0, CodeDigit1, CodeDigit2, CodeDigit3, CodeDigit4);
            try
            {
                var service = ServiceClientFactory.CreateCodeClient();
                if (service.AuthVerificationCode(username, code))
                {
                    CompleteVerification();
                }
                else
                {
                    dialogService.ShowError(Properties.Resources.ErrorNotification_WrongCode);
                }
            }
            catch (FaultException<CodeService.ErrorMessage> faultException)
            {
                logger.Warn(faultException);
                dialogService.ShowError(GetFaultMessage(faultException.Detail.Message));
            }
            catch (CommunicationException communicationException)
            {
                logger.Error(communicationException);
                dialogService.ShowError(Properties.Resources.ErrorNotification_Timeout);
            }
        }

        private static string GetFaultMessage(string detail)
        {
            switch (detail)
            {
                case ExpiredCodeFault:
                    return Properties.Resources.ErrorNotification_CodeExpired;
                default:
                    return Properties.Resources.ErrorNotification_DatabaseError;
            }
        }

        private void CompleteVerification()
        {
            var playerService = ServiceClientFactory.CreatePlayerClient();
            var player = playerService.GetPlayerByUsername(username);
            if (player != null)
            {
                VerificationSucceded?.Invoke(this, player);
            }
            RequestClose();
        }
    }
}
