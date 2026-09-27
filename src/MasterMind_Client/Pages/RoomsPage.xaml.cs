using MasterMind_Client.Pages.PagesUserControls;
using MasterMind_Client.TempData;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MasterMind_Client.Pages
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
            foreach (var i in rooms)
            {
                var roomControl = new MatchRoomUserControl
                {
                    DataContext = i
                };

                roomControl.RoomNameTxt.Text = i.RoomName;
                if (i.Privacy == "Private")
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
            NavigationService.Navigate(new Uri("Pages/CreateRoomPage.xaml", UriKind.Relative));
        }

        private void CodeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void JoinBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
