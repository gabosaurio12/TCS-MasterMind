using MasterMind_Client.FriendsService;
using MasterMind_Client.Helpers;
using MasterMind_Client.ViewModels.Base;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class FriendViewModel : ViewModelBase
    {
        private ImageSource avatar = AvatarLoader.GetDefaultAvatar();

        public FriendViewModel(Player friend)
        {
            Username = friend.Username;
            OnlineBrush = UtilsUI.OfflineGray;
            RemoveCommand = new RelayCommand(_ => Removed?.Invoke(this, EventArgs.Empty));
            _ = LoadAvatarAsync(friend);
        }

        public string Username { get; }

        public ImageSource Avatar
        {
            get { return this.avatar; }
            private set { SetProperty(ref this.avatar, value); }
        }

        public Brush OnlineBrush { get; }

        public RelayCommand RemoveCommand { get; }

        public event EventHandler Removed;

        private async Task LoadAvatarAsync(Player friend)
        {
            Avatar = await AvatarLoader.GetAvatarAsync(friend.PlayerId, friend.AvatarPath);
        }
    }
}
