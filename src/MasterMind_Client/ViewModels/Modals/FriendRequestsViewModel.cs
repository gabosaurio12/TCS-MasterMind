using MasterMind_Client.ServerEndpoints;
using log4net;
using MasterMind_Client.Enums;
using MasterMind_Client.FriendsService;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Windows;

namespace MasterMind_Client.ViewModels.Modals
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class FriendRequestsViewModel : ModalViewModelBase, IFriendsServiceCallback
    {

        private readonly static ILog logger = LogManager.GetLogger(typeof(FriendRequestsViewModel));

        public ObservableCollection<FriendRequestViewModel> Requests { get; }
        public string AddresseeUsername { get; set; } = Properties.Resources.FriendRequestsModal_AddFriendText;
        public RelayCommand SendCommand { get; }
        public RelayCommand CloseCommand { get; }
        private readonly IDialogService dialogService;
        private readonly FriendsServiceClient friendService;
        private bool disposed;

        public FriendRequestsViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            InstanceContext instanceContext = new InstanceContext(this);
            friendService = ServiceClientFactory.CreateFriendsClient(instanceContext);

            Requests = new ObservableCollection<FriendRequestViewModel>();
            SendCommand = new RelayCommand(_ => SendFriendRequest());
            CloseCommand = new RelayCommand(_ => Close());

            LoadRequests();
            Subscribe();
        }

        private void LoadRequests()
        {
            ExecuteSafely(() =>
            {
                try
                {
                    var requests = friendService.GetFriendRequests(CurrentPlayer.Instance.Id);
                    foreach (var request in requests)
                    {
                        AddRequest(request);
                    }
                }
                catch (FaultException<FriendsService.ErrorMessage> faultException)
                {
                    logger.Error(faultException);
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

            });
        }

        private void ExecuteSafely(Action serviceCall)
        {
            try
            {
                serviceCall();
            }
            catch (FaultException<FriendsService.ErrorMessage> faultException)
            {
                logger.Error(faultException);
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
            var requestControl = new FriendRequestViewModel(requester, friendRequest);
            requestControl.Accepted += OnRequestAccepted;
            requestControl.Rejected += OnRequestRejected;
            Requests.Add(requestControl);
        }

        private void OnRequestAccepted(object sender, Friendship request)
        {
            ExecuteSafely(() =>
            {
                friendService.AcceptFriendRequest(request.FriendshipId);
                Requests.Remove((FriendRequestViewModel)sender);
            });
        }

        private void OnRequestRejected(object sender, Friendship request)
        {
            ExecuteSafely(() =>
            {
                friendService.RejectFriendRequest(request.FriendshipId);
                Requests.Remove((FriendRequestViewModel)sender);
            });
        }

        private void SendFriendRequest()
        {
            var playerService = ServiceClientFactory.CreatePlayerClient();
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
                    logger.Error(faultException);
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

        private void Subscribe()
        {
            ExecuteSafely(() => friendService.Subscribe(CurrentPlayer.Instance.Username));
        }

        private void Close()
        {
            dialogService.ShowFriends();
            RequestClose();
        }

        public void OnFriendshipStatusChanged()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (disposed)
                {
                    return;
                }

                ClearRequests();
                LoadRequests();
            }));
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            foreach (var request in Requests)
            {
                request.Accepted -= OnRequestAccepted;
                request.Rejected -= OnRequestRejected;
            }

            CloseChannel();
        }

        private void CloseChannel()
        {
            try
            {
                friendService.Unsubscribe(CurrentPlayer.Instance.Username);
                friendService.Close();
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException)
            {
                logger.Error(ex);
                friendService.Abort();
            }
        }
    }
}
