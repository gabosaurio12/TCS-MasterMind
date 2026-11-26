using log4net;
using MasterMind_Client.Helpers;
using MasterMind_Client.MatchService;
using MasterMind_Client.ServerEndpoints;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers;
using MasterMind_Client.ViewModels.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchTimeTrial
{
    public class MatchTimeTrialViewModel : ViewModelBase, IDisposable
    {
        private const string RoomsPageRoute = "Pages/RoomsPages/RoomsPage.xaml";
        private const string RoundNotStartedResult = "RoundNotStarted";

        private static readonly ILog logger = LogManager.GetLogger(typeof(MatchTimeTrialViewModel));

        private readonly int roomId;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly MatchCallbackHandler callbackHandler = new MatchCallbackHandler();
        private readonly MatchReconnector reconnector;
        private readonly string username = CurrentPlayer.Instance.Username;

        private MatchServiceClient matchService;
        private int currentRowIndex;
        private bool isSending;
        private bool isFinished = true;
        private bool isRoundClosed;
        private bool isSubscribed;
        private bool disposed;
        private bool isWaitingForOpponent;
        private bool isReconnecting;
        private bool isMenuOpen;
        private RoundResult pendingResult;
        private IReadOnlyList<PinColour> lastCode;
        private PaletteColourViewModel selectedColour;

        public MatchClockViewModel Clock { get; } = new MatchClockViewModel();

        public MatchScoreViewModel Score { get; } = new MatchScoreViewModel();

        public ObservableCollection<MatchRowViewModel> TriesRows { get; } =
            new ObservableCollection<MatchRowViewModel>();

        public ObservableCollection<PaletteColourViewModel> PaletteColours { get; } =
            new ObservableCollection<PaletteColourViewModel>();

        public bool IsWaitingForOpponent
        {
            get => isWaitingForOpponent;
            private set => SetProperty(ref isWaitingForOpponent, value);
        }

        public bool IsReconnecting
        {
            get => isReconnecting;
            private set => SetProperty(ref isReconnecting, value);
        }

        public bool IsMenuOpen
        {
            get => isMenuOpen;
            private set => SetProperty(ref isMenuOpen, value);
        }

        public ICommand SelectColourCommand { get; }

        public ICommand TryCommand { get; }

        public ICommand OpenMenuCommand { get; }

        public MatchTimeTrialViewModel(Uri uri, INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            roomId = RouteQuery.GetInt(uri, "roomId");
            reconnector = new MatchReconnector(callbackHandler);
            matchService = ServiceClientFactory.CreateMatchClient(new InstanceContext(callbackHandler));
            AttachEvents();
            CreateBoard();
            CreatePalette();
            SelectColourCommand = new RelayCommand(parameter => SelectColour(parameter as PaletteColourViewModel));
            TryCommand = new RelayCommand(async _ => await SubmitTryAsync(), _ => CanSubmitTry());
            OpenMenuCommand = new RelayCommand(_ => OpenMenu());
        }

        public async Task InitializeAsync()
        {
            if (roomId == 0)
            {
                NavigateTo(RoomsPageRoute);
                return;
            }

            if (await SubscribeAsync())
            {
                await ReadyForRoundAsync();
            }
        }

        public void Dispose()
        {
            disposed = true;
            DetachEvents();
            Clock.Dispose();
            Unsubscribe();
            CloseClient(matchService);
        }

        private MatchRowViewModel CurrentRow => TriesRows[currentRowIndex];

        private bool IsPlaying => Clock.IsRunning && !isFinished;

        private void AttachEvents()
        {
            callbackHandler.RoundResolved += OnRoundResolved;
            callbackHandler.OpponentAbandoned += OnOpponentAbandoned;
            callbackHandler.TimeTrialStarted += OnTimeTrialStarted;
            Clock.Elapsed += OnClockElapsed;
            AttachChannelEvents();
        }

        private void DetachEvents()
        {
            callbackHandler.RoundResolved -= OnRoundResolved;
            callbackHandler.OpponentAbandoned -= OnOpponentAbandoned;
            callbackHandler.TimeTrialStarted -= OnTimeTrialStarted;
            Clock.Elapsed -= OnClockElapsed;
            DetachChannelEvents();
        }

        private void AttachChannelEvents()
        {
            matchService.InnerChannel.Faulted += OnChannelLost;
            matchService.InnerChannel.Closed += OnChannelLost;
        }

        private void DetachChannelEvents()
        {
            matchService.InnerChannel.Faulted -= OnChannelLost;
            matchService.InnerChannel.Closed -= OnChannelLost;
        }

        private void CreateBoard()
        {
            for (int index = 0; index < MatchRules.BoardRowCount; index++)
            {
                TriesRows.Add(new MatchRowViewModel(PlacePin));
            }
        }

        private void CreatePalette()
        {
            foreach (PinColour colour in Enum.GetValues(typeof(PinColour)))
            {
                PaletteColours.Add(new PaletteColourViewModel(colour));
            }

            SelectColour(PaletteColours.First());
        }

        private void OpenMenu()
        {
            if (disposed || IsMenuOpen)
            {
                return;
            }

            IsMenuOpen = dialogService.ShowMatchMenu(ExitMatchAsync, CloseMenu);
        }

        private void CloseMenu()
        {
            IsMenuOpen = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private async Task ExitMatchAsync()
        {
            CloseRound();
            Unsubscribe();
            await new RoomEscapeNotifier(dialogService).NotifyAsync(roomId);
            NavigateTo(RoomsPageRoute);
        }

        private void SelectColour(PaletteColourViewModel colour)
        {
            if (colour == null || IsMenuOpen)
            {
                return;
            }

            foreach (var entry in PaletteColours)
            {
                entry.IsSelected = entry == colour;
            }

            selectedColour = colour;
        }

        private void PlacePin(MatchPinSlotViewModel slot)
        {
            if (!IsPlaying || IsMenuOpen || !CurrentRow.PinSlots.Contains(slot))
            {
                return;
            }

            AppServices.SoundEffectService.Play(SoundEffect.PinSelect);
            slot.Colour = selectedColour.Colour;
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanSubmitTry()
        {
            return IsPlaying && !isSending && !IsMenuOpen && !IsReconnecting && CurrentRow.IsComplete;
        }

        private async Task<bool> SubscribeAsync()
        {
            isSubscribed = await TryCallAsync(() => matchService.SubscribeAsync(username), LeaveAfterError);
            return isSubscribed;
        }

        private async Task ReadyForRoundAsync()
        {
            IsWaitingForOpponent = true;
            await TryCallAsync(() => matchService.ReadyForRoundAsync(username), LeaveAfterError);
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            isSubscribed = false;
            try
            {
                matchService.Unsubscribe(username);
            }
            catch (Exception exception) when (exception is CommunicationException
                || exception is TimeoutException)
            {
                logger.Warn(exception);
            }
        }

        private async Task<bool> TryCallAsync(Func<Task> call, Action<string> onFailure)
        {
            try
            {
                await call();
                return true;
            }
            catch (FaultException<MatchService.ErrorMessage> faultException)
            {
                logger.WarnFormat(faultException.Detail.Message);
                onFailure(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error(faultException);
                onFailure(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error(communicationException);
                onFailure(string.Empty);
            }

            return false;
        }

        private async Task SubmitTryAsync()
        {
            isSending = true;
            try
            {
                await RequestTryAsync(CurrentRow.GetCode());
            }
            finally
            {
                isSending = false;
                ApplyPendingResult();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private async Task RequestTryAsync(IReadOnlyList<PinColour> code)
        {
            Clues clues = null;
            bool succeeded = await TryCallAsync(
                async () => clues = await matchService.ValidateTryAsync(username, MatchColourMapper.ToCodeTry(code)),
                HandleTryFailure);
            if (succeeded && !isRoundClosed)
            {
                HandleClues(clues, code);
            }
        }

        private void HandleTryFailure(string errorDetail)
        {
            if (errorDetail == RoundNotStartedResult || string.IsNullOrEmpty(errorDetail) || isRoundClosed)
            {
                logger.InfoFormat("The try was not applied ({0}).", errorDetail);
                return;
            }

            dialogService.ShowServerError(errorDetail);
        }

        private void HandleClues(Clues clues, IReadOnlyList<PinColour> code)
        {
            CurrentRow.ApplyClues(clues.CluesResult);
            var effect = clues.IsCorrect ? SoundEffect.CodeTryCorrect : SoundEffect.CodeTryResult;
            AppServices.SoundEffectService.Play(effect);
            if (clues.IsCorrect)
            {
                lastCode = code;
                FinishLocally();
                return;
            }

            AdvanceRow();
        }

        private void AdvanceRow()
        {
            if (currentRowIndex < TriesRows.Count - 1)
            {
                currentRowIndex++;
                return;
            }

            TriesRows[0].CopyFrom(CurrentRow);
            foreach (var row in TriesRows.Skip(1))
            {
                row.Clear();
            }

            currentRowIndex = 1;
        }

        private void FinishLocally()
        {
            isFinished = true;
            Clock.Stop();
            IsWaitingForOpponent = !isRoundClosed;
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnTimeTrialStarted(object sender, int remainingMs)
        {
            if (disposed || isRoundClosed)
            {
                return;
            }

            isFinished = false;
            IsWaitingForOpponent = false;
            Clock.Start(remainingMs);
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnClockElapsed(object sender, EventArgs e)
        {
            if (disposed || isRoundClosed)
            {
                return;
            }

            AppServices.SoundEffectService.Play(SoundEffect.TimeIsOver);
            FinishLocally();
        }

        private void OnRoundResolved(object sender, RoundResult result)
        {
            if (disposed || isRoundClosed)
            {
                return;
            }

            pendingResult = result;
            ApplyPendingResult();
        }

        private void ApplyPendingResult()
        {
            if (pendingResult != null && !isSending && !isRoundClosed && !disposed)
            {
                ShowResult(pendingResult);
            }
        }

        private void OnOpponentAbandoned(object sender, string abandonedUsername)
        {
            if (disposed || isRoundClosed)
            {
                return;
            }

            CloseRound();
            dialogService.CloseMatchMenu();
            dialogService.ShowSuccess(Properties.Resources.MatchTries_OpponentAbandoned, ReturnToRoom);
        }

        private void CloseRound()
        {
            isRoundClosed = true;
            isFinished = true;
            Clock.Stop();
            IsWaitingForOpponent = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private void ShowResult(RoundResult result)
        {
            CloseRound();
            Score.Update(result.Scores, username);

            Action continued = result.IsMatchOver ? (Action)ReturnToRoom : async () => await StartNextRound();
            if (string.Equals(result.WinnerUsername, username, StringComparison.OrdinalIgnoreCase))
            {
                dialogService.ShowYouWon(lastCode, continued);
                return;
            }

            dialogService.ShowGameOver(BuildLossTitle(result), continued);
        }

        private static string BuildLossTitle(RoundResult result)
        {
            if (string.IsNullOrEmpty(result.WinnerUsername))
            {
                return Properties.Resources.MatchTimeTrial_TimeIsUp;
            }

            return result.IsMatchOver
                ? Properties.Resources.MatchTries_MatchLost
                : Properties.Resources.MatchTries_RoundLost;
        }

        private async Task StartNextRound()
        {
            if (!disposed)
            {
                ResetBoard();
                await ReadyForRoundAsync();
            }            
        }

        private void ResetBoard()
        {
            foreach (var row in TriesRows)
            {
                row.Clear();
            }

            currentRowIndex = 0;
            pendingResult = null;
            lastCode = null;
            isRoundClosed = false;
            isFinished = true;
            Clock.Reset();
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnChannelLost(object sender, EventArgs e)
        {
            Application.Current?.Dispatcher.BeginInvoke((Action)(async () => await ReconnectAsync()));
        }

        private async Task ReconnectAsync()
        {
            if (disposed || IsReconnecting)
            {
                return;
            }

            IsReconnecting = true;
            DetachChannelEvents();
            matchService.Abort();
            var session = await reconnector.ReconnectAsync(username);
            IsReconnecting = false;
            if (session == null || disposed)
            {
                session?.Client.Abort();
                LeaveAfterConnectionLost();
                return;
            }

            await ApplySession(session);
        }

        private async Task ApplySession(MatchSession session)
        {
            matchService = session.Client;
            AttachChannelEvents();
            if (!session.Snapshot.IsActive)
            {
                ReturnToRoom();
                return;
            }

            Score.Update(session.Snapshot.Scores, username);
            await SyncClock(session.Snapshot);
        }

        private async Task SyncClock(MatchSnapshot snapshot)
        {
            if (isRoundClosed)
            {
                return;
            }

            if (snapshot.IsClockRunning)
            {
                OnTimeTrialStarted(this, snapshot.RemainingMs);
                return;
            }

            ResetBoard();
            await ReadyForRoundAsync();
        }

        private void ReturnToRoom()
        {
            NavigateTo($"Pages/RoomsPages/RoomPage.xaml?roomId={roomId}");
        }

        private void LeaveAfterConnectionLost()
        {
            Clock.Stop();
            dialogService.ShowError(Properties.Resources.MatchTimeTrial_ReconnectFailed);
            ReturnToRoom();
        }

        private void LeaveAfterError(string errorDetail)
        {
            dialogService.ShowServerError(errorDetail);
            NavigateTo(RoomsPageRoute);
        }

        private void NavigateTo(string route)
        {
            navigationService.Navigate(new Uri(route, UriKind.Relative));
        }

        private static void CloseClient(ICommunicationObject client)
        {
            try
            {
                client.Close();
            }
            catch (Exception exception) when (exception is CommunicationException
                || exception is TimeoutException)
            {
                logger.Warn(exception);
                client.Abort();
            }
        }
    }
}
