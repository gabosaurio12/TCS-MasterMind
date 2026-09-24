using MasterMind_Client.Modals.ModalsUserControls;
using MasterMind_Client.TempData;
using System.Windows;

namespace MasterMind_Client.Modals
{
    /// <summary>
    /// Lógica de interacción para FriendsModal.xaml
    /// </summary>
    public partial class FriendsModal : Window
    {
        public FriendsModal()
        {
            InitializeComponent();
            AddFriends();
        }

        private void AddFriends()
        {
            var friends = FriendshipService.GetFrienships(CurrentPlayer.Instance.Id);
            foreach (var friend in friends)
            {
                var requestControl = new FriendUserControl
                {
                    DataContext = FriendshipService.GetFriendship(friend.player_id, CurrentPlayer.Instance.Id)
                };
                requestControl.UsernameTxt.Text = friend.username;
                FriendsStack.Children.Add(requestControl);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FriendRequestsBtn_Click(object sender, RoutedEventArgs e)
        {
            new FriendRequestsModal().Show();
            Close();
        }
    }
}
