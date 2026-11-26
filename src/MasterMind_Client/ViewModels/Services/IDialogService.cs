using MasterMind_Client.PlayerService;
using MasterMind_Client.RoomService;
using MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        void ShowGuestName(Action guestEntered);

        void ShowFriends();

        void ShowFriends(int roomId);

        void ShowFriendRequests();

        void ShowLogOut();

        void ShowYouWon(IReadOnlyList<PinColour> secretCode, Action continued);

        void ShowGameOver(string title, Action continued);

        bool ShowMatchMenu(Func<Task> exitConfirmed, Action menuClosed);

        void CloseMatchMenu();

        string PickImageFile();
    }
}
