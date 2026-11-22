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
