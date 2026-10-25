using System.Collections.Generic;
using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract]
    interface IRoomService
    {
        [OperationContract]
        List<Room> GetPublicRooms();

        [OperationContract]
        string CreateRoom(Room room);

        [OperationContract]
        string JoinRoom(Room room, string username);

        [OperationContract]
        string JoinRoomWithCode(Room room, string username, string code);

        [OperationContract]
        Room GetRoomByName(string roomName);
    }

    public class Room
    {
        public int RoomId { get; set; }
        public string Name { get; set; }
        public string Gamemode { get; set; }
        public string Difficulty { get; set; }
        public string PlayerOneUsername { get; set; }
        public string PlayerTwoUsername { get; set; }
        public string RoomPrivacy { get; set; }
        public string RoomCode { get; set; }
    }
}
