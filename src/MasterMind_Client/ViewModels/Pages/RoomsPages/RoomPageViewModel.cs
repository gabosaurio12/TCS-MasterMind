using MasterMind_Client.AvatarManagement;
using MasterMind_Client.ServerEndpoints;
using log4net;
using MasterMind_Client.Helpers;
using MasterMind_Client.LobbyService;
using MasterMind_Client.MatchService;
using MasterMind_Client.PlayerService;
using MasterMind_Client.RoomService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Threading;

namespace MasterMind_Client.ViewModels.Pages.RoomsPages
{
    public class RoomPageViewModel : ViewModelBase, ILobbyServiceCallback, IDisposable
    {
        private const string MessageSentResult = "MessageSent";
        private const string InappropriateVocabularyResult = "InappropriateVocabulary";
        private const string RoomsPageRoute = "Pages/RoomsPages/RoomsPage.xaml";
        private const string TriesPageRoute = "Pages/MatchPages/MatchTriesPage.xaml";
        private const string TimeTrialPageRoute = "Pages/MatchPages/MatchTimeTrialPage.xaml";
        private const string TimeTrialGamemode = "TimeTrial";


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

        public bool IsCountdownActive
        {
            get => isCountdownActive;
            private set => SetProperty(ref isCountdownActive, value);
        }

        public int CountdownTime
        {
            get => countdownTime;
            private set
            {
                SetProperty(ref countdownTime, value);
                CountdownText = string.Format(Properties.Resources.RoomPage_GameStarting, value);
            }
        }

        public string CountdownText
        {
            get => countdownText;
            private set => SetProperty(ref countdownText, value);
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

        public RelayCommand InviteCommand { get; }

        private string roomTitle;
        private string roomCode;
        private string messageText = string.Empty;
        private string chatText = string.Empty;
        private PlayerSlotViewModel playerOne;
        private PlayerSlotViewModel playerTwo;
        private bool isReady;
        private int currentRoomId;
        private string gamemode;

        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;

        private const int CountdownDuration = 5;

        private bool isCountdownActive;
        private int countdownTime;
        private string countdownText = string.Empty;
        private DispatcherTimer countdownTimer;

        private readonly RoomServiceClient roomService = ServiceClientFactory.CreateRoomClient();
        private readonly MatchServiceClient matchService =
            ServiceClientFactory.CreateMatchClient(new InstanceContext(new NoOpponentMatchCallback()));
        private readonly LobbyServiceClient lobbyService;

        public RoomPageViewModel(Uri uri, INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;

            InstanceContext instanceContext = new InstanceContext(this);
            lobbyService = ServiceClientFactory.CreateLobbyClient(instanceContext);

            ReadyCommand = new RelayCommand(_ => { SetReady(); MaybeStartCountdown(); });
            UnreadyCommand = new RelayCommand(_ => { SetUnready(); StopCountdown(); });
            EscapeCommand = new RelayCommand(_ => ExecuteEscapeCommand());
            InviteCommand = new RelayCommand(
                _ => dialogService.ShowFriends(currentRoomId), _ => !CurrentPlayer.Instance.IsGuest);
            SendCommand = new RelayCommand(_ => SendMessage(), _ => HasMessageText() && !IsCountdownActive);

            countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            countdownTimer.Tick += CountdownTimer_Tick;

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

                var result = lobbyService.SendMessage(chatMessage);
                HandleSendMessageResult(result);
            }
            catch (FaultException<LobbyService.ErrorMessage> faultException)
            {
                logger.WarnFormat("The server rejected the chat message: {0}", faultException.Detail.Message);
                dialogService.ShowServerError(faultException.Detail.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error("Communication failure while sending a chat message", communicationException);
                dialogService.ShowError(Properties.Resources.ErrorNotification_MessageNotSent);
            }
        }

        private void HandleSendMessageResult(ChatMessageResult result)
        {
            switch (result.Status)
            {
                case MessageSentResult:
                    AppendChatMessage(CurrentPlayer.Instance.Username, result.Message);
                    MessageText = string.Empty;
                    break;
                case InappropriateVocabularyResult:
                    AppendChatMessage(CurrentPlayer.Instance.Username, result.Message);
                    MessageText = string.Empty;
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
            int roomId = RouteQuery.GetInt(uri, "roomId");
            if (roomId == 0)
            {
                NavigateTo(RoomsPageRoute);
                return;
            }

            currentRoomId = roomId;

            try
            {
                var room = roomService.GetRoomById(roomId);
                if (room == null)
                {
                    dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorJoiningMatchRoom);
                    NavigateTo(RoomsPageRoute);
                    return;
                }

                RoomTitle = room.Name;
                RoomCode = room.RoomCode;
                gamemode = room.Gamemode;
                SetPlayers(room);
                NotifyPlayerJoined(room);
            }
            catch (FaultException<RoomService.ErrorMessage> faultException)
            {
                logger.WarnFormat("The server rejected the room lookup: {0}", faultException.Detail.Message);
                dialogService.ShowServerError(faultException.Detail.Message);
                NavigateTo(RoomsPageRoute);
            }
            catch (FaultException faultException)
            {
                logger.Error("Server fault during room lookup", faultException);
                dialogService.ShowServerError(faultException.Message);
                NavigateTo(RoomsPageRoute);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error("Communication failure during room lookup", communicationException);
                dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
                NavigateTo(RoomsPageRoute);
            }
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

        private void SetPlayers(Room room)
        {
            if (room.PlayerOneUsername == CurrentPlayer.Instance.Username)
            {
                PlayerOne = CreateCurrentPlayerSlot();
                PlayerTwo = CreateOpponentSlot(room.PlayerTwoUsername);
            }
            else
            {
                PlayerTwo = CreateCurrentPlayerSlot();
                PlayerOne = CreateOpponentSlot(room.PlayerOneUsername);
            }
        }

        private void SetPlayersEscaped(Room room, PlayerStatus status)
        {
            if (room.PlayerOneUsername == status.Username)
            {
                PlayerOne = CreateCurrentPlayerSlot();
                PlayerTwo = CreateEmptySlot();
            }
            else
            {
                PlayerTwo = CreateEmptySlot();
                PlayerOne = CreateCurrentPlayerSlot();
            }
        }

        private static PlayerSlotViewModel CreateCurrentPlayerSlot()
        {
            var currentPlayer = CurrentPlayer.Instance;

            return new PlayerSlotViewModel(currentPlayer.Username, AvatarLoader.GetCurrentPlayerAvatarAsync());
        }

        private static PlayerSlotViewModel CreateEmptySlot()
        {
            return new PlayerSlotViewModel(Properties.Resources.RoomPage_EmptySlot, true);
        }

        private PlayerSlotViewModel CreateOpponentSlot(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return CreateEmptySlot();
            }

            try
            {
                var playerService = ServiceClientFactory.CreatePlayerClient();
                var opponent = playerService.GetPlayerByUsername(username);

                return opponent == null
                    ? CreateEmptySlot()
                    : new PlayerSlotViewModel(opponent.Username, opponent.PlayerId, opponent.AvatarPath);
            }
            catch (FaultException<PlayerService.ErrorMessage> faultException)
            {
                logger.WarnFormat("The server rejected the player lookup: {0}", faultException.Detail.Message);
                dialogService.ShowServerError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error("Server fault during player lookup", faultException);
                dialogService.ShowServerError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error("Communication failure during player lookup", communicationException);
            }

            return CreateEmptySlot();
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

        private void SendEscapeToServer()
        {
            int roomId = currentRoomId;
            var playerStatus = new PlayerStatus
            {
                RoomId = roomId,
                Username = CurrentPlayer.Instance.Username,
                Status = "Escaped",
                IsPlayerOne = CurrentPlayer.Instance.Username.Equals(PlayerOne?.Username)
            };
            lobbyService.ChangePlayerStatusAsync(playerStatus);
        }


        private void ExecuteEscapeCommand()
        {
            StopCountdown();
            SendEscapeToServer();
            NavigateTo(RoomsPageRoute);
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
            {
                AppServices.SoundEffectService.Play(SoundEffect.MessageReceived);
                AppendChatMessage(message.SenderUsername, message.Message);
            });
        }

        public void OnChangePlayerStatusCallback(PlayerStatus status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var freshRoomService = ServiceClientFactory.CreateRoomClient();
                var room = freshRoomService.GetRoomById(status.RoomId);

                if (room != null)
                {
                    if (status.Status.Equals("Ready"))
                    {
                        SetOpponentReady();
                        MaybeStartCountdown();
                    }
                    else if (status.Status.Equals("NotReady"))
                    {
                        StopCountdown();
                        SetPlayers(room);
                        SetOpponentUnready();
                    }
                    else if (status.Status.Equals("Escaped"))
                    {
                        SetPlayersEscaped(room, status);
                        StopCountdown();
                    }
                    else
                    {
                        SetPlayers(room);
                    }
                }
            });
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (CountdownTime > 0)
            {
                CountdownTime--;
            }
            else
            {
                countdownTimer.Stop();
                IsCountdownActive = false;
                StartMatch();
            }
        }

