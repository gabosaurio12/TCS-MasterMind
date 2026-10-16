using MasterMind_Server.Exceptions;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract]
    interface IPlayerService
    {
        [OperationContract]
        (bool, string) RegisterPlayer(Player player);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        bool LoginPlayer(string username, string password);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        Player GetPlayerByUsername(string username);
    }

    [DataContract]
    public class Player
    {
        [DataMember]
        public int PlayerId { get; set; }
        public string Email { get; set; }
        public int NumberOfReports { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AvatarPath { get; set; }
        public bool IsOnline { get; set; }
    }


}
