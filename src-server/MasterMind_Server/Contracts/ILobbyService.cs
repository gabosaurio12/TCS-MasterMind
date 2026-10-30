using MasterMind_Server.Exceptions;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract(CallbackContract = typeof(ILobbyCallbackService))]
    interface ILobbyService
    {
        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        string SendMessage(ChatMessage message);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        string ChangePlayerStatus(PlayerStatus status);
    }

    [DataContract]
    public class PlayerStatus
    {
        [DataMember]
        public int RoomId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public bool IsPlayerOne { get; set; }
    }
}
