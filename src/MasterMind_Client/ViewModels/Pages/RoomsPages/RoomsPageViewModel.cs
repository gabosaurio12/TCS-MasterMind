using MasterMind_Client.RoomService;
using MasterMind_Client.TempData.Enum;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;

namespace MasterMind_Client.ViewModels.Pages.RoomsPages
{
    public class RoomsPageViewModel : ViewModelBase
    {

        public ObservableCollection<MatchRoomViewModel> Rooms { get; }
        public MatchRoomViewModel SelectedRoom
        {
            get => _selectedRoom;
            set => SetProperty(ref _selectedRoom, value);
        }
        public RelayCommand JoinCommand { get; }
        public RelayCommand CodeCommand { get; }
        public RelayCommand CreateCommand { get; }
        public RelayCommand BackCommand { get; }
        private MatchRoomViewModel _selectedRoom;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly RoomServiceClient roomService = new RoomServiceClient();

        public RoomsPageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            Rooms = new ObservableCollection<MatchRoomViewModel>();
            JoinCommand = new RelayCommand(_ => Join());
            CodeCommand = new RelayCommand(_ => { });
            CreateCommand = new RelayCommand(_ => NavigateTo("Pages/RoomsPages/CreateRoomPage.xaml"));
            BackCommand = new RelayCommand(_ => NavigateTo("Pages/MainPage.xaml"));
            LoadRooms();
        }

        private void LoadRooms()
        {
            var rooms = roomService.GetPublicRooms();
            foreach (var room in rooms)
            {
                Rooms.Add(new MatchRoomViewModel(room));
            }
        }

        private void Join()
        {
            if (SelectedRoom == null)
            {
                return;
            }

            var room = SelectedRoom.Room;
            try
            {
                var result = roomService.JoinRoom(room, CurrentPlayer.Instance.Username);
                if (result.Equals(MatchRoomJoinResultEnum.Success.ToString()))
                {
                    var uri = new Uri($"Pages/RoomsPages/RoomPage.xaml?roomId={room.RoomId}", UriKind.Relative);
                    navigationService.Navigate(uri);
                }
                else
                {
                    dialogService.ShowError(GetJoinErrorMessage(result));
                }
            }
            catch (FaultException)
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorJoiningMatchRoom);
            }
            
        }

        private string GetJoinErrorMessage(string result)
        {
            string errorMessage;
            if (result.Equals(MatchRoomJoinResultEnum.RoomIsFull.ToString()))
            {
                errorMessage = Properties.Resources.ErrorNotification_RoomFull;
            }
            else
            {
                errorMessage = Properties.Resources.ErrorNotification_ErrorJoiningMatchRoom;
            }

            return errorMessage;
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }
    }
}
