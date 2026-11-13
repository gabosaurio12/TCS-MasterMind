using MasterMind_Client.RoomService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using MasterMind_Client.ViewModels.UserControls;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.ServiceModel;

namespace MasterMind_Client.ViewModels.Pages.RoomsPages
{
    public class RoomsPageViewModel : ViewModelBase
    {

        private readonly static ILog logger = LogManager.GetLogger(typeof(RoomsPageViewModel));

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
            CodeCommand = new RelayCommand(_ => OpenRoomCodeModal());
            CreateCommand = new RelayCommand(_ => NavigateTo("Pages/RoomsPages/CreateRoomPage.xaml"));
            BackCommand = new RelayCommand(_ => NavigateTo("Pages/MainPage.xaml"));
            LoadRooms();
        }

        private void LoadRooms()
        {
            try
            {
                var rooms = roomService.GetPublicRooms();
                foreach (var room in rooms)
                {
                    Rooms.Add(new MatchRoomViewModel(room));
                }
            }
            catch (FaultException<ErrorMessage> faultException)
            {
                logger.WarnFormat(faultException.Detail.Message);
                ShowServerError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error(faultException);
                ShowServerError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error(communicationException);
                dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }
        }

        private void ShowServerError(string errorDetail)
        {
            switch (errorDetail)
            {
                case "DataBaseError":
                    dialogService.ShowError(Properties.Resources.ErrorNotification_DatabaseError);
                    break;
                case "HashError":
                    dialogService.ShowError(Properties.Resources.ErrorNotification_Timeout);
                    break;
                default:
                    logger.WarnFormat(errorDetail);
                    dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
                    break;
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
                if (result.Equals("Success"))
                {
                    var uri = new Uri($"Pages/RoomsPages/RoomPage.xaml?roomId={room.RoomId}", UriKind.Relative);
                    navigationService.Navigate(uri);
                }
                else
                {
                    dialogService.ShowError(GetJoinErrorMessage(result));
                }
            }
            catch (FaultException<ErrorMessage> faultException)
            {
                logger.WarnFormat(faultException.Detail.Message);
                ShowServerError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error(faultException);
                ShowServerError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error(communicationException);
                dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }
            
        }

        private void OpenRoomCodeModal()
        {
            dialogService.ShowPrivateRoomCode(room => NavigateToRoom(room));
        }

        private void NavigateToRoom(Room room)
        {
            var uri = new Uri($"Pages/RoomsPages/RoomPage.xaml?roomId={room.RoomId}", UriKind.Relative);
            navigationService.Navigate(uri);
        }

        private static string GetJoinErrorMessage(string result)
        {
            string errorMessage;
            if (result.Equals("RoomIsFull"))
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
