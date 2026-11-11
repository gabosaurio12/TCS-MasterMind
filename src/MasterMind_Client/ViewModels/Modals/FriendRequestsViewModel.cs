using log4net;
using MasterMind_Client.Enums;
using MasterMind_Client.FriendsService;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System.Collections.ObjectModel;
using System.ServiceModel;

namespace MasterMind_Client.ViewModels.Modals
{
    public class FriendRequestsViewModel : ModalViewModelBase, IFriendsServiceCallback
    {

        private readonly static ILog logger = LogManager.GetLogger(typeof(FriendRequestsViewModel));

        public ObservableCollection<FriendRequestViewModel> Requests { get; }
        public string AddresseeUsername { get; set; } = Properties.Resources.FriendRequestsModal_AddFriendText;
        public RelayCommand SendCommand { get; }
        public RelayCommand CloseCommand { get; }
        private readonly IDialogService dialogService;
        private readonly FriendsServiceClient friendService;


        public FriendRequestsViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            InstanceContext instanceContext = new InstanceContext(this);
            friendService = new FriendsServiceClient(instanceContext);

            Requests = new ObservableCollection<FriendRequestViewModel>();
            SendCommand = new RelayCommand(_ => SendFriendRequest());
            CloseCommand = new RelayCommand(_ => Close());

            LoadRequests();
            RegisterCallback(CurrentPlayer.Instance.Id);
        }

        private void LoadRequests()
        {
            var requests = friendService.GetFriendRequests(CurrentPlayer.Instance.Id);
            foreach (var request in requests)
            {
                AddRequest(request);
            }
        }

        private void ClearRequests()
        {
            foreach (var request in Requests)
            {
                request.Accepted -= OnRequestAccepted;
                request.Rejected -= OnRequestRejected;
            }
            Requests.Clear();
        }

        private void AddRequest(FriendsService.Player requester)
        {
            var friendRequest = friendService.GetFriendRequest(CurrentPlayer.Instance.Id, requester.PlayerId);
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
            if (addressee.PlayerId > 0)
            {
                try
                {
                    var result = friendService.SendFriendRequest(addressee.PlayerId, CurrentPlayer.Instance.Id);

                    if (result.Equals(ServerMessages.Success.ToString()))
                    {
                        dialogService.ShowSuccess(Properties.Resources.SuccessNotification_FriendRequestSent);

                        RegisterCallback(addressee.PlayerId);
                    }
                    else if (result.Equals(ServerMessages.Pending.ToString()))
                    {
                        dialogService.ShowError(Properties.Resources.ErrorNotification_FriendRequestPending);
                    }
                    else if (result.Equals(ServerMessages.FriendshipAccepted.ToString()))
                    {
                        dialogService.ShowError(Properties.Resources.ErrorNotification_FriendshipAccepted);
                    }
                    else
                    {
                        dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorSendingFriendRequest);
                    }
                }
                catch (FaultException<FriendsService.ErrorMessage> faultException)
                {
                    logger.WarnFormat(faultException.Detail.Message);
                    dialogService.ShowServerError(faultException.Detail.Message);
                }
                catch (FaultException faultException)
                {
                    logger.Error(faultException);
                    dialogService.ShowServerError(faultException.Message);
                }
                catch (CommunicationException communicationException)
                {
                    logger.Error(communicationException);
                    dialogService.ShowServerError(Properties.Resources.ErrorNotification_ServerDisconnected);
                }
            }
            else
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_UsernameNotFound);
            }
        }

        private void RegisterCallback(int addresseeId)
        {
            try
            {
                var friendship = friendService.GetFriendRequest(addresseeId, CurrentPlayer.Instance.Id);

                if (friendship.FriendshipId > 0)
                {
                    friendService.RegisterFriendRequestsClient(
                        friendship.FriendshipId,
                        CurrentPlayer.Instance.Username);
                }
            }
            catch (FaultException<FriendsService.ErrorMessage> faultException)
            {
                logger.WarnFormat(faultException.Detail.Message);
                dialogService.ShowServerError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error(faultException);
                dialogService.ShowServerError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error(communicationException);
                dialogService.ShowServerError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }
        }

        private void Close()
        {
            dialogService.ShowFriends();
            RequestClose();
        }

        public void OnFriendshipStatusChanged()
        {
            ClearRequests();
            LoadRequests();
        }
    }
}
