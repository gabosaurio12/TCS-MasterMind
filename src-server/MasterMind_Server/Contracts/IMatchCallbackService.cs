using System.Runtime.Serialization;
using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract]
    interface IMatchCallbackService
    {
        [OperationContract(IsOneWay = true)]
        void OnOpponentFinishedCallback(OpponentProgress progress);

        [OperationContract(IsOneWay = true)]
        void OnRoundResolvedCallback(RoundResult result);

        [OperationContract(IsOneWay = true)]
        void OnOpponentAbandonedCallback(string username);

        [OperationContract(IsOneWay = true)]
        void OnTimeTrialStartedCallback(int remainingMs);
    }

    [DataContract]
    public class OpponentProgress
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public bool Solved { get; set; }

        [DataMember]
        public int TriesUsed { get; set; }
    }

    [DataContract]
    public class RoundResult
    {
        [DataMember]
        public string WinnerUsername { get; set; }

        [DataMember]
        public OpponentProgress[] Players { get; set; }

        [DataMember]
        public int RoundNumber { get; set; }

        [DataMember]
        public PlayerScore[] Scores { get; set; }

        [DataMember]
        public bool IsMatchOver { get; set; }
    }

    [DataContract]
    public class PlayerScore
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int Points { get; set; }
    }
}
