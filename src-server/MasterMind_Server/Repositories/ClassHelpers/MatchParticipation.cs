using MasterMind_Server.Repositories.Enums;

namespace MasterMind_Server.Repositories.ClassHelpers
{
    internal class MatchParticipation
    {
        public int RoomId { get; set; }

        public bool IsPlayerOne { get; set; }

        public byte PlayerOneStatus { get; set; }

        public byte PlayerTwoStatus { get; set; }

        public int? MaxTries { get; set; }

        public bool AreBothReady
        {
            get
            {
                var ready = (byte)PlayerPossibleStatus.Ready;
                return PlayerOneStatus == ready && PlayerTwoStatus == ready;
            }
        }
    }
}
