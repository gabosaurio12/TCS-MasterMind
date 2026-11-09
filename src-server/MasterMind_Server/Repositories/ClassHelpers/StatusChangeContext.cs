using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Dtos;
using MasterMind_Server.Repositories.Enums;

namespace MasterMind_Server.Repositories.ClassHelpers
{
    internal class StatusChangeContext
    {
        public MatchRoom Room { get; set; }

        public RoomMember Member { get; set; }

        public PlayerPossibleStatus NewStatus { get; set; }

        public PlayerStatusChangeResult Result { get; set; }
    }
}
