using MasterMind_Client.Data;
using MasterMind_Client.TempData;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;

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
        }

        public event EventHandler<Player> VerificationSucceded;

        public RelayCommand ContinueCommand { get; }

        public RelayCommand CancelCommand { get; }

        public string CodeDigit0 { get; set; }
        public string CodeDigit1 { get; set; }
        public string CodeDigit2 { get; set; }
        public string CodeDigit3 { get; set; }
        public string CodeDigit4 { get; set; }

        private readonly string username;
        private readonly IDialogService dialogService;

        private void Continue()
        {
            string code = string.Concat(CodeDigit0, CodeDigit1, CodeDigit2, CodeDigit3, CodeDigit4);
            var result = TempAuthService.AuthVerificationCode(username, code);
            if (result.Item1)
            {
                VerificationSucceded?.Invoke(this, result.Item2);
                RequestClose();
            }
            else
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WrongCode);
            }
        }
    }
}
