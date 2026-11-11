using log4net;
using MasterMind_Client.Enums;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using System;
using System.ServiceModel;
using System.Text.RegularExpressions;

namespace MasterMind_Client.ViewModels.Pages
{
    public class UpdateProfilePageViewModel : ViewModelBase
    {

        private readonly static ILog logger = LogManager.GetLogger(typeof(UpdateProfilePageViewModel));

        private string username;
        private string email;

        public UpdateProfilePageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => NavigateTo("Pages/ProfilePage.xaml"));
            username = CurrentPlayer.Instance.Username;
            email = CurrentPlayer.Instance.Email;
        }

        public string Username
        {
            get { return this.username; }
            set { SetProperty(ref this.username, value); }
        }

        public string Email
        {
            get { return this.email; }
            set { SetProperty(ref this.email, value); }
        }

        public string BestTimeText { get; } = "";

        public string LeastTriesText { get; } = "";

        public RelayCommand SaveCommand { get; }

        public RelayCommand CancelCommand { get; }

        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly PlayerServiceClient playerService = new PlayerServiceClient();

        private void Save()
        {
            if (ValidateInput())
            {
                try
                {

                    var player = playerService.GetPlayerByUsername(CurrentPlayer.Instance.Username);
                    player.Username = username;
                    player.Email = email;
                    var result = playerService.UpdatePlayer(player);
                    if (result.Equals(ServerMessages.EmailTaken.ToString()))
                    {
                        dialogService.ShowError(Properties.Resources.ErrorNotification_EmailTaken);

                    }
                    else if (result.Equals(ServerMessages.UsernameTaken.ToString()))
                    {
                        dialogService.ShowError(Properties.Resources.ErrorNotification_UsernameTaken);
                    }
                    else if (result.Equals(ServerMessages.Success.ToString()))
                    {
                        dialogService.ShowSuccess(Properties.Resources.SuccessNotification_PlayerUpdated);
                        CurrentPlayer.Instance.SetCurrentPlayer(player);
                    }
                }
                catch (FaultException<RoomService.ErrorMessage> faultException)
                {
                    logger.WarnFormat("The server rejected the room creation: {0}", faultException.Detail.Message);
                    dialogService.ShowServerError(faultException.Detail.Message);
                }
                catch (FaultException faultException)
                {
                    logger.Error("Server fault during room creation", faultException);
                    dialogService.ShowServerError(faultException.Message);
                }
                catch (CommunicationException communicationException)
                {
                    logger.Error("Communication failure during room creation", communicationException);
                    dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
                }

                NavigateTo("Pages/ProfilePage.xaml");
            }
        }

        private bool ValidateInput()
        {
            var isValid = true;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                isValid = false;
            }

            if (Regex.IsMatch(Username, @"\s", RegexOptions.None, TimeSpan.FromMilliseconds(500)))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_NoSpaces);
                isValid = false;
            }

            return isValid;
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }
    }
}
