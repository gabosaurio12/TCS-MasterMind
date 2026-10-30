using MasterMind_Server.Contracts;

namespace MasterMind_Server.Repositories.Dtos
{
    public class PlayerStatusChangeResult
    {
        public string Status { get; set; }

        public bool RoomDeleted { get; set; }

        public PlayerStatus ChangedPlayer { get; set; }

        public PlayerStatus EscapedPlayer { get; set; }

        public PlayerStatus PromotedPlayer { get; set; }
    }
}
