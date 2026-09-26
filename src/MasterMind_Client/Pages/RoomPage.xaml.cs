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

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Lógica de interacción para RoomPage.xaml
    /// </summary>
    public partial class RoomPage : Page
    {
        public RoomPage()
        {
            InitializeComponent();
        }

        private void ReadyBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = CurrentPlayer.Instance.Username;
            if (PlayerOneUserControl.UsernameTxt.Text.Equals(username))
            {
                PlayerOneUserControl.SetAsReady();
            }
            else
            {
                PlayerTwoUserControl.SetAsReady();
            }

            ReadyBtn.Visibility = Visibility.Hidden;
            ReadyBtn.IsEnabled = false;
            UnreadyBtn.IsEnabled = true;
            UnreadyBtn.Visibility = Visibility.Visible;
        }

        private void EscapeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UnreadyBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = CurrentPlayer.Instance.Username;
            if (PlayerOneUserControl.UsernameTxt.Text.Equals(username))
            {
                PlayerOneUserControl.SetAsNotReady();
            }
            else
            {
                PlayerTwoUserControl.SetAsNotReady();
            }

            UnreadyBtn.IsEnabled = false;
            UnreadyBtn.Visibility = Visibility.Hidden;
            ReadyBtn.Visibility = Visibility.Visible;
            ReadyBtn.IsEnabled = true;
        }
    }
}
