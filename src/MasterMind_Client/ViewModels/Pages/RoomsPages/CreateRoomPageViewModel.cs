using log4net;
using MasterMind_Client.RoomService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;
using System.ServiceModel;
using System.Text.RegularExpressions;

namespace MasterMind_Client.ViewModels.Pages.RoomsPages
{
    public class CreateRoomPageViewModel : ViewModelBase
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(CreateRoomPageViewModel));

        public string RoomName { get; set; }
        public bool IsTimeTrial { get; set; }
        public bool IsTries { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsPublic { get; set; }
        public bool IsEasy { get; set; }
        public bool IsNormal { get; set; }
        public bool IsHard { get; set; }
        public bool IsEnigma { get; set; }

        public RelayCommand CreateCommand { get; }
        public RelayCommand CancelCommand { get; }

        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;

        private readonly RoomServiceClient roomService = new RoomServiceClient();

        public CreateRoomPageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            CreateCommand = new RelayCommand(_ => Create());
            CancelCommand = new RelayCommand(_ => NavigateTo("Pages/RoomsPages/RoomsPage.xaml"));
        }

        private void Create()
        {
            if (ValidateInput())
            {
                var matchRoom = new Room
                {
                    Name = RoomName.Trim(),
                    Gamemode = IsTimeTrial ? "TimeTrial" : "Tries",
                    Difficulty = GetDifficulty(),
                    PlayerOneUsername = CurrentPlayer.Instance.Username,
                    RoomPrivacy = IsPrivate ? "Private" : "Public"
                };
                try
                {
                    var result = roomService.CreateRoom(matchRoom);
                    if (result.Status.Equals("Success"))
                    {
                        dialogService.ShowSuccess(Properties.Resources.SuccessNotification_MatchRoomCreatedSuccessfully);

                        var room = roomService.GetRoomByName(RoomName.Trim());
                        if (room != null && room.RoomId > 0)
                        {
                            var uri = new Uri($"Pages/RoomsPages/RoomPage.xaml?roomId={room.RoomId}", UriKind.Relative);
                            navigationService.Navigate(uri);
                        } 
                        else
                        {
                            dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorJoiningMatchRoom);
                            NavigateTo("Pages/RoomsPages/RoomsPage.xaml");
                        }
                    }
                    else if (result.Status.Equals("RoomNameTaken"))
                    {
                        dialogService.ShowError(Properties.Resources.ErrorNotification_RoomNameTaken);
                    }
                    else
                    {
                        dialogService.ShowError(Properties.Resources.ErrorNotification_UnexpectedError);
                    }
                }
                catch (FaultException<ErrorMessage> faultException)
                {
                    logger.WarnFormat(faultException.Detail.Message);
                    dialogService.ShowServerError(faultException.Detail.Message);
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
        }

        private bool ValidateInput()
        {
            var roomName = RoomName?.Trim();
            var isValid = true;

            if (string.IsNullOrWhiteSpace(roomName))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                isValid = false;
            }

            if (roomName != null && Regex.IsMatch(roomName, @"\s", RegexOptions.None, TimeSpan.FromMilliseconds(500)))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_NoSpaces);
                isValid = false;
            }

            return isValid;
        }

        private string GetDifficulty()
        {
            if (IsEasy)
            {
                return "Easy";
            }
            else if (IsNormal)
            {
                return "Normal";
            }
            else if (IsHard)
            {
                return "Hard";
            }
            else
            {
                return "Enigma";
            }
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }
    }
}
