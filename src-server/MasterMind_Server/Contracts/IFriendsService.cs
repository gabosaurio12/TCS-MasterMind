using MasterMind_Server.Exceptions;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract]
    interface IFriendsService
    {
        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        List<Player> GetFriendships(int playerId);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        List<Player> GetFriendRequests(int playerId);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        Friendship GetFriendship(int addresseeId, int requesterId);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        Friendship GetFriendRequest(int addresseeId, int requesterId);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        string SendFriendRequest(int addresseeId, int requesterId);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        string AcceptFriendRequest(int requestId);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        string RejectFriendRequest(int requestId);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        string RemoveFriendship(string addresseeUsername, string requesterUsername);
    }

    [DataContract]
    class Friendship
    {
        [DataMember]
        public int FriendshipId { get; set; }
        [DataMember]
        public string AddresseeUsername { get; set; }
        [DataMember]
        public string RequesterUsername { get; set; }
        [DataMember]
        public string Status { get; set; }
    }
}
