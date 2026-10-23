using MasterMind_Client.FriendsService;
using MasterMind_Client.TempData;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System.Collections.ObjectModel;

namespace MasterMind_Client.ViewModels.Modals
{
    public class FriendsViewModel : ModalViewModelBase
    {
        public ObservableCollection<FriendViewModel> Friends { get; }

        public RelayCommand CloseCommand { get; }

        public RelayCommand FriendRequestsCommand { get; }

        private readonly IDialogService dialogService;

        public FriendsViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            Friends = new ObservableCollection<FriendViewModel>();
            CloseCommand = new RelayCommand(_ => RequestClose());
            FriendRequestsCommand = new RelayCommand(_ => OpenFriendRequests());
            LoadFriends();
        }

        private void LoadFriends()
        {
            var friendService = new FriendsServiceClient();
            var friends = friendService.GetFriendships(CurrentPlayer.Instance.Id);
            foreach (var friend in friends)
            {
                Friends.Add(new FriendViewModel(friend.Username));
            }
        }

        private void OpenFriendRequests()
        {
            dialogService.ShowFriendRequests();
            RequestClose();
        }
    }
}
