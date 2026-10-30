using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Dtos;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;
using System;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Text.RegularExpressions;

namespace MasterMind_Server.Repositories
{
    internal class LobbyRepository : ILobbyRepository
    {
        private const int MaxMessageLength = 200;
        private const char CensorshipCharacter = '*';
        private static readonly string[] bannedWords = { "pendejo", "mierda", "fuck", "shit" };
        private static readonly Regex bannedWordsRegex = BuildBannedWordsRegex();
        private readonly ILog logger = LogManager.GetLogger(typeof(LobbyRepository));
        private readonly MasterMindEntities context = new MasterMindEntities(true);
        private readonly IPlayerRepository playerRepository = new PlayerRepository();

        public ChatMessageResult SendMessage(ChatMessage message)
        {
            try
            {
                return SendMessageInternal(message);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        private ChatMessageResult SendMessageInternal(ChatMessage message)
        {
            if (!IsMessageValid(message))
            {
                return CreateResult(LobbyResultEnum.Error, null);
            }

            var room = GetRoomEntity(message.RoomId);
            var member = GetRoomMember(room, message.SenderUsername);
            if (member == null)
            {
                return CreateResult(LobbyResultEnum.Error, null);
            }

            bool hasBannedWords;
            var content = message.Message.Trim();
            var censoredMessage = CensorMessage(content, out hasBannedWords);
            if (hasBannedWords)
            {
                ReportOffensiveMessage(room, member);
            }

            var status = hasBannedWords ? LobbyResultEnum.InappropriateVocabulary : LobbyResultEnum.MessageSent;
            return CreateResult(status, censoredMessage);
        }

        private bool IsMessageValid(ChatMessage message)
        {
            return message != null
                && message.RoomId > 0
                && !string.IsNullOrWhiteSpace(message.SenderUsername)
                && !string.IsNullOrWhiteSpace(message.Message)
                && message.Message.Trim().Length <= MaxMessageLength;
        }

        private string CensorMessage(string message, out bool hasBannedWords)
        {
            var censoredMessage = bannedWordsRegex.Replace(
                message,
                match => new string(CensorshipCharacter, match.Length));
            hasBannedWords = !string.Equals(censoredMessage, message, StringComparison.Ordinal);
            return censoredMessage;
        }

        private void ReportOffensiveMessage(MatchRoom room, RoomMember member)
        {
            var report = new ReportDto
            {
                ReportedPlayerUsername = member.Username,
                ReportedByPlayerUsername = GetOtherMemberUsername(room, member),
                Reason = ReportReasonEnum.InappropriateLanguage.ToString()
            };

            playerRepository.ReportPlayer(report);
        }

        private string GetOtherMemberUsername(MatchRoom room, RoomMember member)
        {
            if (!member.IsPlayerOne)
            {
                return room.Player?.username;
            }

            return room.Player1?.username ?? member.Username;
        }

        private RoomMember GetRoomMember(MatchRoom room, string username)
        {
            if (room == null || string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var playerName = username.Trim();
            if (string.Equals(room.Player?.username, playerName, StringComparison.OrdinalIgnoreCase))
            {
                return new RoomMember { Username = room.Player.username, IsPlayerOne = true };
            }

            if (room.Player1 != null
                && string.Equals(room.Player1.username, playerName, StringComparison.OrdinalIgnoreCase))
            {
                return new RoomMember { Username = room.Player1.username, IsPlayerOne = false };
            }

            return null;
        }

        private MatchRoom GetRoomEntity(int roomId)
        {
            return context.MatchRoom
                .Include(room => room.Player)
                .Include(room => room.Player1)
                .FirstOrDefault(room => room.match_room_id == roomId);
        }

        private ChatMessageResult CreateResult(LobbyResultEnum status, string message)
        {
            return new ChatMessageResult
            {
                Status = status.ToString(),
                Message = message
            };
        }

        private static Regex BuildBannedWordsRegex()
        {
            var pattern = string.Join("|", bannedWords.Select(Regex.Escape));
            return new Regex(
                $@"\b({pattern})\b",
                RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        private class RoomMember
        {
            public string Username { get; set; }

            public bool IsPlayerOne { get; set; }
        }
    }
}
