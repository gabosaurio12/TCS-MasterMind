using MasterMind_Server.Exceptions;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract(CallbackContract = typeof(IMatchCallbackService))]
    public interface IMatchService
    {
        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        string UpdateCode(string username);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        Clues ValidateTry(string username, CodeTry codeTry);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        int GetMaxTries(string username);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        void Subscribe(string username);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        void Unsubscribe(string username);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        void ReadyForRound(string username);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        MatchSnapshot GetMatchSnapshot(string username);
    }

    [DataContract]
    public class MatchSnapshot
    {
        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public bool IsTimeTrial { get; set; }

        [DataMember]
        public bool IsClockRunning { get; set; }

        [DataMember]
        public int RemainingMs { get; set; }

        [DataMember]
        public int RoundNumber { get; set; }

        [DataMember]
        public PlayerScore[] Scores { get; set; }
    }

    [DataContract]
    public class Clues
    {
        [DataMember]
        public string[] CluesResult { get; set; }

        [DataMember]
        public bool IsCorrect { get; set; }
    }

    [DataContract]
    public class CodeTry
    {
        [DataMember]
        public string[] Code { get; set; }
    }
}
