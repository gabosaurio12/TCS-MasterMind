using MasterMind_Server.Repositories.Dtos;

namespace MasterMind_Server.Services.EmailHelpers
{
    internal static class InvitationEmailBuilder
    {
        public const string Subject = "MasterMind room invitation";

        public static string BuildBody(RoomInvitationDto invitation)
        {
            return $"{invitation.InviterUsername} invited you to play MasterMind in the room "
                + $"\"{invitation.RoomName}\".\n\n"
                + $"Room code: {invitation.RoomCode}\n\n"
                + "Enter this code in the \"Code\" option of the rooms list to join the game.";
        }
    }
}
