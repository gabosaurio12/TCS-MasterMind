using System.Runtime.Serialization;
using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract]
    interface ILobbyCallbackService
    {
        [OperationContract(IsOneWay = true)]
        void OnMessageReceivedCallback(ChatMessage message);

        [OperationContract(IsOneWay = true)]
        void OnChangePlayerStatusCallback(PlayerStatus status);
    }

    [DataContract]
    public class ChatMessage
    {
        [DataMember]
        public int RoomId { get; set; }

        [DataMember]
        public string SenderUsername { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}
