using MasterMind_Client.Data;
using System;

namespace MasterMind_Client.ViewModels.Services
{
    public interface IDialogService
    {
        void ShowError(string message);

        void ShowSuccess(string message, Action modalClosed = null);

        void ShowVerificationCode(string username, Action<Player> verificationSucceded);

        void ShowFriends();

        void ShowFriendRequests();

        void ShowLogOut();
    }
}
