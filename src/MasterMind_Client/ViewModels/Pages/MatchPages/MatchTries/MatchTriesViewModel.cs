using MasterMind_Client.ServerEndpoints;
using log4net;
using MasterMind_Client.Helpers;
using MasterMind_Client.MatchService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Pages.MatchPages.MatchTries.Helpers;
using MasterMind_Client.ViewModels.Pages.MatchPages.MatchTries.ViewModels;
using MasterMind_Client.ViewModels.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchTries
{
    public class MatchTriesViewModel : ViewModelBase, IDisposable
    {
        private const string RoomsPageRoute = "Pages/RoomsPages/RoomsPage.xaml";
        private const string TriesExhaustedResult = "TriesExhausted";

        private static readonly ILog logger = LogManager.GetLogger(typeof(MatchTriesViewModel));

        private readonly int roomId;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly MatchCallbackHandler callbackHandler = new MatchCallbackHandler();
        private readonly MatchServiceClient matchService;

        private int remainingTries;
        private int maxTries;
        private int currentRowIndex;
        private bool isSending;
        private bool isFinished;
        private bool isRoundClosed;
        private bool isSubscribed;
        private bool disposed;
        private bool isWaitingForOpponent;
        private RoundResult pendingResult;
        private IReadOnlyList<PinColour> lastCode;
        private PaletteColourViewModel selectedColour;

        public int RemainingTries
        {
            get => remainingTries;
            private set => SetProperty(ref remainingTries, value);
        }

        public bool IsWaitingForOpponent
        {
            get => isWaitingForOpponent;
            private set => SetProperty(ref isWaitingForOpponent, value);
        }

        public MatchScoreViewModel Score { get; } = new MatchScoreViewModel();

        public ObservableCollection<MatchRowViewModel> TriesRows { get; } =
            new ObservableCollection<MatchRowViewModel>();

        public ObservableCollection<PaletteColourViewModel> PaletteColours { get; } =
            new ObservableCollection<PaletteColourViewModel>();

        public ICommand SelectColourCommand { get; }

        public ICommand TryCommand { get; }

        public MatchTriesViewModel(Uri uri, INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            roomId = RouteQuery.GetInt(uri, "roomId");
            
            matchService = ServiceClientFactory.CreateMatchClient(new InstanceContext(callbackHandler));

            callbackHandler.OpponentFinished += OnOpponentFinished;
            callbackHandler.RoundResolved += OnRoundResolved;
            callbackHandler.OpponentAbandoned += OnOpponentAbandoned;

            CreateBoard();
            CreatePalette();
            SelectColourCommand = new RelayCommand(parameter => SelectColour(parameter as PaletteColourViewModel));
            TryCommand = new RelayCommand(async _ => await SubmitTryAsync(), _ => CanSubmitTry());
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
                await LoadRemainingTriesAsync();
            }
        }

        public void Dispose()
        {
            disposed = true;
            callbackHandler.OpponentFinished -= OnOpponentFinished;
            callbackHandler.RoundResolved -= OnRoundResolved;
            callbackHandler.OpponentAbandoned -= OnOpponentAbandoned;
            Unsubscribe();
            CloseClient(matchService);
        }

        private MatchRowViewModel CurrentRow => TriesRows[currentRowIndex];

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

        private void SelectColour(PaletteColourViewModel colour)
        {
            if (colour != null)
            {
                foreach (var entry in PaletteColours)
                {
                    entry.IsSelected = entry == colour;
                }

                selectedColour = colour;
            }
        }

        private void PlacePin(MatchPinSlotViewModel slot)
        {
            if (isFinished || !CurrentRow.PinSlots.Contains(slot))
            {
                return;
            }

            slot.Colour = selectedColour.Colour;
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanSubmitTry()
        {
            return RemainingTries > 0 && !isSending && !isFinished && CurrentRow.IsComplete;
        }

        private async Task<bool> SubscribeAsync()
        {
            try
            {
                await matchService.SubscribeAsync(CurrentPlayer.Instance.Username);
                isSubscribed = true;
            }
            catch (FaultException<MatchService.ErrorMessage> faultException)
            {
                logger.WarnFormat(faultException.Detail.Message);
                LeaveAfterError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error(faultException);
                LeaveAfterError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error(communicationException);
                LeaveAfterError(string.Empty);
            }

            return isSubscribed;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            try
            {
                matchService.Unsubscribe(CurrentPlayer.Instance.Username);
            }
            catch (Exception exception) when (exception is CommunicationException
                || exception is TimeoutException)
            {
                logger.Warn(exception);
            }
        }

        private async Task LoadRemainingTriesAsync()
        {
            try
            {
                maxTries = await matchService.GetMaxTriesAsync(CurrentPlayer.Instance.Username);
                RemainingTries = maxTries;
            }
            catch (FaultException<ErrorMessage> faultException)
            {
                logger.WarnFormat(faultException.Detail.Message);
                LeaveAfterError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error(faultException);
                LeaveAfterError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error(communicationException);
                LeaveAfterError(string.Empty);
            }
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
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private async Task RequestTryAsync(IReadOnlyList<PinColour> code)
        {
            try
            {
                var clues = await matchService.ValidateTryAsync(
                    CurrentPlayer.Instance.Username, MatchColourMapper.ToCodeTry(code));
                HandleClues(clues, code);
            }
            catch (FaultException<MatchService.ErrorMessage> faultException)
            {
                logger.WarnFormat(faultException.Detail.Message);
                HandleFault(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error(faultException);
                dialogService.ShowServerError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error(communicationException);
                dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }
        }

        private void HandleClues(Clues clues, IReadOnlyList<PinColour> code)
        {
            CurrentRow.ApplyClues(clues.CluesResult);

            if (clues.IsCorrect)
            {
                FinishWon(code);
                return;
            }

            RemainingTries--;
            if (RemainingTries == 0)
            {
                FinishLost();
                return;
            }

            AdvanceRow();
        }

        private void HandleFault(string errorDetail)
        {
            if (errorDetail == TriesExhaustedResult)
            {
                FinishLost();
                return;
            }

            dialogService.ShowServerError(errorDetail);
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

        private void FinishWon(IReadOnlyList<PinColour> code)
        {
            lastCode = code;
            FinishLocally();
        }

        private void FinishLost()
        {
            FinishLocally();
        }

        private void FinishLocally()
        {
            isFinished = true;
            if (isRoundClosed)
            {
                return;
            }

            if (pendingResult != null)
            {
                ShowResult(pendingResult);
                return;
            }

            IsWaitingForOpponent = true;
        }

        private void OnOpponentFinished(object sender, OpponentProgress progress)
        {
            logger.InfoFormat("Opponent {0} finished (solved: {1}, tries: {2})",
                progress.Username, progress.Solved, progress.TriesUsed);
        }

        private void OnRoundResolved(object sender, RoundResult result)
        {
            if (disposed || isRoundClosed)
            {
                return;
            }

            if (!isFinished)
            {
                pendingResult = result;
                return;
            }

            ShowResult(result);
        }

        private void OnOpponentAbandoned(object sender, string username)
        {
            if (disposed || isRoundClosed)
            {
                return;
            }

            CloseRound();
            dialogService.ShowSuccess(Properties.Resources.MatchTries_OpponentAbandoned, ReturnToRoom);
        }

        private void CloseRound()
        {
            isRoundClosed = true;
            isFinished = true;
            IsWaitingForOpponent = false;
        }

        private void ShowResult(RoundResult result)
        {
            CloseRound();
            string ownUsername = CurrentPlayer.Instance.Username;
            Score.Update(result.Scores, ownUsername);
            Action continued = result.IsMatchOver ? (Action)ReturnToRoom : StartNextRound;
            if (string.Equals(result.WinnerUsername, ownUsername, StringComparison.OrdinalIgnoreCase))
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
                return Properties.Resources.MatchTries_RoundTie;
            }

            return result.IsMatchOver
                ? Properties.Resources.MatchTries_MatchLost
                : Properties.Resources.MatchTries_RoundLost;
        }

        private void StartNextRound()
        {
            if (disposed)
            {
                return;
            }

            foreach (var row in TriesRows)
            {
                row.Clear();
            }

            currentRowIndex = 0;
            pendingResult = null;
            lastCode = null;
            isRoundClosed = false;
            isFinished = false;
            RemainingTries = maxTries;
            CommandManager.InvalidateRequerySuggested();
        }

        private void ReturnToRoom()
        {
            NavigateTo($"Pages/RoomsPages/RoomPage.xaml?roomId={roomId}");
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
            catch (CommunicationException communicationException)
            {
                logger.Warn(communicationException);
                client.Abort();
            }
            catch (TimeoutException timeoutException)
            {
                logger.Warn(timeoutException);
                client.Abort();
            }
        }
    }
}
