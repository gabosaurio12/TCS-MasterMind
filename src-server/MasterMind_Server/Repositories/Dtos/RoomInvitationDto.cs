namespace MasterMind_Server.Repositories.Dtos
{
    internal class RoomInvitationDto
    {
        public string FriendEmail { get; set; }

        public string InviterUsername { get; set; }

        public string RoomName { get; set; }

        public string RoomCode { get; set; }
    }
}
