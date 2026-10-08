using System;
using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.Modals
{
    public class SuccessNotificationViewModel : ModalViewModelBase
    {
        public SuccessNotificationViewModel(string message, bool isRegisterModal)
        {
            Message = message;
            this.isRegisterModal = isRegisterModal;
            ContinueCommand = new RelayCommand(_ => Continue());
        }

        public event EventHandler ModalClosed;

        public string Message { get; }

        public RelayCommand ContinueCommand { get; }

        private readonly bool isRegisterModal;

        private void Continue()
        {
            if (isRegisterModal)
            {
                ModalClosed?.Invoke(this, EventArgs.Empty);
            }

            RequestClose();
        }
    }
}
