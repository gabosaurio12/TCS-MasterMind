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
using System.Linq;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class LobbyService : ILobbyService
    {
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, ILobbyCallbackService>>
            roomClientsByRoom = new ConcurrentDictionary<int, ConcurrentDictionary<string, ILobbyCallbackService>>();
        private readonly ILog logger = LogManager.GetLogger(typeof(LobbyService));
        private readonly ErrorMessage clientMessage = new ErrorMessage();
        private readonly ILobbyRepository lobbyRepository = new LobbyRepository();
        private readonly IRoomRepository roomRepository = new RoomRepository();

        public string SendMessage(ChatMessage message)
        {
            try
            {
                return SendMessageInternal(message);
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
                return ChangePlayerStatusInternal(status);
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

        private string SendMessageInternal(ChatMessage message)
        {
            var result = lobbyRepository.SendMessage(message);
            if (!IsMessageAccepted(result.Status))
            {
                return result.Status;
            }

            RegisterClient(message.RoomId, message.SenderUsername);
            var outgoingMessage = BuildOutgoingMessage(message, result.Message);
            NotifyClients(
                message.RoomId,
                message.SenderUsername,
                callback => callback.OnMessageReceivedCallback(outgoingMessage));
            return result.Status;
        }

        private string ChangePlayerStatusInternal(PlayerStatus status)
        {
            var result = roomRepository.ChangePlayerStatus(status);
            if (result.Status != RoomResultEnum.Success.ToString())
            {
                return result.Status;
            }

            RegisterClient(status.RoomId, status.Username);
            NotifyStatusChanges(status, result);
            return result.Status;
        }

        private void NotifyStatusChanges(PlayerStatus status, PlayerStatusChangeResult result)
        {
            if (result.RoomDeleted)
            {
                RemoveRoom(status.RoomId);
                return;
            }

            if (result.PromotedPlayer != null)
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

        private void NotifyStatus(int roomId, string senderUsername, PlayerStatus status)
        {
            NotifyClients(
                roomId,
                senderUsername,
                callback => callback.OnChangePlayerStatusCallback(status));
        }

        private bool IsMessageAccepted(string status)
        {
            return status == LobbyResultEnum.MessageSent.ToString()
                || status == LobbyResultEnum.InappropriateVocabulary.ToString();
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

        private void RegisterClient(int roomId, string username)
        {
            if (roomId <= 0 || string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            var callback = OperationContext.Current?.GetCallbackChannel<ILobbyCallbackService>();
            if (callback == null)
            {
                return;
            }

            var clients = roomClientsByRoom.GetOrAdd(
                roomId,
                id => new ConcurrentDictionary<string, ILobbyCallbackService>(StringComparer.OrdinalIgnoreCase));
            clients[username.Trim()] = callback;
        }

        private void NotifyClients(int roomId, string senderUsername, Action<ILobbyCallbackService> notify)
        {
            ConcurrentDictionary<string, ILobbyCallbackService> clients;
            if (!roomClientsByRoom.TryGetValue(roomId, out clients))
            {
                return;
            }

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

        private void HandleDisconnectedClient(int roomId, string username, Exception exception)
        {
            logger.Warn("A lobby client disconnected.", exception);
            RemoveClient(roomId, username);
        }

        private void RemoveClient(int roomId, string username)
        {
            var key = username?.Trim();
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            ConcurrentDictionary<string, ILobbyCallbackService> clients;
            if (!roomClientsByRoom.TryGetValue(roomId, out clients))
            {
                return;
            }

            ILobbyCallbackService removedClient;
            clients.TryRemove(key, out removedClient);
            if (clients.IsEmpty)
            {
                RemoveRoom(roomId);
            }
        }

        private void RemoveRoom(int roomId)
        {
            ConcurrentDictionary<string, ILobbyCallbackService> removedRoom;
            roomClientsByRoom.TryRemove(roomId, out removedRoom);
        }

        private FaultException<ErrorMessage> BuildDatabaseFault()
        {
            clientMessage.Message = ErrorMessagesEnum.DataBaseError.ToString();
            return new FaultException<ErrorMessage>(clientMessage);
        }
    }
}
