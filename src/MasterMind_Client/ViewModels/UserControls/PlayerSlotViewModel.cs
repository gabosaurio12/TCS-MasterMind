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
            backgroundBrush = UtilsUI.PlayerSlotDefaultBackground;
        }

        public string Username { get; }

        public Brush BackgroundBrush
        {
            get => backgroundBrush;
            private set => SetProperty(ref backgroundBrush, value);
        }

        private Brush backgroundBrush;

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
