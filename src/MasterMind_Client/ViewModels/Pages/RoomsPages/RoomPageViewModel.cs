using MasterMind_Client.LobbyService;
using MasterMind_Client.PlayerService;
using MasterMind_Client.RoomService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System;
using System.ServiceModel;
using System.Web;

namespace MasterMind_Client.ViewModels.Pages.RoomsPages
{
    public class RoomPageViewModel : ViewModelBase, ILobbyServiceCallback
    {

        public string RoomTitle
        {
            get => roomTitle;
            private set => SetProperty(ref roomTitle, value);
        }

        public PlayerSlotViewModel PlayerOne
        {
            get => playerOne;
            private set => SetProperty(ref playerOne, value);
        }

        public PlayerSlotViewModel PlayerTwo
        {
            get => playerTwo;
            private set => SetProperty(ref playerTwo, value);
        }

        public bool IsReady
        {
            get => isReady;
            private set => SetProperty(ref isReady, value);
        }

        public RelayCommand ReadyCommand { get; }

        public RelayCommand UnreadyCommand { get; }

        public RelayCommand EscapeCommand { get; }

        private string roomTitle;
        private PlayerSlotViewModel playerOne;
        private PlayerSlotViewModel playerTwo;
        private bool isReady;

        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly InstanceContext instanceContext;

        private readonly RoomServiceClient roomService = new RoomServiceClient();
        private readonly LobbyServiceClient lobbyService;

        public RoomPageViewModel(Uri uri, INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;

            var callbackHandler = this;
            var instanceContext = new InstanceContext(callbackHandler);
            lobbyService = new LobbyServiceClient(instanceContext);
            this.instanceContext = instanceContext;

            ReadyCommand = new RelayCommand(_ => SetReady());
            UnreadyCommand = new RelayCommand(_ => SetUnready());
            EscapeCommand = new RelayCommand(_ => NavigateTo("Pages/RoomsPages/RoomsPage.xaml"));
            InitializeFromUri(uri);

            lobbyService = new LobbyServiceClient(this.instanceContext);
        }


        private void InitializeFromUri(Uri uri)
        {
            int roomId = GetRoomIdFromQuery(uri);
            if (roomId == 0)
            {
                NavigateTo("Pages/RoomsPages/RoomsPage.xaml");
                return;
            }

            var room = roomService.GetRoomById(roomId);
            if (room == null)
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorJoiningMatchRoom);
                NavigateTo("Pages/RoomsPages/RoomsPage.xaml");
                return;
            }

            RoomTitle = room.Name;
            SetPlayers(room);
            NotifyPlayerStatus(room);
        }

        private void NotifyPlayerStatus(Room room)
        {
            string username = CurrentPlayer.Instance.Username;
            var playerStatus = new PlayerStatus
            {
                RoomId = room.RoomId,
                Username = username,
                Status = "NotReady",
                IsPlayerOne = username.Equals(PlayerOne.Username)
            };

            lobbyService.ChangePlayerStatusAsync(playerStatus);
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

        private void SetPlayers(Room room)
        {
            if (room.PlayerOneUsername == CurrentPlayer.Instance.Username)
            {
                PlayerOne = new PlayerSlotViewModel(CurrentPlayer.Instance.Username);
                PlayerTwo = new PlayerSlotViewModel(GetOpponentUsername(room.PlayerTwoUsername));
            }
            else
            {
                PlayerTwo = new PlayerSlotViewModel(CurrentPlayer.Instance.Username);
                PlayerOne = new PlayerSlotViewModel(GetOpponentUsername(room.PlayerOneUsername));
            }
        }

        private string GetOpponentUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Properties.Resources.RoomPage_EmptySlot;
            }

            var playerService = new PlayerServiceClient();
            var opponent = playerService.GetPlayerByUsername(username);
            return opponent?.Username ?? Properties.Resources.RoomPage_EmptySlot;
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

        public void OnMessageReceivedCallback(ChatMessage message)
        {
            throw new NotImplementedException();
        }

        public void OnChangePlayerStatusCallback(PlayerStatus status)
        {
            var room = roomService.GetRoomById(status.RoomId);
            SetPlayers(room);
        }
    }
}
