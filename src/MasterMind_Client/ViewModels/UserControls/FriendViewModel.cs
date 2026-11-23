using MasterMind_Client.AvatarManagement;
using MasterMind_Client.Helpers;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Base;
using System;
using System.Threading.Tasks;
using System.Windows.Media;
using MasterMind_Client.ServerEndpoints;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class FriendViewModel : ViewModelBase
    {
        private ImageSource avatar = AvatarLoader.GetDefaultAvatar();

        public ImageSource Avatar
        {
            get { return this.avatar; }
            private set { SetProperty(ref this.avatar, value); }
        }

        public string Username { get; }

        public Brush OnlineBrush { get; }

        public RelayCommand RemoveCommand { get; }

        public event EventHandler Removed;

        public FriendViewModel(string username)
        {
            Username = username;
            OnlineBrush = UtilsUI.OfflineGray;
            RemoveCommand = new RelayCommand(_ => Removed?.Invoke(this, EventArgs.Empty));
            var player = ServiceClientFactory.CreatePlayerClient().GetPlayerByUsername(username);
            _ = LoadAvatarAsync(player);
        }

        private async Task LoadAvatarAsync(Player requester)
        {
            Avatar = await AvatarLoader.GetAvatarAsync(requester.PlayerId, requester.AvatarPath);
        }
    }
}