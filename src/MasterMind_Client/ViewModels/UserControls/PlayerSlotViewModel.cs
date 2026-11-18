using MasterMind_Client.Helpers;
using MasterMind_Client.ViewModels.Base;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class PlayerSlotViewModel : ViewModelBase
    {
        public PlayerSlotViewModel(string username)
        {
            Username = username;
        }

        public PlayerSlotViewModel(string username, int playerId, string avatarPath) : this(username)
        {
            _ = LoadAvatarAsync(playerId, avatarPath);
        }

        public string Username { get; }

        public ImageSource Avatar
        {
            get => avatar;
            private set => SetProperty(ref avatar, value);
        }

        public bool IsReady
        {
            get => isReady;
            private set => SetProperty(ref isReady, value);
        }

        private bool isReady;

        private ImageSource avatar = AvatarLoader.GetDefaultAvatar();

        public void SetReady()
        {
            IsReady = true;
        }

        public void SetNotReady()
        {
            IsReady = false;
        }

        private async Task LoadAvatarAsync(int playerId, string avatarPath)
        {
            Avatar = await AvatarLoader.GetAvatarAsync(playerId, avatarPath);
        }
    }
}
