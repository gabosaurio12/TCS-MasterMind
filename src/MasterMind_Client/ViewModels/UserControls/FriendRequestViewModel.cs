using MasterMind_Client.AvatarManagement;
using MasterMind_Client.FriendsService;
using MasterMind_Client.Helpers;
using MasterMind_Client.ViewModels.Base;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MasterMind_Client.ViewModels.UserControls
{
    public class FriendRequestViewModel : ViewModelBase
    {
        private ImageSource avatar = AvatarLoader.GetDefaultAvatar();

        public string Username { get; }

        public ImageSource Avatar
        {
            get { return this.avatar; }
            private set { SetProperty(ref this.avatar, value); }
        }

        public Brush OnlineBrush { get; }

        public Friendship Request { get; }

        public RelayCommand AcceptCommand { get; }

        public RelayCommand RejectCommand { get; }

        public event EventHandler<Friendship> Accepted;

        public event EventHandler<Friendship> Rejected;

        public FriendRequestViewModel(Player requester, Friendship request)
        {
            Username = requester.Username;
            Request = request;
            OnlineBrush = UtilsUI.OfflineGray;
            AcceptCommand = new RelayCommand(_ => Accepted?.Invoke(this, Request));
            RejectCommand = new RelayCommand(_ => Rejected?.Invoke(this, Request));
            _ = LoadAvatarAsync(requester);
        }

        private async Task LoadAvatarAsync(Player requester)
        {
            Avatar = await AvatarLoader.GetAvatarAsync(requester.PlayerId, requester.AvatarPath);
        }
    }
}
