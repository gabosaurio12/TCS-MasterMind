using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class RoomService : IRoomService
    {
        private readonly ErrorMessage clientMessage = new ErrorMessage();
        private readonly IRoomRepository roomRepository = new RoomRepository();

        public List<Room> GetPublicRooms()
        {
            try
            {
                return roomRepository.GetPublicRooms();
            }
            catch (EntityException)
            {
                throw BuildDatabaseFault();
            }
            catch (DbUpdateException)
            {
                throw BuildDatabaseFault();
            }
        }

        public RoomCreationResult CreateRoom(Room room)
        {
            try
            {
                return roomRepository.CreateRoom(room);
            }
            catch (EntityException)
            {
                throw BuildDatabaseFault();
            }
            catch (DbUpdateException)
            {
                throw BuildDatabaseFault();
            }
        }

        public string JoinRoom(Room room, string username)
        {
            try
            {
                return roomRepository.JoinRoom(room, username);
            }
            catch (EntityException)
            {
                throw BuildDatabaseFault();
            }
            catch (DbUpdateException)
            {
                throw BuildDatabaseFault();
            }
        }

        public string JoinRoomWithCode(Room room, string username, string code)
        {
            try
            {
                return roomRepository.JoinRoomWithCode(room, username, code);
            }
            catch (EntityException)
            {
                throw BuildDatabaseFault();
            }
            catch (DbUpdateException)
            {
                throw BuildDatabaseFault();
            }
        }

        public Room GetRoomByName(string roomName)
        {
            try
            {
                return roomRepository.GetRoomByName(roomName);
            }
            catch (EntityException)
            {
                throw BuildDatabaseFault();
            }
            catch (DbUpdateException)
            {
                throw BuildDatabaseFault();
            }
        }

        private FaultException<ErrorMessage> BuildDatabaseFault()
        {
            clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
            return new FaultException<ErrorMessage>(clientMessage);
        }
    }
}
