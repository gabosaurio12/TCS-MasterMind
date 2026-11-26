using System;

namespace MasterMind_Server.Repositories.ClassHelpers
{
    internal class MatchRoundRecord
    {
        public int RoomId { get; set; }

        public int RoundNumber { get; set; }

        public int PlayerOnePoints { get; set; }

        public int PlayerTwoPoints { get; set; }

        public DateTime? StartedAtUtc { get; set; }

        public DateTime? DeadlineUtc { get; set; }

        public string FinishedUsername { get; set; }

        public bool FinishedSolved { get; set; }

        public int FinishedTries { get; set; }
    }
}
