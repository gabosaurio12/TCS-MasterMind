using MasterMind_Client.Modals;
using MasterMind_Client.TempData;
using MasterMind_Client.TempData.DTO;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MasterMind_Client.Pages.RoomsPages
{
    /// <summary>
    /// Lógica de interacción para CreateRoomPage.xaml
    /// </summary>
    public partial class CreateRoomPage : Page
    {
        public CreateRoomPage()
        {
            InitializeComponent();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/RoomsPages/RoomsPage.xaml", UriKind.Relative));
        }

        private string GetDifficulty()
        {
            if (EasyBtn.IsChecked == true)
            {
                return "Easy";
            }
            else if (NormalBtn.IsChecked == true)
            {
                return "Normal";
            }
            else if (HardBtn.IsChecked == true)
            {
                return "Hard";
            }
            else
            {
                return "Enigma";
            }
        }

        private MatchRoomDto GetMatchRoomData()
        {
            string roomName = RoomNameTxt.Text.Trim();
            bool isTimeTrial = TimeTrialBtn.IsChecked == true;
            bool isPrivate = PrivateBtn.IsChecked == true;
            string difficulty = GetDifficulty();

            var matchRoom = new MatchRoomDto
            {
                RoomName = roomName,
                Gamemode = isTimeTrial ? "TimeTrial" : "Tries",
                Difficulty = difficulty,
                Privacy = isPrivate ? "Private" : "Public"
            };

            return matchRoom;
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            string roomName = RoomNameTxt.Text.Trim();
            if (string.IsNullOrWhiteSpace(roomName))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WhiteInput).Show();
                return;
            }

            if (Regex.IsMatch(roomName, @"\s"))
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_NoSpaces).Show();
                return;
            }

            var matchRoom = GetMatchRoomData();
            var result = TempMatchRoomsService.CreateMatchRoom(matchRoom);
            if (result == TempData.Enum.MatchRoomCreationResultEnum.Success)
            {
                new SuccessNotificationModal(Properties.Resources.SuccessNotification_MatchRoomCreatedSuccessfully).Show();
                NavigationService.Navigate(new Uri("Pages/RoomsPages/RoomsPage.xaml", UriKind.Relative));
            }
            else
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_ErrorCreatingMatchRoom).Show();
            }

        }
    }
}
