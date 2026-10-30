using MasterMind_Server.Contracts;
using MasterMind_Server.Repositories.Dtos;
using System.Collections.Generic;

namespace MasterMind_Server.Repositories.Interfaces
{
    interface IRoomRepository
    {
        List<Room> GetPublicRooms();

        RoomCreationResult CreateRoom(Room room);

        string JoinRoom(Room room, string username);

        string JoinRoomWithCode(Room room, string username, string code);

        Room GetRoomByName(string roomName);

        PlayerStatusChangeResult ChangePlayerStatus(PlayerStatus status);
    }
}
