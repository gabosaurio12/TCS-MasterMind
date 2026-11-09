using MasterMind_Client.ViewModels.Base;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class PlayerSlotViewModel : ViewModelBase
    {
        public PlayerSlotViewModel(string username)
        {
            Username = username;
        }

        public string Username { get; }

        public bool IsReady
        {
            get => isReady;
            private set => SetProperty(ref isReady, value);
        }

        private bool isReady;

        public void SetReady()
        {
            IsReady = true;
        }

        public void SetNotReady()
        {
            IsReady = false;
        }
    }
}
