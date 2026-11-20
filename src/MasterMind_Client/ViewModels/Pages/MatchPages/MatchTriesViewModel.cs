using log4net;
using MasterMind_Client.Helpers;
using MasterMind_Client.MatchService;
using MasterMind_Client.RoomService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MasterMind_Client.ViewModels.Pages.MatchPages
{
    public class MatchTriesViewModel : ViewModelBase, IDisposable
    {
        private const string RoomsPageRoute = "Pages/RoomsPages/RoomsPage.xaml";
        private const string TriesExhaustedResult = "TriesExhausted";

        private static readonly ILog logger = LogManager.GetLogger(typeof(MatchTriesViewModel));

        private readonly int roomId;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly MatchServiceClient matchService = new MatchServiceClient();
        private readonly RoomServiceClient roomService = new RoomServiceClient();

        private int remainingTries;
        private int currentRowIndex;
        private bool isSending;
        private bool isFinished;
        private PaletteColourViewModel selectedColour;

        public MatchTriesViewModel(Uri uri, INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            roomId = RouteQuery.GetInt(uri, "roomId");

            CreateBoard();
            CreatePalette();
            SelectColourCommand = new RelayCommand(parameter => SelectColour(parameter as PaletteColourViewModel));
            TryCommand = new RelayCommand(async _ => await SubmitTryAsync(), _ => CanSubmitTry());
        }

        public int RemainingTries
        {
            get => remainingTries;
            private set => SetProperty(ref remainingTries, value);
        }

        public ObservableCollection<MatchRowViewModel> TriesRows { get; } =
            new ObservableCollection<MatchRowViewModel>();

        public ObservableCollection<PaletteColourViewModel> PaletteColours { get; } =
            new ObservableCollection<PaletteColourViewModel>();

        public ICommand SelectColourCommand { get; }

        public ICommand TryCommand { get; }

        public async Task InitializeAsync()
        {
            if (roomId == 0)
            {
                NavigateTo(RoomsPageRoute);
                return;
            }

            await LoadRemainingTriesAsync();
        }

        public void Dispose()
        {
            CloseClient(matchService);
            CloseClient(roomService);
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
            if (colour == null)
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

        private async Task LoadRemainingTriesAsync()
        {
            try
            {
                var room = await roomService.GetRoomByIdAsync(roomId);
                RemainingTries = MatchRules.GetMaxTries(room?.Difficulty);
            }
            catch (FaultException<RoomService.ErrorMessage> faultException)
            {
                logger.WarnFormat("The server rejected the room lookup: {0}", faultException.Detail.Message);
                LeaveAfterError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error("Server fault during room lookup", faultException);
                LeaveAfterError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error("Communication failure during room lookup", communicationException);
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
                logger.WarnFormat("The server rejected the try: {0}", faultException.Detail.Message);
                HandleFault(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error("Server fault while validating a try", faultException);
                dialogService.ShowServerError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error("Communication failure while validating a try", communicationException);
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
            isFinished = true;
            dialogService.ShowYouWon(code, ReturnToRoom);
        }

        private void FinishLost()
        {
            isFinished = true;
            dialogService.ShowGameOver(ReturnToRoom);
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
                logger.Warn("Could not close a WCF client gracefully", communicationException);
                client.Abort();
            }
            catch (TimeoutException timeoutException)
            {
                logger.Warn("Timeout while closing a WCF client", timeoutException);
                client.Abort();
            }
        }
    }
}
