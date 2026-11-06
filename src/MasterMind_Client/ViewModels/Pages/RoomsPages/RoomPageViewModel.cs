using log4net;
using MasterMind_Client.LobbyService;
using MasterMind_Client.PlayerService;
using MasterMind_Client.RoomService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System;
using System.ServiceModel;
using System.Web;
using System.Windows;

namespace MasterMind_Client.ViewModels.Pages.RoomsPages
{
    public class RoomPageViewModel : ViewModelBase, ILobbyServiceCallback
    {
        private const string MessageSentResult = "MessageSent";
        private const string InappropriateVocabularyResult = "InappropriateVocabulary";

        private readonly static ILog logger = LogManager.GetLogger(typeof(RoomPageViewModel));

        public string RoomTitle
        {
            get => roomTitle;
            private set => SetProperty(ref roomTitle, value);
        }

        public string RoomCode
        {
            get => roomCode;
            private set => SetProperty(ref roomCode, value);
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

        public string MessageText
        {
            get => messageText;
            set => SetProperty(ref messageText, value);
        }

        public string ChatText
        {
            get => chatText;
            private set => SetProperty(ref chatText, value);
        }

        public RelayCommand ReadyCommand { get; }

        public RelayCommand UnreadyCommand { get; }

        public RelayCommand EscapeCommand { get; }

        public RelayCommand SendCommand { get; }

        private string roomTitle;
        private string roomCode;
        private string messageText = string.Empty;
        private string chatText = string.Empty;
        private PlayerSlotViewModel playerOne;
        private PlayerSlotViewModel playerTwo;
        private bool isReady;
        private int currentRoomId;

        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly InstanceContext instanceContext;

        private readonly RoomServiceClient roomService = new RoomServiceClient();
        private readonly LobbyServiceClient lobbyService;

        public RoomPageViewModel(Uri uri, INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;

            this.instanceContext = new InstanceContext(this);
            this.lobbyService = new LobbyServiceClient(this.instanceContext);

            ReadyCommand = new RelayCommand(_ => SetReady());
            UnreadyCommand = new RelayCommand(_ => SetUnready());
            EscapeCommand = new RelayCommand(_ => NavigateTo("Pages/RoomsPages/RoomsPage.xaml"));
            SendCommand = new RelayCommand(_ => SendMessage(), _ => HasMessageText());

            InitializeFromUri(uri);
        }

        private bool HasMessageText()
        {
            return !string.IsNullOrWhiteSpace(messageText);
        }

        private void SendMessage()
        {
            string message = MessageText.Trim();
            if (message.Length == 0)
            {
                return;
            }

            try
            {
                var chatMessage = new ChatMessage
                {
                    RoomId = currentRoomId,
                    SenderUsername = CurrentPlayer.Instance.Username,
                    Message = message
                };

                string result = lobbyService.SendMessage(chatMessage);
                HandleSendMessageResult(result, message);
            }
            catch (FaultException<LobbyService.ErrorMessage> faultException)
            {
                logger.WarnFormat("The server rejected the chat message: {0}", faultException.Detail.Message);
                dialogService.ShowError(Properties.Resources.ErrorNotification_MessageNotSent);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error("Communication failure while sending a chat message", communicationException);
                dialogService.ShowError(Properties.Resources.ErrorNotification_MessageNotSent);
            }
        }

        private void HandleSendMessageResult(string result, string message)
        {
            switch (result)
            {
                case MessageSentResult:
                    AppendChatMessage(CurrentPlayer.Instance.Username, message);
                    MessageText = string.Empty;
                    break;
                case InappropriateVocabularyResult:
                    AppendChatMessage(CurrentPlayer.Instance.Username, message);
                    dialogService.ShowError(Properties.Resources.ErrorNotification_InappropriateVocabulary);
                    break;
                default:
                    dialogService.ShowError(Properties.Resources.ErrorNotification_MessageNotSent);
                    break;
            }
        }

        private void AppendChatMessage(string senderUsername, string message)
        {
            ChatText = string.IsNullOrEmpty(chatText)
                ? $"{senderUsername}: {message}"
                : $"{chatText}{Environment.NewLine}{senderUsername}: {message}";
        }


        private void InitializeFromUri(Uri uri)
        {
            int roomId = GetRoomIdFromQuery(uri);
            if (roomId == 0)
            {
                NavigateTo("Pages/RoomsPages/RoomsPage.xaml");
                return;
            }

            currentRoomId = roomId;

            var room = roomService.GetRoomById(roomId);
            if (room == null)
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorJoiningMatchRoom);
                NavigateTo("Pages/RoomsPages/RoomsPage.xaml");
                return;
            }

            RoomTitle = room.Name;
            RoomCode = room.RoomCode;
            SetPlayers(room);
            NotifyPlayerJoined(room);
        }

        private void NotifyPlayerJoined(Room room)
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
            SendStatusToServer("Ready");
        }

        private void SetOpponentReady()
        {
            GetOpponentSlot().SetReady();
        }

        private void SetUnready()
        {
            IsReady = false;
            GetCurrentPlayerSlot().SetNotReady();
            SendStatusToServer("NotReady");
        }

        private void SetOpponentUnready()
        {
            GetOpponentSlot().SetNotReady();
        }

        private void SendStatusToServer(string status)
        {
            int roomId = currentRoomId;
            var playerStatus = new PlayerStatus
            {
                RoomId = roomId,
                Username = CurrentPlayer.Instance.Username,
                Status = status,
                IsPlayerOne = CurrentPlayer.Instance.Username.Equals(PlayerOne?.Username)
            };
            lobbyService.ChangePlayerStatusAsync(playerStatus);
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

        private PlayerSlotViewModel GetOpponentSlot()
        {
            var username = CurrentPlayer.Instance.Username;
            if (PlayerOne.Username == username)
            {
                return PlayerTwo;
            }

            return PlayerOne;
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }

        public void OnMessageReceivedCallback(ChatMessage message)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
                AppendChatMessage(message.SenderUsername, message.Message));
        }

        public void OnChangePlayerStatusCallback(PlayerStatus status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var freshRoomService = new RoomServiceClient();
                var room = freshRoomService.GetRoomById(status.RoomId);

                if (room != null)
                {
                    if (status.Status.Equals("Ready"))
                    {
                        SetOpponentReady();
                    }
                    else if (status.Status.Equals("NotReady"))
                    {
                        SetPlayers(room);
                        SetOpponentUnready();
                    }
                    else
                    {
                        SetPlayers(room);
                    }
                }
            });
        }
    }
}
