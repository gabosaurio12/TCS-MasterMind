using System.Windows;
using System.Windows.Controls;

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
        }

        private void AddreseeUsernameTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            var input = AddreseeUsernameTxt.Text;
            if (input.Equals(Properties.Resources.FriendRequestsModal_AddFriendText.Trim()))
            {
                AddreseeUsernameTxt.Foreground = System.Windows.Media.Brushes.Black;
                AddreseeUsernameTxt.Text = "";
            }
        }

        private void AddreseeUsernameTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            var input = AddreseeUsernameTxt.Text;
            if (input.Equals(""))
            {
                AddreseeUsernameTxt.Foreground = System.Windows.Media.Brushes.Gray;
                AddreseeUsernameTxt.Text = Properties.Resources.FriendRequestsModal_AddFriendText.Trim();
            }
        }
    }
}
