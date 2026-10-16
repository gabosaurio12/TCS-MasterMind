using System.Windows;
using System.Windows.Media;

namespace MasterMind_Client.Modals
{
    /// <summary>
    /// Interaction logic for FriendRequestsModal.xaml.
    /// </summary>
    public partial class FriendRequestsModal : Window
    {
        public FriendRequestsModal()
        {
            InitializeComponent();
        }

        private void AddreseeUsernameTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            var input = AddreseeUsernameTxt.Text;
            if (input.Equals(Properties.Resources.FriendRequestsModal_AddFriendText.Trim()))
            {
                AddreseeUsernameTxt.Foreground = (Brush)Application.Current.Resources["InkBrush"];
                AddreseeUsernameTxt.Text = "";
            }
        }

        private void AddreseeUsernameTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            var input = AddreseeUsernameTxt.Text;
            if (input.Equals(""))
            {
                AddreseeUsernameTxt.Foreground = (Brush)Application.Current.Resources["InkMutedBrush"];
                AddreseeUsernameTxt.Text = Properties.Resources.FriendRequestsModal_AddFriendText.Trim();
            }
        }
    }
}
