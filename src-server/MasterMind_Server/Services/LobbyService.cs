using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.Dtos;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class LobbyService : ILobbyService
    {
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, ILobbyCallbackService>>
            roomClientsByRoom = new ConcurrentDictionary<int, ConcurrentDictionary<string, ILobbyCallbackService>>();
        private static readonly ILog logger = LogManager.GetLogger(typeof(LobbyService));
        private readonly ErrorMessage clientMessage = new ErrorMessage();
        private readonly ILobbyRepository lobbyRepository = new LobbyRepository();
        private readonly IRoomRepository roomRepository = new RoomRepository();

        public string SendMessage(ChatMessage message)
        {
            try
            {
                var result = lobbyRepository.SendMessage(message);
                if (!IsMessageAccepted(result.Status))
                {
                    return result.Status;
                }

                RegisterClient(message.RoomId, message.SenderUsername);

                var outgoingMessage = BuildOutgoingMessage(message, result.Message);
                Action<ILobbyCallbackService> sendMessageCallback =
                    callback => callback.OnMessageReceivedCallback(outgoingMessage);

                NotifyClients(
                    message.RoomId,
                    message.SenderUsername,
                    sendMessageCallback);

                return result.Status;
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

        public string ChangePlayerStatus(PlayerStatus status)
        {
            try
            {
                var result = roomRepository.ChangePlayerStatus(status);
                if (result.Status != RoomResult.Success.ToString())
                {
                    return result.Status;
                }

                RegisterClient(status.RoomId, status.Username);
                NotifyStatusChanges(status, result);

                return result.Status;
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

        private static void NotifyStatusChanges(PlayerStatus status, PlayerStatusChangeResult result)
        {
            if (result.RoomDeleted)
            {
                RemoveRoom(status.RoomId);
            }
            else
            {
                if (result.JoinedPlayer != null)
                {
                    NotifyStatus(status.RoomId, status.Username, result.JoinedPlayer);
                }
                else if (result.PromotedPlayer != null)
                {
                    NotifyStatus(status.RoomId, status.Username, result.PromotedPlayer);
                }
                else if (result.EscapedPlayer != null)
                {
                    NotifyStatus(status.RoomId, status.Username, result.EscapedPlayer);
                }
                else if (result.ChangedPlayer != null)
                {
                    NotifyStatus(status.RoomId, status.Username, result.ChangedPlayer);
                }

                if (result.EscapedPlayer != null)
                {
                    RemoveClient(status.RoomId, status.Username);
                }
            }
        }

        private static void NotifyStatus(int roomId, string senderUsername, PlayerStatus status)
        {
            Action<ILobbyCallbackService> notifyStatusCallback =
                    callback => callback.OnChangePlayerStatusCallback(status);

            NotifyClients(
                roomId,
                senderUsername,
                notifyStatusCallback);
        }

        private static bool IsMessageAccepted(string status)
        {
            return status == LobbyResult.MessageSent.ToString()
                || status == LobbyResult.InappropriateVocabulary.ToString();
        }

        private ChatMessage BuildOutgoingMessage(ChatMessage message, string content)
        {
            return new ChatMessage
            {
                RoomId = message.RoomId,
                SenderUsername = message.SenderUsername,
                Message = content
            };
        }

        private static void RegisterClient(int roomId, string username)
        {
            if (roomId > 0 && !string.IsNullOrWhiteSpace(username))
            {
                var callback = OperationContext.Current?.GetCallbackChannel<ILobbyCallbackService>();
                if (callback != null)
                {
                    var newClients = new ConcurrentDictionary<string, ILobbyCallbackService>(StringComparer.OrdinalIgnoreCase);

                    var clients = roomClientsByRoom.GetOrAdd(
                        roomId,
                        id => newClients);
                    clients[username.Trim()] = callback;
                }
            }
        }

        private static void NotifyClients(int roomId, string senderUsername, Action<ILobbyCallbackService> notify)
        {
            ConcurrentDictionary<string, ILobbyCallbackService> clients;

            if (roomClientsByRoom.TryGetValue(roomId, out clients))
            {
                var sender = senderUsername?.Trim();
                foreach (var client in clients.ToArray())
                {
                    if (string.Equals(client.Key, sender, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        notify(client.Value);
                    }
                    catch (CommunicationException ex)
                    {
                        HandleDisconnectedClient(roomId, client.Key, ex);
                    }
                    catch (TimeoutException ex)
                    {
                        HandleDisconnectedClient(roomId, client.Key, ex);
                    }
                }
            }
        }

        private static void HandleDisconnectedClient(int roomId, string username, Exception exception)
        {
            logger.Warn("A lobby client disconnected.", exception);
            RemoveClient(roomId, username);
        }

        private static void RemoveClient(int roomId, string username)
        {
            var key = username?.Trim();
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            ConcurrentDictionary<string, ILobbyCallbackService> clients;
            if (roomClientsByRoom.TryGetValue(roomId, out clients))
            {
                ILobbyCallbackService removedClient;
                clients.TryRemove(key, out removedClient);
                if (clients.IsEmpty)
                {
                    RemoveRoom(roomId);
                }
            }
        }

        private static void RemoveRoom(int roomId)
        {
            ConcurrentDictionary<string, ILobbyCallbackService> removedRoom;
            roomClientsByRoom.TryRemove(roomId, out removedRoom);
        }

        private FaultException<ErrorMessage> BuildDatabaseFault()
        {
            clientMessage.Message = ErrorMessages.DataBaseError.ToString();
            return new FaultException<ErrorMessage>(clientMessage);
        }
    }
}
