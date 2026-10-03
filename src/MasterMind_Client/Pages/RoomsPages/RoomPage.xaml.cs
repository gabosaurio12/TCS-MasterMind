using MasterMind_Client.Pages.PagesUserControls;
using MasterMind_Client.TempData;
using MasterMind_Client.TempData.DTO;
using System;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MasterMind_Client.Pages.RoomsPages
{
    /// <summary>
    /// Lógica de interacción para RoomPage.xaml
    /// </summary>
    public partial class RoomPage : Page
    {
        public RoomPage()
        {
            InitializeComponent();
            this.Loaded += RoomPage_Loaded;
        }

        private void RoomPage_Loaded(object sender, RoutedEventArgs e)
        {
            var uri = NavigationService?.CurrentSource;
            if (uri == null)
            {
                return;
            }

            InitializeFromUri(uri);
        }

        private void InitializeFromUri(Uri uri)
        {
            int roomId = GetRoomIdFromQuery(uri);
            if (roomId == 0)
            {
                NavigationService.Navigate(new Uri("Pages/RoomsPages/RoomsPage.xaml", UriKind.Relative));
                return;
            }

            var room = TempMatchRoomsService.GetMatchRoomById(roomId);
            if (room == null)
            {
                new Modals.ErrorNotificationModal(Properties.Resources.ErrorNotification_ErrorJoiningMatchRoom).Show();
                NavigationService.Navigate(new Uri("Pages/RoomsPages/RoomsPage.xaml", UriKind.Relative));
                return;
            }

            RoomTitleLbl.Content = room.RoomName;
            SetPlayers(room);
        }

        private int GetRoomIdFromQuery(Uri uri)
        {
            string queryString = uri.IsAbsoluteUri
                ? uri.Query
                : GetQueryFromRelativeUri(uri);

            var query = HttpUtility.ParseQueryString(queryString);
            if (int.TryParse(query["roomId"], out int roomId))
            {
                return roomId;
            }

            return 0;
        }

        private string GetQueryFromRelativeUri(Uri uri)
        {
            string original = uri.OriginalString;
            int queryIndex = original.IndexOf('?');
            return queryIndex >= 0 ? original.Substring(queryIndex) : string.Empty;
        }

        private void SetPlayers(MatchRoomDto room)
        {
            if (room.PlayerOneId == CurrentPlayer.Instance.Id)
            {
                PlayerOneUserControl.UsernameTxt.Text = CurrentPlayer.Instance.Username;
                SetOpponentUsername(PlayerTwoUserControl, room.PlayerTwoId);
            }
            else
            {
                PlayerTwoUserControl.UsernameTxt.Text = CurrentPlayer.Instance.Username;
                SetOpponentUsername(PlayerOneUserControl, room.PlayerOneId);
            }
        }

        private void SetOpponentUsername(RoomPlayerUserControl playerControl, int opponentId)
        {
            if (opponentId == 0)
            {
                playerControl.UsernameTxt.Text = Properties.Resources.RoomPage_EmptySlot;
                return;
            }

            var opponent = TempPlayerService.GetPlayerById(opponentId);
            playerControl.UsernameTxt.Text = opponent?.username ?? Properties.Resources.RoomPage_EmptySlot;
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
            NavigationService.Navigate(new Uri("Pages/RoomsPages/RoomsPage.xaml", UriKind.Relative));
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
