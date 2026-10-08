using MasterMind_Client.Assets;
using MasterMind_Client.ViewModels.Base;
using System.Windows.Media;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class PlayerSlotViewModel : ViewModelBase
    {
        public PlayerSlotViewModel(string username)
        {
            Username = username;
            _backgroundBrush = UtilsUI.PlayerSlotDefaultBackground;
        }

        public string Username { get; }

        public Brush BackgroundBrush
        {
            get => _backgroundBrush;
            private set => SetProperty(ref _backgroundBrush, value);
        }

        private Brush _backgroundBrush;

        public void SetReady()
        {
            BackgroundBrush = UtilsUI.OnlineGreen;
        }

        public void SetNotReady()
        {
            BackgroundBrush = UtilsUI.NotReadyYellow;
        }
    }
}
