using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class FriendsService : IFriendsService
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(FriendsService));
        private readonly ErrorMessage clientMessage = new ErrorMessage();
        private readonly FriendsRepository friendsRepository = new FriendsRepository();

        public string AcceptFriendRequest(int requestId)
        {
            throw new NotImplementedException();
        }

        public Friendship GetFriendRequest(int addresseeId, int requesterId)
        {
            try
            {
                var friendRequest = friendsRepository.GetFriendRequest(addresseeId, requesterId);
                return friendRequest;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public List<Player> GetFriendRequests(int playerId)
        {
            try
            {
                var friendships = friendsRepository.GetFriendRequests(playerId);
                return friendships;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public Friendship GetFriendship(int addresseeId, int requesterId)
        {
            try
            {
                var friendRequest = friendsRepository.GetFriendship(addresseeId, requesterId);
                return friendRequest;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public List<Player> GetFriendships(int playerId)
        {
            try
            {
                var friendships = friendsRepository.GetFriendships(playerId);
                return friendships;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public string RejectFriendRequest(int requestId)
        {
            throw new NotImplementedException();
        }

        public string RemoveFriendship(string addresseeUsername, string requesterUsername)
        {
            throw new NotImplementedException();
        }

        public string SendFriendRequest(int addresseId, int requesterId)
        {
            throw new NotImplementedException();
        }
    }
}
