using MasterMind_Client.Data;
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

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Lógica de interacción para UpdateProfilePage.xaml
    /// </summary>
    public partial class UpdateProfilePage : Page
    {
        public UpdateProfilePage()
        {
            InitializeComponent();
            SetPlayerInfo();
        }

        private void SetPlayerInfo()
        {
            PlayerUsernameTxt.Text = CurrentPlayer.Instance.Username;
            PlayerEmailTxt.Text = CurrentPlayer.Instance.Email;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/ProfilePage.xaml", UriKind.Relative));
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var player = new Player
            {
                player_id = CurrentPlayer.Instance.Id,
                username = PlayerUsernameTxt.Text,
                email = PlayerEmailTxt.Text,
            };

            TempPlayerService.UpdatePlayer(player);
            NavigationService.Navigate(new Uri("Pages/ProfilePage.xaml", UriKind.Relative));
        }
    }
}
