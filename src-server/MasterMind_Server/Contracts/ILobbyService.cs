using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract]
    interface ILobbyService
    {
        [OperationContract]
        string SendMessage(string message);

        [OperationContract]
        string ChangePlayerStatus(PlayerStatus status);
    }

    public class PlayerStatus
    {
        public string Username { get; set; }
        public string Status { get; set; }
    }
}
