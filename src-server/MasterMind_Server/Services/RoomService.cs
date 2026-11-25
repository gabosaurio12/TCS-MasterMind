using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Services.EmailHelpers;
using MasterMind_Server.Services.ErrorHelpers;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class RoomService : IRoomService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(RoomService));
        private readonly ErrorMessage clientMessage = new ErrorMessage();
        private readonly RoomRepository roomRepository = new RoomRepository();

        public List<Room> GetPublicRooms()
        {
            try
            {
                return roomRepository.GetPublicRooms();
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
        }

        public RoomCreationResult CreateRoom(Room room)
        {
            try
            {
                return roomRepository.CreateRoom(room);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
        }

        public string JoinRoom(Room room, string username)
        {
            try
            {
                return roomRepository.JoinRoom(room, username);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
        }

        public string JoinRoomWithCode(Room room, string username, string code)
        {
            try
            {
                return roomRepository.JoinRoomWithCode(room, username, code);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
        }

        public Room GetRoomByName(string roomName)
        {
            try
            {
                return roomRepository.GetRoomByName(roomName);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
        }

        public Room GetRoomById(int roomId)
        {
            try
            {
                return roomRepository.GetRoomById(roomId);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
        }

        public Room GetRoomByCode(string code)
        {
            try
            {
                return roomRepository.GetRoomByCode(code);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
        }

        public void InviteFriendToRoom(int roomId, string inviterUsername, string friendUsername)
        {
            try
            {
                var invitation = roomRepository.GetRoomInvitation(roomId, inviterUsername, friendUsername);
                EmailSender.Send(
                    invitation.FriendEmail,
                    InvitationEmailBuilder.Subject,
                    InvitationEmailBuilder.BuildBody(invitation));
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex);
                clientMessage.Message = ex.Message;
                throw new FaultException<ErrorMessage>(clientMessage);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildDatabaseFault(ex);
            }
        }

        private FaultException<ErrorMessage> BuildDatabaseFault(Exception ex)
        {
            clientMessage.Message = DbFaultClassifier.Classify(ex).ToString();
            return new FaultException<ErrorMessage>(clientMessage);
        }
    }
}
