using MasterMind_Client.PlayerService;
using MasterMind_Client.RoomService;
using MasterMind_Client.ViewModels.Pages.MatchPages;
using System;
using System.Collections.Generic;

namespace MasterMind_Client.ViewModels.Services
{
    public interface IDialogService
    {
        void ShowError(string message);

        void ShowServerError(string detailMessage);

        void ShowSuccess(string message, Action modalClosed = null);

        void ShowVerificationCode(string username, Action<Player> verificationSucceded);

        void ShowPrivateRoomCode(Action<Room> roomJoined);

        void ShowChangePassword();

        void ShowFriends();

        void ShowFriendRequests();

        void ShowLogOut();

        void ShowYouWon(IReadOnlyList<PinColour> secretCode, Action continued);

        void ShowGameOver(Action continued);

        string PickImageFile();
    }
}
