using MasterMind_Client.Data;
using MasterMind_Client.Modals.ModalsUserControls;
using MasterMind_Client.TempData;
using MasterMind_Client.TempData.Enum;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MasterMind_Client.Modals
{
    /// <summary>
    /// Lógica de interacción para FriendRequestsModal.xaml
    /// </summary>
    public partial class FriendRequestsModal : Window
    {
        public FriendRequestsModal()
        {
            InitializeComponent();
            AddFriendRequests();
        }

        private void AddFriendRequests()
        {
            var requestersFriendRequests = FriendshipService.GetFriendRequests(CurrentPlayer.Instance.Id);
            foreach (var requester in requestersFriendRequests)
            {
                var requestControl = new FriendRequestUserControl
                {
                    DataContext = FriendshipService.GetFriendRequest(requester.player_id, CurrentPlayer.Instance.Id)
                };
                requestControl.UsernameTxt.Text = requester.username;

                requestControl.Accepted += RequestControl_Accepted;
                requestControl.Rejected += RequestControl_Rejected;

                FriendRequestsStack.Children.Add(requestControl);
            }
        }

        private void RequestControl_Accepted(object sender, Friendship request)
        {
            FriendshipService.AcceptFriendRequest(request.friendship_id);
            FriendRequestsStack.Children.Remove((UIElement)sender);
        }

        private void RequestControl_Rejected(object sender, Friendship request)
        {
            FriendshipService.RejectFriendRequest(request.friendship_id);
            FriendRequestsStack.Children.Remove((UIElement)sender);
        }

        private void AddreseeUsernameTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            var input = AddreseeUsernameTxt.Text;
            if (input.Equals(Properties.Resources.FriendRequestsModal_AddFriendText.Trim()))
            {
                AddreseeUsernameTxt.Foreground = Brushes.Black;
                AddreseeUsernameTxt.Text = "";
            }
        }

        private void AddreseeUsernameTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            var input = AddreseeUsernameTxt.Text;
            if (input.Equals(""))
            {
                AddreseeUsernameTxt.Foreground = Brushes.Gray;
                AddreseeUsernameTxt.Text = Properties.Resources.FriendRequestsModal_AddFriendText.Trim();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            new FriendsModal().Show();
            Close();
        }

        private void SendFriendRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            var username = AddreseeUsernameTxt.Text.Trim();
            int addreseeId = TempPlayerService.GetPlayerByUsername(username).player_id;
            var result = FriendshipService.SendFrienshipRequest(CurrentPlayer.Instance.Id, addreseeId);

            switch(result)
            {
                case RequestResult.Success:
                    new SuccessNotificationModal(Properties.Resources.SuccessNotification_FriendRequestSent).Show();
                    break;
                case RequestResult.RequestIsPendant:
                    new ErrorNotificationModal(Properties.Resources.ErrorNotification_FriendRequestPending).Show();
                    break;
                case RequestResult.Error:
                    new ErrorNotificationModal(Properties.Resources.ErrorNotification_ErrorSendingFriendRequest).Show();
                    break;
            }
        }
    }
}
