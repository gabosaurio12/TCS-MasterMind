using MasterMind_Client.Data;
using MasterMind_Client.Modals;
using MasterMind_Client.TempData;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

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

        private bool ValidateInput()
        {
            bool isValid = true;
            if (string.IsNullOrWhiteSpace(PlayerUsernameTxt.Text) || string.IsNullOrWhiteSpace(PlayerEmailTxt.Text))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WhiteInput).Show();
                isValid = false;
            }

            if (Regex.IsMatch(PlayerUsernameTxt.Text, @"\s"))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_NoSpaces).Show();
                isValid = false;
            }
            return isValid;
        }

        private void UpdatePlayer()
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

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                UpdatePlayer();
            }
        }
    }
}
