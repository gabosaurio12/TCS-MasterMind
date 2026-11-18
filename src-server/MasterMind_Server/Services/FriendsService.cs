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
        private readonly PlayerRepository playerRepository = new PlayerRepository();

        public string AcceptFriendRequest(int requestId)
        {
            try
            {
                var result = friendsRepository.AcceptFriendRequest(requestId);

                if (IsSuccess(result))
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

        private static bool IsSuccess(string result)
        {
            return RepositoriesMessages.Success.ToString().Equals(result, StringComparison.OrdinalIgnoreCase);
        }

        private static void NotifyFriendshipStatusChanged(string username)
        {
            if (string.IsNullOrEmpty(username) || !friendsClientsByModal.TryGetValue(username, out var callback))
            {
                return;
            }

            try
            {
                callback.OnFriendshipStatusChanged();
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException
                || ex is ObjectDisposedException)
            {
                logger.WarnFormat("Callback for {0} failed: {1}", username, ex.Message);
                RemoveSubscription(username, callback);
            }
        }

        private static void RemoveSubscription(string username, IFriendsCallbackService callback)
        {
            ((ICollection<KeyValuePair<string, IFriendsCallbackService>>)friendsClientsByModal)
                .Remove(new KeyValuePair<string, IFriendsCallbackService>(username, callback));
        }

        public void Subscribe(string username)
        {
            var callback = OperationContext.Current?.GetCallbackChannel<IFriendsCallbackService>();
            if (callback == null || string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            friendsClientsByModal[username] = callback;
            var channel = OperationContext.Current.Channel;
            channel.Closed += (sender, e) => RemoveSubscription(username, callback);
            channel.Faulted += (sender, e) => RemoveSubscription(username, callback);
        }

        public void Unsubscribe(string username)
        {
            var callback = OperationContext.Current?.GetCallbackChannel<IFriendsCallbackService>();
            if (callback != null && !string.IsNullOrWhiteSpace(username))
            {
                RemoveSubscription(username, callback);
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

                if (IsSuccess(result))
                {
                    NotifyFriendshipStatusChanged(addresseeUsername);
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

        public string SendFriendRequest(int addresseeId, int requesterId)
        {
            try
            {
                var result = friendsRepository.SendFriendRequest(addresseeId, requesterId);

                if (IsSuccess(result))
                {
                    NotifyFriendshipStatusChanged(playerRepository.GetPlayerById(addresseeId)?.username);
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