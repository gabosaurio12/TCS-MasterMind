using MasterMind_Client.ServerEndpoints;
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
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class FriendsViewModel : ModalViewModelBase, IFriendsServiceCallback
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(FriendsViewModel));

        public ObservableCollection<FriendViewModel> Friends { get; }

        public RelayCommand CloseCommand { get; }

        public RelayCommand FriendRequestsCommand { get; }

        private readonly IDialogService dialogService;

        private readonly FriendsServiceClient friendService;

        private bool disposed;

        public FriendsViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            InstanceContext instanceContext = new InstanceContext(this);
            friendService = ServiceClientFactory.CreateFriendsClient(instanceContext);

            Friends = new ObservableCollection<FriendViewModel>();
            CloseCommand = new RelayCommand(_ => RequestClose());
            FriendRequestsCommand = new RelayCommand(_ => OpenFriendRequests());
            LoadFriends();
            Subscribe();
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

        private void Subscribe()
        {
            try
            {
                friendService.Subscribe(CurrentPlayer.Instance.Username);
            }
            catch (FaultException<ErrorMessage> faultException)
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

        private void OpenFriendRequests()
        {
            dialogService.ShowFriendRequests();
            RequestClose();
        }

        private void ClearFriends()
        {
            foreach (var friendVm in Friends)
            {
                friendVm.Removed -= OnFriendRemoved;
            }

            Friends.Clear();
        }

        public void OnFriendshipStatusChanged()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (disposed)
                {
                    return;
                }

                ClearFriends();
                LoadFriends();
            }));
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            foreach (var friendVm in Friends)
            {
                friendVm.Removed -= OnFriendRemoved;
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
    }
}