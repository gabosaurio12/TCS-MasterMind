using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.Modals
{
    public class ErrorNotificationViewModel : ModalViewModelBase
    {
        public ErrorNotificationViewModel(string message)
        {
            Message = message;
            ContinueCommand = new RelayCommand(_ => RequestClose());
        }

        public string Message { get; }

        public RelayCommand ContinueCommand { get; }
    }
}
