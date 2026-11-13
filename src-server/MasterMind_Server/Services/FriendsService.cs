using System;
using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class FriendsService : IFriendsService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(FriendsService));

        private static readonly ConcurrentDictionary<string, IFriendsCallbackService> friendsClientsByModal =
            new ConcurrentDictionary<string, IFriendsCallbackService>();

        private readonly ErrorMessage clientMessage = new ErrorMessage();
        private readonly FriendsRepository friendsRepository = new FriendsRepository();

        public string AcceptFriendRequest(int requestId)
        {
            try
            {
                var result = friendsRepository.AcceptFriendRequest(requestId);

                if (result.Equals(RepositoriesMessages.Success.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    var requesterUsername = friendsRepository.GetRequesterUsernameByRequestId(requestId);
                    NotifyFriendshipStatusChanged(requesterUsername);
                }

                return result;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        private void NotifyFriendshipStatusChanged(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                if (friendsClientsByModal.TryGetValue(username, out var callback))
                {
                    try
                    {
                        callback.OnFriendshipStatusChanged();
                    }
                    catch (Exception ex)
                    {
                        logger.WarnFormat(username, ex.Message);
                        friendsClientsByModal.TryRemove(username, out _);
                    }
                }
            }
        }

        public void RegisterFriendsClient(int friendshipId, string username)
        {
            try
            {
                var callback = OperationContext.Current?.GetCallbackChannel<IFriendsCallbackService>();
                if (callback != null)
                {
                    friendsClientsByModal[username] = callback;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public Friendship GetFriendRequest(int addresseeId, int requesterId)
        {
            try
            {
                var friendRequest = friendsRepository.GetFriendRequest(addresseeId, requesterId);
                return friendRequest;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
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
            catch (EntityException ex)
            {
                logger.Error(ex);
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
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
            catch (EntityException ex)
            {
                logger.Error(ex);
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
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
            catch (EntityException ex)
            {
                logger.Error(ex);
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
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
            catch (EntityException ex)
            {
                logger.Error(ex);
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
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
            catch (EntityException ex)
            {
                logger.Error(ex);
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
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
            catch (EntityException ex)
            {
                logger.Error(ex);
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public List<Friendship> GetAllFriendshipsAndRequests(int playerId)
        {
            try
            {
                var friendships = friendsRepository.GetAllFriendshipsAndRequests(playerId);
                return friendships;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }
    }
}