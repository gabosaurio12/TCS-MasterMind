using MasterMind_Client.Assets;
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

namespace MasterMind_Client.Modals.ModalsUserControls
{
    /// <summary>
    /// Lógica de interacción para FriendRequestUserControl.xaml
    /// </summary>
    public partial class FriendRequestUserControl : UserControl
    {
        public event EventHandler<TempFriendship> Accepted;
        public event EventHandler<TempFriendship> Rejected;

        public FriendRequestUserControl()
        {
            InitializeComponent();
        }

        public void SetOnlineVisibility()
        {
            OnlineVisibility.Fill = UtilsUI.OnlineGreen;
        }

        public void SetOfflineVisibility()
        {
            OnlineVisibility.Fill = UtilsUI.OfflineGray;
        }

        private void AcceptFriendRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            var request = (TempFriendship)DataContext;
            Accepted?.Invoke(this, request);
        }

        private void RejectFriendRequestBtn_Click(object sender, RoutedEventArgs e)
        {
            var request = (TempFriendship)DataContext;
            Rejected?.Invoke(this, request);
        }
    }
}
