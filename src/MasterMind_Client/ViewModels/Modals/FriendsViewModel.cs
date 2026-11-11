using log4net;
using MasterMind_Client.Enums;
using MasterMind_Client.FriendsService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Windows;

namespace MasterMind_Client.ViewModels.Modals
{
    public class FriendsViewModel : ModalViewModelBase, IFriendsServiceCallback
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(FriendsViewModel));

        public ObservableCollection<FriendViewModel> Friends { get; }

        public RelayCommand CloseCommand { get; }

        public RelayCommand FriendRequestsCommand { get; }

        private readonly IDialogService dialogService;

        private readonly FriendsServiceClient friendService;

        public FriendsViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            InstanceContext instanceContext = new InstanceContext(this);
            friendService = new FriendsServiceClient(instanceContext);

            Friends = new ObservableCollection<FriendViewModel>();
            CloseCommand = new RelayCommand(_ => RequestClose());
            FriendRequestsCommand = new RelayCommand(_ => OpenFriendRequests());
            LoadFriends();
            RegisterFriendships();
        }

        private void LoadFriends()
        {
            try
            {
                var friends = friendService.GetFriendships(CurrentPlayer.Instance.Id);
                foreach (var friend in friends)
                {
                    var friendVm = new FriendViewModel(friend.Username);
                    friendVm.Removed += OnFriendRemoved;
                    Friends.Add(friendVm);
                }
            }
            catch (FaultException<ErrorMessage> faultException)
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

        private void RegisterFriendships()
        {
            try
            {
                var friendRequests = friendService.GetFriendRequests(CurrentPlayer.Instance.Id);
                foreach (var requester in friendRequests)
                {
                    var request = friendService.GetFriendRequest(
                        CurrentPlayer.Instance.Id,
                        requester.PlayerId);

                    if (request.FriendshipId > 0)
                    {
                        friendService.RegisterFriendRequestsClient(
                            request.FriendshipId,
                            CurrentPlayer.Instance.Username);
                    }
                }

                var frienships = friendService.GetFriendships(CurrentPlayer.Instance.Id);
                foreach (var friend in frienships)
                {
                    var request = friendService.GetFriendship(
                        CurrentPlayer.Instance.Id,
                        friend.PlayerId);

                    if (request.FriendshipId > 0)
                    {
                        friendService.RegisterFriendsClient(
                        request.FriendshipId,
                        CurrentPlayer.Instance.Username);
                    }
                }
            }
            catch (FaultException<ErrorMessage> faultException)
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

        private void OpenFriendRequests()
        {
            dialogService.ShowFriendRequests();
            RequestClose();
        }

        public void OnFriendshipStatusChanged()
        {
            MessageBox.Show("Friendship status changed. Refreshing friends list.");
        }

        private void OnFriendRemoved(object sender, EventArgs e)
        {
            var friendVm = sender as FriendViewModel;
            if (friendVm == null)
            {
                return;
            }

            try
            {
                var result = friendService.RemoveFriendship(friendVm.Username, CurrentPlayer.Instance.Username);

                if (result.Equals(ServerMessages.Success.ToString()))
                {
                    Friends.Remove(friendVm);
                    dialogService.ShowSuccess(Properties.Resources.SuccessNotification_FriendRemoved);
                }
                else
                {
                    dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorRemovingFriend);
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
    }
}
