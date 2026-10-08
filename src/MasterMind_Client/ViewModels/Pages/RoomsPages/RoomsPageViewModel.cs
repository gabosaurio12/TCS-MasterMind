using MasterMind_Client.TempData;
using MasterMind_Client.TempData.Enum;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System;
using System.Collections.ObjectModel;

namespace MasterMind_Client.ViewModels.Pages.RoomsPages
{
    public class RoomsPageViewModel : ViewModelBase
    {
        public RoomsPageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            Rooms = new ObservableCollection<MatchRoomViewModel>();
            JoinCommand = new RelayCommand(_ => Join());
            CodeCommand = new RelayCommand(_ => { });
            CreateCommand = new RelayCommand(_ => NavigateTo("Pages/RoomsPages/CreateRoomPage.xaml"));
            BackCommand = new RelayCommand(_ => NavigateTo("Pages/MainPage.xaml"));
            LoadRooms();
        }

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

        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        private void LoadRooms()
        {
            var rooms = TempMatchRoomsService.GetMatchRooms();
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
            var result = TempMatchRoomsService.JoinMatchRoom(room.MatchRoomId, CurrentPlayer.Instance.Id);
            if (result == MatchRoomJoinResultEnum.Success || result == MatchRoomJoinResultEnum.AlreadyJoined)
            {
                var uri = new Uri($"Pages/RoomsPages/RoomPage.xaml?roomId={room.MatchRoomId}", UriKind.Relative);
                _navigationService.Navigate(uri);
            }
            else
            {
                _dialogService.ShowError(GetJoinErrorMessage(result));
            }
        }

        private string GetJoinErrorMessage(MatchRoomJoinResultEnum result)
        {
            switch (result)
            {
                case MatchRoomJoinResultEnum.RoomFull:
                    return Properties.Resources.ErrorNotification_RoomFull;
                case MatchRoomJoinResultEnum.OwnRoom:
                    return Properties.Resources.ErrorNotification_OwnRoom;
                default:
                    return Properties.Resources.ErrorNotification_ErrorJoiningMatchRoom;
            }
        }

        private void NavigateTo(string uri)
        {
            _navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }
    }
}
