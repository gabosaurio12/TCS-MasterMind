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

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        Player GetPlayerById(int playerId);

    }

    [DataContract]
    public class Player
    {
        [DataMember]
        public int PlayerId { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public int NumberOfReports { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string AvatarPath { get; set; }
        [DataMember]
        public bool IsOnline { get; set; }
    }


}
