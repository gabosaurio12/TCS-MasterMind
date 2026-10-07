using MasterMind_Client.Data;
using MasterMind_Client.TempData;
using MasterMind_Client.TempData.Enum;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System;
using System.Collections.ObjectModel;

namespace MasterMind_Client.ViewModels.Modals
{
    public class FriendRequestsViewModel : ModalViewModelBase
    {
        public FriendRequestsViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            Requests = new ObservableCollection<FriendRequestViewModel>();
            SendCommand = new RelayCommand(_ => SendFriendRequest());
            CloseCommand = new RelayCommand(_ => Close());
            LoadRequests();
        }

        public ObservableCollection<FriendRequestViewModel> Requests { get; }

        public string AddresseeUsername { get; set; } = Properties.Resources.FriendRequestsModal_AddFriendText;

        public RelayCommand SendCommand { get; }

        public RelayCommand CloseCommand { get; }

        private readonly IDialogService dialogService;

        private void LoadRequests()
        {
            var requests = FriendshipService.GetFriendRequests(CurrentPlayer.Instance.Id);
            foreach (var request in requests)
            {
                AddRequest(request);
            }
        }

        private void AddRequest(Player requester)
        {
            var requestControl = new FriendRequestViewModel(
                requester.username,
                FriendshipService.GetFriendRequest(requester.player_id, CurrentPlayer.Instance.Id));
            requestControl.Accepted += OnRequestAccepted;
            requestControl.Rejected += OnRequestRejected;
            Requests.Add(requestControl);
        }

        private void OnRequestAccepted(object sender, Friendship request)
        {
            FriendshipService.AcceptFriendRequest(request.friendship_id);
            Requests.Remove((FriendRequestViewModel)sender);
        }

        private void OnRequestRejected(object sender, Friendship request)
        {
            FriendshipService.RejectFriendRequest(request.friendship_id);
            Requests.Remove((FriendRequestViewModel)sender);
        }

        private void SendFriendRequest()
        {
            var username = AddresseeUsername.Trim();
            int addresseeId = TempPlayerService.GetPlayerByUsername(username).player_id;
            var result = FriendshipService.SendFrienshipRequest(CurrentPlayer.Instance.Id, addresseeId);

            switch (result)
            {
                case RequestStatusEnum.Success:
                    dialogService.ShowSuccess(Properties.Resources.SuccessNotification_FriendRequestSent);
                    break;
                case RequestStatusEnum.Pending:
                    dialogService.ShowError(Properties.Resources.ErrorNotification_FriendRequestPending);
                    break;
                case RequestStatusEnum.Error:
                    dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorSendingFriendRequest);
                    break;
            }
        }

        private void Close()
        {
            dialogService.ShowFriends();
            RequestClose();
        }
    }
}
