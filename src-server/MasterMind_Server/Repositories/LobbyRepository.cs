using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Dtos;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;
using MasterMind_Server.Services.ChatHelpers;
using System;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;

namespace MasterMind_Server.Repositories
{
    internal class LobbyRepository : ILobbyRepository
    {
        private const int MaxMessageLength = 200;
        private readonly MasterMindEntities context = new MasterMindEntities(true);
        private readonly IPlayerRepository playerRepository = new PlayerRepository();

        public ChatMessageResult SendMessage(ChatMessage message)
        {
            if (!IsMessageValid(message))
            {
                return CreateResult(LobbyResult.Error, string.Empty);
            }

            var room = GetRoomEntity(message.RoomId);
            var member = GetRoomMember(room, message.SenderUsername);
            if (member == null)
            {
                return CreateResult(LobbyResult.Error, string.Empty);
            }

            var filtered = ProfanityFilter.Censor(message.Message.Trim());
            if (!filtered.HasBannedWords)
            {
                return CreateResult(LobbyResult.MessageSent, filtered.CensoredText);
            }

            ReportOffensiveMessage(room, member);
            return CreateResult(LobbyResult.InappropriateVocabulary, filtered.CensoredText);
        }

        private static bool IsMessageValid(ChatMessage message)
        {
            return message != null
                && message.RoomId > 0
                && !string.IsNullOrWhiteSpace(message.SenderUsername)
                && !string.IsNullOrWhiteSpace(message.Message)
                && message.Message.Trim().Length <= MaxMessageLength;
        }

        private void ReportOffensiveMessage(MatchRoom room, RoomMemberDto member)
        {
            var opponentUsername = GetOtherMemberUsername(room, member);

            if (opponentUsername == null)
            {
                return;
            }

            var report = new ReportDto
            {
                ReportedPlayerUsername = member.Username,
                ReportedByPlayerUsername = opponentUsername,
                Reason = ReportReason.InappropriateLanguage.ToString()
            };

            playerRepository.ReportPlayer(report);
        }

        private static string GetOtherMemberUsername(MatchRoom room, RoomMemberDto member)
        {
            if (!member.IsPlayerOne)
            {
                return room.Player?.username;
            }

            return room.Player1?.username;
        }

        private RoomMemberDto GetRoomMember(MatchRoom room, string username)
        {
            if (room == null || string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var playerName = username.Trim();
            if (string.Equals(room.Player?.username, playerName, StringComparison.OrdinalIgnoreCase))
            {
                return new RoomMemberDto { Username = room.Player.username, IsPlayerOne = true };
            }

            if (room.Player1 != null
                && string.Equals(room.Player1.username, playerName, StringComparison.OrdinalIgnoreCase))
            {
                return new RoomMemberDto { Username = room.Player1.username, IsPlayerOne = false };
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

        private ChatMessageResult CreateResult(LobbyResult status, string message)
        {
            return new ChatMessageResult
            {
                Status = status.ToString(),
                Message = message
            };
        }
    }
}
