using System;

namespace MasterMind_Server.Repositories.ClassHelpers
{
    internal class MatchRoomInfo
    {
        public int RoomId { get; set; }

        public string PlayerOneUsername { get; set; }

        public string PlayerTwoUsername { get; set; }

        public int PlayerOneTries { get; set; }

        public int PlayerTwoTries { get; set; }

        public bool IsTimeTrial { get; set; }

        public int RoundSeconds { get; set; }

        public string GetOpponent(string username)
        {
            bool isPlayerOne = string.Equals(PlayerOneUsername, username, StringComparison.OrdinalIgnoreCase);
            return isPlayerOne ? PlayerTwoUsername : PlayerOneUsername;
        }
    }
}
