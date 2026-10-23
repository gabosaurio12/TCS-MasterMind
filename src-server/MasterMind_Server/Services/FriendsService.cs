using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class FriendsService : IFriendsService
    {
        private readonly ErrorMessage clientMessage = new ErrorMessage();
        private readonly FriendsRepository friendsRepository = new FriendsRepository();

        public string AcceptFriendRequest(int requestId)
        {
            try
            {
                var result = friendsRepository.AcceptFriendRequest(requestId);
                return result;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
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
            try
            {
                var result = friendsRepository.RejectFriendRequest(requestId);
                return result;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public string RemoveFriendship(string addresseeUsername, string requesterUsername)
        {
            try
            {
                var result = friendsRepository.RemoveFriendship(addresseeUsername, requesterUsername);
                return result;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public string SendFriendRequest(int addresseeId, int requesterId)
        {
            try
            {
                var result = friendsRepository.SendFriendRequest(addresseeId, requesterId);
                return result;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }
    }
}
