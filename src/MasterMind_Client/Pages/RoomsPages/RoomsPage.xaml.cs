using MasterMind_Client.Modals;
using MasterMind_Client.Pages.PagesUserControls;
using MasterMind_Client.TempData;
using MasterMind_Client.TempData.DTO;
using MasterMind_Client.TempData.Enum;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MasterMind_Client.Pages.RoomsPages
{
    /// <summary>
    /// Lógica de interacción para RoomsPage.xaml
    /// </summary>
    public partial class RoomsPage : Page
    {
        private MatchRoomUserControl selectedRoomControl;

        public RoomsPage()
        {
            InitializeComponent();
            SetRooms();
        }

        private void SetRooms()
        {
            var rooms = TempMatchRoomsService.GetMatchRooms();
            foreach (var room in rooms)
            {
                var roomControl = new MatchRoomUserControl
                {
                    DataContext = room
                };

                roomControl.RoomNameTxt.Text = room.RoomName;
                if (room.Privacy == "Private")
                {
                    roomControl.SetAsPrivate();
                }

                roomControl.Margin = new Thickness(20);

                roomControl.RoomSelected += RoomControl_RoomSelected;

                RoomsStack.Children.Add(roomControl);
            }
        }

        private void RoomControl_RoomSelected(object sender, EventArgs e)
        {
            selectedRoomControl?.Deselect();

            selectedRoomControl = (MatchRoomUserControl)sender;

            selectedRoomControl.Select();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/RoomsPages/CreateRoomPage.xaml", UriKind.Relative));
        }

        private void CodeBtn_Click(object sender, RoutedEventArgs e)
        {
            var roomName = selectedRoomControl.RoomNameTxt.Text;
        }

        private void JoinBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoomControl != null)
            {
                var room = (MatchRoomDto)selectedRoomControl.DataContext;
                var result = TempMatchRoomsService.JoinMatchRoom(room.MatchRoomId, CurrentPlayer.Instance.Id);
                if (result == MatchRoomJoinResultEnum.Success || result == MatchRoomJoinResultEnum.AlreadyJoined)
                {
                    var uri = new Uri($"Pages/RoomsPages/RoomPage.xaml?roomId={room.MatchRoomId}", UriKind.Relative);
                    NavigationService.Navigate(uri);
                }
                else
                {
                    new ErrorNotificationModal(GetJoinErrorMessage(result)).Show();
                }
            }           
        }

        private string GetJoinErrorMessage(MatchRoomJoinResultEnum result)
        {
            string errorMessage = string.Empty;
            switch (result)
            {
                case MatchRoomJoinResultEnum.RoomFull:
                    errorMessage = Properties.Resources.ErrorNotification_RoomFull;
                    break;
                case MatchRoomJoinResultEnum.OwnRoom:
                    errorMessage = Properties.Resources.ErrorNotification_OwnRoom;
                    break;
                default:
                    errorMessage = Properties.Resources.ErrorNotification_ErrorJoiningMatchRoom;
                    break;
            }

            return errorMessage;
        }
    }
}
