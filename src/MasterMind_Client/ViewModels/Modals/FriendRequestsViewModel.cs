using MasterMind_Client.FriendsService;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System.Collections.ObjectModel;

namespace MasterMind_Client.ViewModels.Modals
{
    public class FriendRequestsViewModel : ModalViewModelBase
    {

        public ObservableCollection<FriendRequestViewModel> Requests { get; }
        public string AddresseeUsername { get; set; } = Properties.Resources.FriendRequestsModal_AddFriendText;
        public RelayCommand SendCommand { get; }
        public RelayCommand CloseCommand { get; }
        private readonly IDialogService dialogService;
        private readonly FriendsServiceClient friendService = new FriendsServiceClient();


        public FriendRequestsViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            Requests = new ObservableCollection<FriendRequestViewModel>();
            SendCommand = new RelayCommand(_ => SendFriendRequest());
            CloseCommand = new RelayCommand(_ => Close());
            LoadRequests();
        }

        private void LoadRequests()
        {
            var requests = friendService.GetFriendRequests(CurrentPlayer.Instance.Id);
            foreach (var request in requests)
            {
                AddRequest(request);
            }
        }

        private void AddRequest(FriendsService.Player requester)
        {
            var friendRequest = friendService.GetFriendRequest(requester.PlayerId, CurrentPlayer.Instance.Id);
            var requestControl = new FriendRequestViewModel(requester.Username, friendRequest);
            requestControl.Accepted += OnRequestAccepted;
            requestControl.Rejected += OnRequestRejected;
            Requests.Add(requestControl);
        }

        private void OnRequestAccepted(object sender, Friendship request)
        {
            friendService.AcceptFriendRequest(request.FriendshipId);
            Requests.Remove((FriendRequestViewModel)sender);
        }

        private void OnRequestRejected(object sender, Friendship request)
        {
            friendService.RejectFriendRequest(request.FriendshipId);
            Requests.Remove((FriendRequestViewModel)sender);
        }

        private void SendFriendRequest()
        {
            var playerService = new PlayerServiceClient();
            var username = AddresseeUsername.Trim();
            var addressee = playerService.GetPlayerByUsername(username);
            if (addressee != null)
            {

                // to-do: pending functionality

                var result = friendService.SendFriendRequest(CurrentPlayer.Instance.Id, addressee.PlayerId);

                switch (result)
                {
                    case "Success":
                        dialogService.ShowSuccess(Properties.Resources.SuccessNotification_FriendRequestSent);
                        break;
                    case "Pending":
                        dialogService.ShowError(Properties.Resources.ErrorNotification_FriendRequestPending);
                        break;
                    case "Error":
                        dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorSendingFriendRequest);
                        break;
                }
            }
            else
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_UsernameNotFound);
            }
        }

        private void Close()
        {
            dialogService.ShowFriends();
            RequestClose();
        }
    }
}
