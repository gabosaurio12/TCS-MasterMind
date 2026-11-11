using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class FriendsService : IFriendsService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(FriendsService));

        private readonly ErrorMessage clientMessage = new ErrorMessage();
        private readonly FriendsRepository friendsRepository = new FriendsRepository();

        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, IFriendsCallbackService>>
            friendsClientsByModal = new ConcurrentDictionary<int,
                ConcurrentDictionary<string, IFriendsCallbackService>>();

        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, IFriendsCallbackService>>
            friendRequestsClientsByModal = new ConcurrentDictionary<int,
                ConcurrentDictionary<string, IFriendsCallbackService>>();

        public string AcceptFriendRequest(int requestId)
        {
            try
            {
                var result = friendsRepository.AcceptFriendRequest(requestId);
                var request = friendsRepository.GetFriendRequestById(requestId);

                if (request.FriendshipId > 0)
                {
                    var requester = new PlayerRepository().GetPlayerByUsername(request.RequesterUsername);

                    if (requester != null)
                    {
                        NotifySpecificClientInFriends(
                            requestId,
                            requester.username,
                            c => c.OnFriendshipStatusChanged());
                    }
                }

                return result;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
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
            catch (EntityException)
            {
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
            catch (EntityException)
            {
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
            catch (EntityException)
            {
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
            catch (EntityException)
            {
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
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public string SendFriendRequest(int addresseeId, int requesterId)
        {
            try
            {
                var result = friendsRepository.SendFriendRequest(addresseeId, requesterId);

                if (result.Equals(RepositoriesMessages.Success.ToString()))
                {
                    var friendship = friendsRepository.GetFriendRequest(addresseeId, requesterId);
                    var addressee = new PlayerRepository().GetPlayerById(addresseeId);

                    if (friendship != null)
                    {
                        NotifySpecificClientInRequests(
                            friendship.FriendshipId,
                            addressee.username,
                            c => c.OnFriendshipStatusChanged());
                    }
                }

                return result;
            }
            catch (EntityException)
            {
                clientMessage.Message = ErrorMessages.DataBaseError.ToString();
                throw new FaultException<ErrorMessage>(clientMessage);
            }
        }

        public void RegisterFriendsClient(int friendshipId, string username)
        {
            if (friendshipId > 0 && !string.IsNullOrWhiteSpace(username))
            {
                var callback = OperationContext.Current?.GetCallbackChannel<IFriendsCallbackService>();
                if (callback != null)
                {
                    var clients = friendsClientsByModal.GetOrAdd(
                        friendshipId,
                        id => new ConcurrentDictionary<string,
                            IFriendsCallbackService>(StringComparer.OrdinalIgnoreCase));
                    clients[username.Trim()] = callback;
                }
            }
        }

        public void RegisterFriendRequestsClient(int friendshipId, string username)
        {
            if (friendshipId > 0 && !string.IsNullOrWhiteSpace(username))
            {
                var callback = OperationContext.Current?.GetCallbackChannel<IFriendsCallbackService>();
                if (callback != null)
                {
                    var clients = friendRequestsClientsByModal.GetOrAdd(
                        friendshipId,
                        id => new ConcurrentDictionary<string,
                            IFriendsCallbackService>(StringComparer.OrdinalIgnoreCase));
                    clients[username.Trim()] = callback;
                }
            }
        }

        private static void NotifySpecificClientInFriends(int friendshipId, string targetUsername,
            Action<IFriendsCallbackService> notify)
        {
            ConcurrentDictionary<string, IFriendsCallbackService> clients;

            if (friendsClientsByModal.TryGetValue(friendshipId, out clients))
            {
                if (clients.TryGetValue(targetUsername, out var callback))
                {
                    try
                    {
                        notify(callback);
                    }
                    catch (CommunicationException ex)
                    {
                        HandleDisconnectedFriendsClient(friendshipId, targetUsername, ex);
                    }
                    catch (TimeoutException ex)
                    {
                        HandleDisconnectedFriendsClient(friendshipId, targetUsername, ex);
                    }
                }
            }
        }

        private static void NotifySpecificClientInRequests(int friendshipId, string targetUsername,
            Action<IFriendsCallbackService> notify)
        {
            ConcurrentDictionary<string, IFriendsCallbackService> clients;

            if (friendRequestsClientsByModal.TryGetValue(friendshipId, out clients))
            {
                if (clients.TryGetValue(targetUsername, out var callback))
                {
                    try
                    {
                        notify(callback);
                    }
                    catch (CommunicationException ex)
                    {
                        HandleDisconnectedFriendRequestsClient(friendshipId, targetUsername, ex);
                    }
                    catch (TimeoutException ex)
                    {
                        HandleDisconnectedFriendRequestsClient(friendshipId, targetUsername, ex);
                    }
                }
            }
        }

        private static void HandleDisconnectedFriendsClient(int friendshipId, string username, Exception exception)
        {
            logger.Warn(exception);
            RemoveFriendsClient(friendshipId, username);
        }

        private static void HandleDisconnectedFriendRequestsClient(int friendshipId, string username, Exception exception)
        {
            logger.Warn(exception);
            RemoveFriendRequestsClient(friendshipId, username);
        }

        private static void RemoveFriendsClient(int friendshipId, string username)
        {
            var key = username?.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                ConcurrentDictionary<string, IFriendsCallbackService> clients;
                if (friendsClientsByModal.TryGetValue(friendshipId, out clients))
                {
                    IFriendsCallbackService removedClient;
                    clients.TryRemove(key, out removedClient);
                    if (clients.IsEmpty)
                    {
                        RemoveFriendshipFromFriends(friendshipId);
                    }
                }
            }
        }

        private static void RemoveFriendRequestsClient(int friendshipId, string username)
        {
            var key = username?.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                ConcurrentDictionary<string, IFriendsCallbackService> clients;
                if (friendRequestsClientsByModal.TryGetValue(friendshipId, out clients))
                {
                    IFriendsCallbackService removedClient;
                    clients.TryRemove(key, out removedClient);
                    if (clients.IsEmpty)
                    {
                        RemoveFriendshipFromFriendRequests(friendshipId);
                    }
                }
            }
        }

        private static void RemoveFriendshipFromFriends(int friendshipId)
        {
            ConcurrentDictionary<string, IFriendsCallbackService> removedFriendship;
            friendsClientsByModal.TryRemove(friendshipId, out removedFriendship);
        }

        private static void RemoveFriendshipFromFriendRequests(int friendshipId)
        {
            ConcurrentDictionary<string, IFriendsCallbackService> removedFriendship;
            friendRequestsClientsByModal.TryRemove(friendshipId, out removedFriendship);
        }
    }
}
