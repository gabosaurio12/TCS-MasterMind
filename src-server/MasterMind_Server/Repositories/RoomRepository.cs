using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Dtos;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using PlayerEntity = MasterMind_Server.Data.Player;

namespace MasterMind_Server.Repositories
{
    internal class RoomRepository : IRoomRepository
    {
        private const int RoomCodeMinimum = 100000;
        private const int RoomCodeMaximum = 999999;
        private const int RoomCodeAttempts = 10;
        private const int MaxRoomNameLength = 100;
        private readonly ILog logger = LogManager.GetLogger(typeof(RoomRepository));
        private readonly MasterMindEntities context = new MasterMindEntities(true);

        public List<Room> GetPublicRooms()
        {
            try
            {
                var privacyId = FindPrivacyId(RoomPrivacyEnum.Public);
                if (!privacyId.HasValue)
                {
                    return new List<Room>();
                }

                var rooms = context.MatchRoom
                    .Include(room => room.Player)
                    .Include(room => room.Player1)
                    .Include(room => room.GameModesCatalog)
                    .Include(room => room.DifficultiesCatalog)
                    .Include(room => room.PrivacyCatalog)
                    .Where(room => room.room_privacy_id == privacyId.Value)
                    .ToList();

                return rooms.Select(RoomEntityToContract).ToList();
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public RoomCreationResult CreateRoom(Room room)
        {
            try
            {
                var data = BuildRoomCreationData(room);
                if (data == null)
                {
                    return CreateEmptyRoomResult(RoomResultEnum.Error);
                }
                return PersistRoom(data);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public string JoinRoom(Room room, string username)
        {
            try
            {
                return JoinRoomInternal(room, username);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public string JoinRoomWithCode(Room room, string username, string code)
        {
            try
            {
                return JoinRoomWithCodeInternal(room, username, code);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public Room GetRoomByName(string roomName)
        {
            try
            {
                var roomEntity = GetRoomEntityByName(roomName);
                return roomEntity == null
                    ? new Room { RoomId = 0 }
                    : RoomEntityToContract(roomEntity);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public PlayerStatusChangeResult ChangePlayerStatus(PlayerStatus status)
        {
            try
            {
                return ChangePlayerStatusInternal(status);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        private RoomCreationResult PersistRoom(RoomCreationData data)
        {
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                if (context.MatchRoom.Any(room => room.match_room_name == data.RoomName))
                {
                    return CreateEmptyRoomResult(RoomResultEnum.RoomNameTaken);
                }

                var roomEntity = BuildRoomEntity(data);
                if (data.Privacy == RoomPrivacyEnum.Private && roomEntity.room_code == null)
                {
                    return CreateEmptyRoomResult(RoomResultEnum.Error);
                }

                context.MatchRoom.Add(roomEntity);
                context.SaveChanges();
                transaction.Commit();

                return new RoomCreationResult
                {
                    Status = RoomResultEnum.Success.ToString(),
                    Room = GetRoomByName(data.RoomName)
                };
            }
        }

        private string JoinRoomInternal(Room room, string username)
        {
            if (room == null || string.IsNullOrWhiteSpace(room.Name))
            {
                return RoomResultEnum.Error.ToString();
            }

            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var roomEntity = GetRoomEntityByName(room.Name);
                var player = GetPlayer(username);
                if (roomEntity == null || player == null || !IsPublic(roomEntity))
                {
                    return RoomResultEnum.Error.ToString();
                }

                var blockingResult = GetJoinBlockingResult(roomEntity, player.player_id);
                if (blockingResult != null)
                {
                    return blockingResult;
                }

                SetPlayerTwo(roomEntity, player.player_id);
                context.SaveChanges();
                transaction.Commit();
                return RoomResultEnum.Success.ToString();
            }
        }

        private string JoinRoomWithCodeInternal(Room room, string username, string code)
        {
            if (room == null || string.IsNullOrWhiteSpace(room.Name))
            {
                return RoomResultEnum.Error.ToString();
            }
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var roomEntity = GetRoomEntityByName(room.Name);
                var player = GetPlayer(username);
                if (roomEntity == null || player == null || IsPublic(roomEntity))
                {
                    return RoomResultEnum.Error.ToString();
                }
                var blockingResult = GetJoinBlockingResult(roomEntity, player.player_id);
                if (blockingResult != null)
                {
                    return blockingResult;
                }
                if (!IsRoomCodeValid(roomEntity, code))
                {
                    return RoomResultEnum.WrongCode.ToString();
                }
                SetPlayerTwo(roomEntity, player.player_id);
                context.SaveChanges();
                transaction.Commit();
                return RoomResultEnum.Success.ToString();
            }
        }

        private PlayerStatusChangeResult ChangePlayerStatusInternal(PlayerStatus status)
        {
            var result = new PlayerStatusChangeResult { Status = RoomResultEnum.Error.ToString() };
            PlayerStatusEnum newStatus;
            if (status == null || !TryParsePlayerStatus(status.Status, out newStatus))
            {
                return result;
            }
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var room = GetRoomEntityById(status.RoomId);
                var member = GetRoomMember(room, status.Username);
                if (member == null)
                {
                    return result;
                }
                result.Status = RoomResultEnum.Success.ToString();
                result.ChangedPlayer = BuildPlayerStatus(room, member, newStatus);
                var changeContext = new StatusChangeContext
                {
                    Room = room,
                    Member = member,
                    NewStatus = newStatus,
                    Result = result
                };
                ApplyStatusChange(changeContext);
                transaction.Commit();
                return result;
            }
        }

        private void ApplyStatusChange(StatusChangeContext context)
        {
            if (context.NewStatus == PlayerStatusEnum.Escaped)
            {
                ApplyEscape(context);
                return;
            }

            SavePlayerStatus(context.Room, context.Member.IsPlayerOne, context.NewStatus);
        }

        private void ApplyEscape(StatusChangeContext changeContext)
        {
            changeContext.Result.EscapedPlayer = BuildPlayerStatus(
                changeContext.Room,
                changeContext.Member,
                PlayerStatusEnum.Escaped);

            if (changeContext.Member.IsPlayerOne && !changeContext.Room.player_two_id.HasValue)
            {
                DeleteRoom(changeContext.Room);
                changeContext.Result.RoomDeleted = true;
                return;
            }

            if (changeContext.Member.IsPlayerOne)
            {
                PromotePlayerTwo(changeContext.Room, changeContext.Result);
            }
            else
            {
                ClearPlayerTwo(changeContext.Room);
            }

            context.SaveChanges();
        }

        private void PromotePlayerTwo(MatchRoom room, PlayerStatusChangeResult result)
        {
            var promotedUsername = room.Player1.username;
            var promotedStatus = room.player_two_status;

            room.player_one_id = room.player_two_id.Value;
            room.player_one_status = promotedStatus;
            room.player_two_id = null;
            room.player_two_status = (byte)PlayerStatusEnum.NotReady;

            result.PromotedPlayer = new PlayerStatus
            {
                RoomId = room.match_room_id,
                Username = promotedUsername,
                Status = ((PlayerStatusEnum)promotedStatus).ToString(),
                IsPlayerOne = true
            };
        }

        private void ClearPlayerTwo(MatchRoom room)
        {
            room.player_two_id = null;
            room.player_two_status = (byte)PlayerStatusEnum.NotReady;
        }

        private void SavePlayerStatus(MatchRoom room, bool isPlayerOne, PlayerStatusEnum status)
        {
            if (isPlayerOne)
            {
                room.player_one_status = (byte)status;
            }
            else
            {
                room.player_two_status = (byte)status;
            }

            context.SaveChanges();
        }

        private void DeleteRoom(MatchRoom room)
        {
            var playerInMatchIds = GetPlayerInMatchIds(room.match_room_id);
            context.MatchRoom.Remove(room);
            context.SaveChanges();
            DeleteOrphanedPlayerInMatch(playerInMatchIds);
        }

        private List<int> GetPlayerInMatchIds(int roomId)
        {
            var results = context.MatchResults
                .Where(result => result.match_room_id == roomId)
                .Select(result => new { result.player_one_results_id, result.player_two_results_id })
                .ToList();

            return results
                .SelectMany(result => new[] { result.player_one_results_id, result.player_two_results_id })
                .Distinct()
                .ToList();
        }

        private void DeleteOrphanedPlayerInMatch(List<int> playerInMatchIds)
        {
            if (playerInMatchIds.Count == 0)
            {
                return;
            }

            var referencedIds = GetReferencedPlayerInMatchIds(playerInMatchIds);
            var orphanedIds = playerInMatchIds.Except(referencedIds).ToList();
            if (orphanedIds.Count == 0)
            {
                return;
            }

            var orphanedPlayers = context.PlayerInMatch
                .Where(player => orphanedIds.Contains(player.player_in_match_id));
            context.PlayerInMatch.RemoveRange(orphanedPlayers);
            context.SaveChanges();
        }

        private List<int> GetReferencedPlayerInMatchIds(List<int> playerInMatchIds)
        {
            var results = context.MatchResults
                .Where(result => playerInMatchIds.Contains(result.player_one_results_id)
                    || playerInMatchIds.Contains(result.player_two_results_id))
                .Select(result => new { result.player_one_results_id, result.player_two_results_id })
                .ToList();

            return results
                .SelectMany(result => new[] { result.player_one_results_id, result.player_two_results_id })
                .Distinct()
                .ToList();
        }

        private RoomCreationData BuildRoomCreationData(Room room)
        {
            if (room == null || string.IsNullOrWhiteSpace(room.Name))
            {
                return null;
            }
            var roomName = room.Name.Trim();
            var privacy = ParsePrivacy(room.RoomPrivacy);
            var privacyId = privacy.HasValue ? FindPrivacyId(privacy.Value) : null;
            var gamemodeId = FindGameModeId(room.Gamemode);
            var difficultyId = FindDifficultyId(room.Difficulty);
            var player = GetPlayer(room.PlayerOneUsername);
            if (roomName.Length > MaxRoomNameLength
                || !privacyId.HasValue
                || !gamemodeId.HasValue
                || !difficultyId.HasValue
                || player == null)
            {
                return null;
            }
            return new RoomCreationData
            {
                RoomName = roomName,
                Privacy = privacy.Value,
                PrivacyId = privacyId.Value,
                GamemodeId = gamemodeId.Value,
                DifficultyId = difficultyId.Value,
                PlayerOneId = player.player_id
            };
        }

        private MatchRoom BuildRoomEntity(RoomCreationData data)
        {
            return new MatchRoom
            {
                match_room_name = data.RoomName,
                gamemode_id = data.GamemodeId,
                difficulty_id = data.DifficultyId,
                player_one_id = data.PlayerOneId,
                player_one_status = (byte)PlayerStatusEnum.NotReady,
                player_two_status = (byte)PlayerStatusEnum.NotReady,
                room_privacy_id = data.PrivacyId,
                room_code = data.Privacy == RoomPrivacyEnum.Private ? GenerateRoomCode() : null
            };
        }

        private string GetJoinBlockingResult(MatchRoom room, int playerId)
        {
            if (IsMember(room, playerId))
            {
                return RoomResultEnum.Success.ToString();
            }

            if (room.player_two_id.HasValue)
            {
                return RoomResultEnum.RoomIsFull.ToString();
            }

            return null;
        }

        private void SetPlayerTwo(MatchRoom room, int playerId)
        {
            room.player_two_id = playerId;
            room.player_two_status = (byte)PlayerStatusEnum.NotReady;
        }

        private bool IsRoomCodeValid(MatchRoom room, string code)
        {
            var enteredCode = code?.Trim();
            return !string.IsNullOrEmpty(enteredCode)
                && string.Equals(enteredCode, room.room_code, StringComparison.Ordinal);
        }

        private bool IsPublic(MatchRoom room)
        {
            return room.PrivacyCatalog != null
                && room.PrivacyCatalog.privacy == RoomPrivacyEnum.Public.ToString();
        }

        private bool IsMember(MatchRoom room, int playerId)
        {
            return room.player_one_id == playerId || room.player_two_id == playerId;
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

        private PlayerStatus BuildPlayerStatus(MatchRoom room, RoomMember member, PlayerStatusEnum status)
        {
            return new PlayerStatus
            {
                RoomId = room.match_room_id,
                Username = member.Username,
                Status = status.ToString(),
                IsPlayerOne = member.IsPlayerOne
            };
        }

        private bool TryParsePlayerStatus(string status, out PlayerStatusEnum result)
        {
            result = PlayerStatusEnum.NotReady;
            return !string.IsNullOrWhiteSpace(status)
                && Enum.TryParse(status.Trim(), true, out result)
                && Enum.IsDefined(typeof(PlayerStatusEnum), result);
        }

        private RoomPrivacyEnum? ParsePrivacy(string privacy)
        {
            RoomPrivacyEnum result;
            if (!string.IsNullOrWhiteSpace(privacy)
                && Enum.TryParse(privacy.Trim(), true, out result)
                && Enum.IsDefined(typeof(RoomPrivacyEnum), result))
            {
                return result;
            }

            return null;
        }

        private int? FindPrivacyId(RoomPrivacyEnum privacy)
        {
            var catalog = context.PrivacyCatalog
                .FirstOrDefault(entity => entity.privacy == privacy.ToString());
            return catalog?.privacy_id;
        }

        private int? FindGameModeId(string gamemode)
        {
            if (string.IsNullOrWhiteSpace(gamemode))
            {
                return null;
            }

            var catalog = context.GameModesCatalog
                .FirstOrDefault(entity => entity.gamemode == gamemode.Trim());
            return catalog?.gamemode_id;
        }

        private int? FindDifficultyId(string difficulty)
        {
            if (string.IsNullOrWhiteSpace(difficulty))
            {
                return null;
            }

            var catalog = context.DifficultiesCatalog
                .FirstOrDefault(entity => entity.difficulty == difficulty.Trim());
            return catalog?.difficulty_id;
        }

        private PlayerEntity GetPlayer(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var playerName = username.Trim();
            return context.Player.FirstOrDefault(player => player.username == playerName);
        }

        private MatchRoom GetRoomEntityByName(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
            {
                return null;
            }

            var name = roomName.Trim();
            return context.MatchRoom
                .Include(room => room.Player)
                .Include(room => room.Player1)
                .Include(room => room.GameModesCatalog)
                .Include(room => room.DifficultiesCatalog)
                .Include(room => room.PrivacyCatalog)
                .FirstOrDefault(room => room.match_room_name == name);
        }

        private MatchRoom GetRoomEntityById(int roomId)
        {
            return context.MatchRoom
                .Include(room => room.Player)
                .Include(room => room.Player1)
                .FirstOrDefault(room => room.match_room_id == roomId);
        }

        private Room RoomEntityToContract(MatchRoom room)
        {
            return new Room
            {
                RoomId = room.match_room_id,
                Name = room.match_room_name,
                Gamemode = room.GameModesCatalog?.gamemode,
                Difficulty = room.DifficultiesCatalog?.difficulty,
                PlayerOneUsername = room.Player?.username,
                PlayerTwoUsername = room.Player1?.username,
                RoomPrivacy = room.PrivacyCatalog?.privacy,
                RoomCode = room.room_code
            };
        }

        private RoomCreationResult CreateEmptyRoomResult(RoomResultEnum status)
        {
            return new RoomCreationResult
            {
                Status = status.ToString(),
                Room = new Room { RoomId = 0 }
            };
        }

        private string GenerateRoomCode()
        {
            for (var attempt = 0; attempt < RoomCodeAttempts; attempt++)
            {
                var code = BuildRandomRoomCode();
                if (!context.MatchRoom.Any(room => room.room_code == code))
                {
                    return code;
                }
            }

            return null;
        }

        private string BuildRandomRoomCode()
        {
            var bytes = new byte[2];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }

            var value = BitConverter.ToUInt16(bytes, 0);
            var range = RoomCodeMaximum - RoomCodeMinimum + 1;
            return (RoomCodeMinimum + (value % range)).ToString(CultureInfo.InvariantCulture);
        }

        private class RoomCreationData
        {
            public string RoomName { get; set; }

            public RoomPrivacyEnum Privacy { get; set; }

            public int PrivacyId { get; set; }

            public int GamemodeId { get; set; }

            public int DifficultyId { get; set; }

            public int PlayerOneId { get; set; }
        }

        private class RoomMember
        {
            public string Username { get; set; }

            public bool IsPlayerOne { get; set; }
        }

        private class StatusChangeContext
        {
            public MatchRoom Room { get; set; }

            public RoomMember Member { get; set; }

            public PlayerStatusEnum NewStatus { get; set; }

            public PlayerStatusChangeResult Result { get; set; }
        }
    }
}
