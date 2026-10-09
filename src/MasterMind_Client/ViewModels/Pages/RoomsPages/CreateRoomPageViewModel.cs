using MasterMind_Client.TempData;
using MasterMind_Client.TempData.DTO;
using MasterMind_Client.TempData.Enum;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;
using System.Text.RegularExpressions;

namespace MasterMind_Client.ViewModels.Pages.RoomsPages
{
    public class CreateRoomPageViewModel : ViewModelBase
    {
        public CreateRoomPageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            CreateCommand = new RelayCommand(_ => Create());
            CancelCommand = new RelayCommand(_ => NavigateTo("Pages/RoomsPages/RoomsPage.xaml"));
        }

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

        private void Create()
        {
            if (!ValidateInput())
            {
                return;
            }

            var matchRoom = new MatchRoomDto
            {
                RoomName = RoomName.Trim(),
                Gamemode = IsTimeTrial ? "TimeTrial" : "Tries",
                Difficulty = GetDifficulty(),
                Privacy = IsPrivate ? "Private" : "Public"
            };

            var result = TempMatchRoomsService.CreateMatchRoom(matchRoom);
            if (result == MatchRoomCreationResultEnum.Success)
            {
                dialogService.ShowSuccess(Properties.Resources.SuccessNotification_MatchRoomCreatedSuccessfully);
                NavigateTo("Pages/RoomsPages/RoomsPage.xaml");
            }
            else
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorCreatingMatchRoom);
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

            if (roomName != null && Regex.IsMatch(roomName, @"\s"))
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
