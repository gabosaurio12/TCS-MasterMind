using MasterMind_Client.Helpers;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ServerEndpoints;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace MasterMind_Client.ViewModels.Pages
{
    public class LoginPageViewModel : ViewModelBase
    {
        private const string SendEmailFault = "SendEmailError";
        private const string SmtpServerFault = "SmtpServerError";

        private readonly static ILog logger = LogManager.GetLogger(typeof(LoginPageViewModel));

        public string Username { get; set; }

        public string Password { get; set; }

        public bool RememberLogin { get; set; }

        public RelayCommand LoginCommand { get; }

        public RelayCommand SignupCommand { get; }

        public RelayCommand GuestCommand { get; }

        public RelayCommand LanguageCommand { get; }

        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private string emailFaultMessage;

        public LoginPageViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            LoginCommand = new RelayCommand(async _ => await Login());
            SignupCommand = new RelayCommand(_ => NavigateTo("Pages/SignupPage.xaml"));
            GuestCommand = new RelayCommand(_ => dialogService.ShowGuestName(OnGuestEntered));
            LanguageCommand = new RelayCommand(_ => ChangeLanguage());
        }

        private async Task Login()
        {
            var player = new Player
            {
                Username = Username,
                Password = Password
            };

            emailFaultMessage = null;

            if (await ValidateFormsData(player))
            {
                dialogService.ShowVerificationCode(player.Username, OnVerificationSucceded);
                NotifyCodeDelivery();
            }
        }

        private void NotifyCodeDelivery()
        {
            if (emailFaultMessage == null)
            {
                dialogService.ShowSuccess(Properties.Resources.VerificationCodeModal_CodeSent);
            }
            else
            {
                dialogService.ShowServerError(emailFaultMessage);
            }
        }

        private async Task<bool> ValidateFormsData(Player player)
        {
            if (string.IsNullOrWhiteSpace(player.Username) || string.IsNullOrWhiteSpace(player.Password))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WhiteInput);
                return false;
            }

            try
            {
                var service = ServiceClientFactory.CreatePlayerClient();
                var result = await service.LoginPlayerAsync(player.Username, player.Password);
                if (!result)
                {
                    dialogService.ShowError(Properties.Resources.ErrorNotification_WrongCredentials);
                }

                return result;
            }
            catch (FaultException<ErrorMessage> faultException)
            {
                logger.WarnFormat("The server rejected the login: {0}", faultException.Detail.Message);
                return HandleLoginFault(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error(faultException);
                dialogService.ShowServerError(faultException.Message);
                return false;
            }
            catch (CommunicationException communicationException)
            {
                logger.Error(communicationException);
                dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
                return false;
            }
        }

        private bool HandleLoginFault(string faultMessage)
        {
            if (faultMessage == SendEmailFault || faultMessage == SmtpServerFault)
            {
                emailFaultMessage = faultMessage;
                return true;
            }

            dialogService.ShowServerError(faultMessage);
            return false;
        }

        private void OnVerificationSucceded(Player verifiedPlayer)
        {
            var player = ServiceClientFactory.CreatePlayerClient().GetPlayerById(verifiedPlayer.PlayerId);
            CurrentPlayer.Instance.SetCurrentPlayer(player);

            if (RememberLogin)
            {
                Properties.Settings.Default.RememberLogin = true;
                Properties.Settings.Default.SavedUsername = player.Username;
            }
            else
            {
                Properties.Settings.Default.RememberLogin = false;
                Properties.Settings.Default.SavedUsername = "";
            }

            Properties.Settings.Default.Save();
            navigationService.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));
        }

        private void OnGuestEntered()
        {
            NavigateTo("Pages/MainPage.xaml");
        }

        private void NavigateTo(string uri)
        {
            navigationService.Navigate(new Uri(uri, UriKind.Relative));
        }

        private void ChangeLanguage()
        {
            UtilsUI.ToggleLanguage();
            navigationService.Refresh();
        }
    }
}
