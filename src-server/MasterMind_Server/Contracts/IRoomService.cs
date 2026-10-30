using MasterMind_Server.Exceptions;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract]
    interface IRoomService
    {
        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        List<Room> GetPublicRooms();

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        RoomCreationResult CreateRoom(Room room);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        string JoinRoom(Room room, string username);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        string JoinRoomWithCode(Room room, string username, string code);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        Room GetRoomByName(string roomName);
    }

    [DataContract]
    public class Room
    {
        [DataMember]
        public int RoomId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Gamemode { get; set; }

        [DataMember]
        public string Difficulty { get; set; }

        [DataMember]
        public string PlayerOneUsername { get; set; }

        [DataMember]
        public string PlayerTwoUsername { get; set; }

        [DataMember]
        public string RoomPrivacy { get; set; }

        [DataMember]
        public string RoomCode { get; set; }
    }

    [DataContract]
    public class RoomCreationResult
    {
        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public Room Room { get; set; }
    }
}
