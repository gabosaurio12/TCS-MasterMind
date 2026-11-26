using MasterMind_Client.Modals;
using MasterMind_Client.PlayerService;
using MasterMind_Client.RoomService;
using MasterMind_Client.ViewModels.Modals;
using MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MasterMind_Client.ViewModels.Services
{
    public class DialogService : IDialogService
    {
        private const string ImageFileFilter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

        private const double ModalDesignHeight = 857;
        private const double ModalDesignWidth = 628;

        private static readonly ILog logger = LogManager.GetLogger(typeof(DialogService));

        private static readonly Dictionary<string, Func<string>> serverErrorMessages =
            new Dictionary<string, Func<string>>
            {
                { "DataBaseError", () => Properties.Resources.ErrorNotification_DatabaseError },
                { "DbServerError", () => Properties.Resources.ErrorNotification_DbServerError },
                { "SendEmailError", () => Properties.Resources.ErrorNotification_SendEmailError },
                { "SmtpServerError", () => Properties.Resources.ErrorNotification_SmtpServerError },
                { "HashError", () => Properties.Resources.ErrorNotification_Timeout },
                { "VerificationCodeExpired", () => Properties.Resources.ErrorNotification_CodeExpired },
                { "InvalidImage", () => Properties.Resources.ErrorNotification_InvalidImage },
                { "ImageTooLarge", () => Properties.Resources.ErrorNotification_ImageTooLarge },
                { "ImageProcessingError", () => Properties.Resources.ErrorNotification_AvatarNotSaved },
                { "NotFriends", () => Properties.Resources.ErrorNotification_NotFriends },
                { "RoomFull", () => Properties.Resources.ErrorNotification_PlayerAlreadyJoined },
                { "NotRoomMember", () => Properties.Resources.ErrorNotification_InvitationNotSent }
            };

        public void ShowError(string message)
        {
            var modal = new ErrorNotificationModal();
            var viewModel = new ErrorNotificationViewModel(message);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            modal.Show();
        }

        public void ShowServerError(string detailMessage)
        {
            Func<string> messageProvider;

            if (detailMessage != null && serverErrorMessages.TryGetValue(detailMessage, out messageProvider))
            {
                ShowError(messageProvider());
            }
            else
            {
                logger.WarnFormat("Unknown server error: {0}", detailMessage);
                ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }
        }

        public void ShowSuccess(string message, Action modalClosed = null)
        {
            var modal = new SuccessNotificationModal();
            var viewModel = new SuccessNotificationViewModel(message, modalClosed != null);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            if (modalClosed != null)
            {
                viewModel.ModalClosed += (sender, e) => modalClosed();
            }

            modal.Show();
        }

        public void ShowVerificationCode(string username, Action<Player> verificationSucceded)
        {
            var modal = new VerificationCodeModal();
            var viewModel = new VerificationCodeViewModel(username, this);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            viewModel.VerificationSucceded += (sender, player) =>
            {
                verificationSucceded?.Invoke(player);
                modal.Close();
            };
            modal.Show();
        }

        public void ShowPrivateRoomCode(Action<Room> roomJoined)
        {
            var modal = new PrivateRoomCodeModal();
            var viewModel = new PrivateRoomCodeViewModel(this);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            viewModel.RoomJoined += (sender, room) =>
            {
                modal.Close();
                roomJoined?.Invoke(room);
            };
            modal.Show();
        }

        public void ShowChangePassword()
        {
            var modal = new ChangePasswordModal();
            var viewModel = new ChangePasswordViewModel(this);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            modal.Show();
        }

        public void ShowGuestName(Action guestEntered)
        {
            var modal = new GuestNameModal();
            var viewModel = new GuestNameViewModel(this);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            viewModel.GuestEntered += (sender, e) =>
            {
                modal.Close();
                guestEntered?.Invoke();
            };
            modal.Show();
        }

        public void ShowFriends()
        {
            ShowFriends(FriendsViewModel.NoRoom);
        }

        public void ShowFriends(int roomId)
        {
            var modal = new FriendsModal();
            var viewModel = new FriendsViewModel(this, roomId);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            Present(modal);
        }

        public void ShowFriendRequests()
        {
            var modal = new FriendRequestsModal();
            var viewModel = new FriendRequestsViewModel(this);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            Present(modal);
        }

        public void ShowLogOut()
        {
            var modal = new LogOutModal();
            var viewModel = new LogOutViewModel();
            modal.DataContext = viewModel;
            modal.Show();
        }

        public void ShowYouWon(IReadOnlyList<PinColour> secretCode, Action continued)
        {
            var modal = new YouWonTriesModal();
            var viewModel = new YouWonTriesViewModel(secretCode);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            modal.Closed += (sender, e) => continued?.Invoke();
            modal.Show();
        }

        public void ShowGameOver(string title, Action continued)
        {
            var modal = new GameOverTriesModal();
            var viewModel = new GameOverTriesViewModel(title);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            modal.Closed += (sender, e) => continued?.Invoke();
            modal.Show();
        }

        public bool ShowMatchMenu(Func<Task> exitConfirmed, Action menuClosed)
        {
            if (HasOpenModal())
            {
                return false;
            }

            var modal = new MatchMenuModal();
            var viewModel = new MatchMenuViewModel(exitConfirmed);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            modal.Closed += (sender, e) => menuClosed?.Invoke();
            Present(modal);
            return true;
        }

        public void CloseMatchMenu()
        {
            Application.Current.Windows.OfType<MatchMenuModal>().FirstOrDefault()?.Close();
        }

        private static bool HasOpenModal()
        {
            return Application.Current.Windows.OfType<Window>()
                .Any(window => window != Application.Current.MainWindow && window.IsVisible);
        }

        public string PickImageFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = ImageFileFilter,
                CheckFileExists = true
            };

            return dialog.ShowDialog() == true ? dialog.FileName : string.Empty;
        }

        private static void Present(Window modal)
        {
            FitToWorkArea(modal);
            modal.Owner = Application.Current.MainWindow;
            modal.Show();
        }

        private static void FitToWorkArea(Window modal)
        {
            double availableHeight = SystemParameters.WorkArea.Height;

            if (availableHeight >= ModalDesignHeight)
            {
                return;
            }

            modal.Height = availableHeight;
            modal.Width = availableHeight * ModalDesignWidth / ModalDesignHeight;
        }
    }
}
