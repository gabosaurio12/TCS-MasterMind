using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract(CallbackContract = typeof(ILobbyCallbackService))]
    interface ILobbyCallbackService
    {
        [OperationContract(IsOneWay = true)]
        void OnMessageReceivedCallback(ChatMessage message);

        [OperationContract(IsOneWay = true)]
        void OnChangePlayerStatusCallback(PlayerStatus status);
    }

    public class ChatMessage
    {
        public string SenderUsername { get; set; }
        public string Message { get; set; }
    }
}