        private void MaybeStartCountdown()
        {
            if (PlayerOne.IsReady && PlayerTwo.IsReady && !IsCountdownActive)
            {
                CountdownTime = CountdownDuration;
                IsCountdownActive = true;
                countdownTimer.Start();
            }
        }

        private void StopCountdown()
        {
            IsCountdownActive = false;
            countdownTimer.Stop();
            CountdownTime = CountdownDuration;
        }

        private void StartMatch()
        {
            bool isPlayerOne = CurrentPlayer.Instance.Username.Equals(PlayerOne?.Username);
            if (isPlayerOne && !PrepareSecretCode())
            {
                SetUnready();
                return;
            }

            NavigateTo($"{GetMatchPageRoute()}?roomId={currentRoomId}");
        }

        private string GetMatchPageRoute()
        {
            return gamemode == TimeTrialGamemode ? TimeTrialPageRoute : TriesPageRoute;
        }

        private bool PrepareSecretCode()
        {
            try
            {
                matchService.UpdateCode(CurrentPlayer.Instance.Username);
                return true;
            }
            catch (FaultException<MatchService.ErrorMessage> faultException)
            {
                logger.WarnFormat("The server rejected the secret code creation: {0}", faultException.Detail.Message);
                dialogService.ShowServerError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error("Server fault while creating the secret code", faultException);
                dialogService.ShowServerError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error("Communication failure while creating the secret code", communicationException);
                dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }

            return false;
        }

        public void Dispose()
        {
            countdownTimer.Stop();
            countdownTimer.Tick -= CountdownTimer_Tick;
            matchService.Abort();
        }

    }
}
