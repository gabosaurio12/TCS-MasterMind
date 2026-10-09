using MasterMind_Client.TempData;
using MasterMind_Client.TempData.DTO;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System;
using System.Web;

namespace MasterMind_Client.ViewModels.Pages.RoomsPages
{
    public class RoomPageViewModel : ViewModelBase
    {
        public RoomPageViewModel(Uri uri, INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            ReadyCommand = new RelayCommand(_ => SetReady());
            UnreadyCommand = new RelayCommand(_ => SetUnready());
            EscapeCommand = new RelayCommand(_ => NavigateTo("Pages/RoomsPages/RoomsPage.xaml"));
            InitializeFromUri(uri);
        }

        public string RoomTitle
        {
            get => _roomTitle;
            private set => SetProperty(ref _roomTitle, value);
        }

        public PlayerSlotViewModel PlayerOne
        {
            get => _playerOne;
            private set => SetProperty(ref _playerOne, value);
        }

        public PlayerSlotViewModel PlayerTwo
        {
            get => _playerTwo;
            private set => SetProperty(ref _playerTwo, value);
        }

        public bool IsReady
        {
            get => _isReady;
            private set => SetProperty(ref _isReady, value);
        }

        public RelayCommand ReadyCommand { get; }

        public RelayCommand UnreadyCommand { get; }

        public RelayCommand EscapeCommand { get; }

        private string _roomTitle;
        private PlayerSlotViewModel _playerOne;
        private PlayerSlotViewModel _playerTwo;
        private bool _isReady;

        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;

        private void InitializeFromUri(Uri uri)
        {
            int roomId = GetRoomIdFromQuery(uri);
            if (roomId == 0)
            {
                NavigateTo("Pages/RoomsPages/RoomsPage.xaml");
                return;
            }

            var room = TempMatchRoomsService.GetMatchRoomById(roomId);
            if (room == null)
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorJoiningMatchRoom);
                NavigateTo("Pages/RoomsPages/RoomsPage.xaml");
                return;
            }

            RoomTitle = room.RoomName;
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

            string queryString = string.Empty;

            if (queryIndex >= 0)
            {
                queryString = original.Substring(queryIndex);
            }

            return queryString;
        }

        private void SetPlayers(MatchRoomDto room)
        {
            if (room.PlayerOneId == CurrentPlayer.Instance.Id)
            {
                PlayerOne = new PlayerSlotViewModel(CurrentPlayer.Instance.Username);
                PlayerTwo = new PlayerSlotViewModel(GetOpponentUsername(room.PlayerTwoId));
            }
            else
            {
                PlayerTwo = new PlayerSlotViewModel(CurrentPlayer.Instance.Username);
                PlayerOne = new PlayerSlotViewModel(GetOpponentUsername(room.PlayerOneId));
            }
        }

        private string GetOpponentUsername(int opponentId)
        {
            if (opponentId == 0)
            {
                return Properties.Resources.RoomPage_EmptySlot;
            }

            var opponent = TempPlayerService.GetPlayerById(opponentId);
            return opponent?.username ?? Properties.Resources.RoomPage_EmptySlot;
        }

        private void SetReady()
        {
            IsReady = true;
            GetCurrentPlayerSlot().SetReady();
        }

        private void SetUnready()
        {
            IsReady = false;
            GetCurrentPlayerSlot().SetNotReady();
        }

        private PlayerSlotViewModel GetCurrentPlayerSlot()
        {
            var username = CurrentPlayer.Instance.Username;
            if (PlayerOne.Username == username)
            {
                return PlayerOne;
            }

            return PlayerTwo;
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }
    }
}
