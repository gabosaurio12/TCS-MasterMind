using MasterMind_Client.Modals.ModalsUserControls;
using MasterMind_Client.TempData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                    DataContext = FriendshipService.GetFriendship(friend.Id, CurrentPlayer.Instance.Id)
                };
                requestControl.UsernameTxt.Text = friend.Username;
                if (friend.IsOnline)
                    requestControl.SetOnlineVisibility();
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
