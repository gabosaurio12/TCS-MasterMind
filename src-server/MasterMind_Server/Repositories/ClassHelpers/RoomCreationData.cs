using MasterMind_Server.Repositories.Enums;

namespace MasterMind_Server.Repositories.ClassHelpers
{
    internal class RoomCreationData
    {
        public string RoomName { get; set; }

        public RoomPrivacy Privacy { get; set; }

        public int PrivacyId { get; set; }

        public int GamemodeId { get; set; }

        public int DifficultyId { get; set; }

        public int PlayerOneId { get; set; }
    }
}
