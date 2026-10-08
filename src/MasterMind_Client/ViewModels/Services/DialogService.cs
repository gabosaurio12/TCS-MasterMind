using MasterMind_Client.Data;
using MasterMind_Client.Modals;
using MasterMind_Client.ViewModels.Modals;
using System;

namespace MasterMind_Client.ViewModels.Services
{
    public class DialogService : IDialogService
    {
        public void ShowError(string message)
        {
            var modal = new ErrorNotificationModal();
            var viewModel = new ErrorNotificationViewModel(message);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            modal.Show();
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

        public void ShowFriends()
        {
            var modal = new FriendsModal();
            var viewModel = new FriendsViewModel(this);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            modal.Show();
        }

        public void ShowFriendRequests()
        {
            var modal = new FriendRequestsModal();
            var viewModel = new FriendRequestsViewModel(this);
            modal.DataContext = viewModel;
            viewModel.CloseRequested += (sender, e) => modal.Close();
            modal.Show();
        }

        public void ShowLogOut()
        {
            var modal = new LogOutModal();
            var viewModel = new LogOutViewModel();
            modal.DataContext = viewModel;
            modal.Show();
        }
    }
}
