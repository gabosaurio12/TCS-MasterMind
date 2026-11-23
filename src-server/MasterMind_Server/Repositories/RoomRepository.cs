using MasterMind_Server.Contracts;
using MasterMind_Server.Data;
using MasterMind_Server.Repositories.ClassHelpers;
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

namespace MasterMind_Server.Repositories
{
    internal class RoomRepository : IRoomRepository
    {
        private const int RoomCodeMinimum = 100000;
        private const int RoomCodeMaximum = 999999;
        private const int RoomCodeAttempts = 10;
        private const int MaxRoomNameLength = 100;
        private readonly PlayerRepository playerRepository = new PlayerRepository();

        public List<Room> GetPublicRooms()
        {
            using (var dbContext = new MasterMindEntities(true))
            {
                var privacyId = FindPrivacyId(dbContext, RoomPrivacy.Public);
                if (!privacyId.HasValue)
                {
                    return new List<Room>();
                }

                var rooms = RoomsWithDetails(dbContext)
                    .AsNoTracking()
                    .Where(room => room.room_privacy_id == privacyId.Value)
                    .ToList();

                return rooms.Select(RoomEntityToContract).ToList();
            }
        }

        public RoomCreationResult CreateRoom(Room room)
        {
            RoomCreationData data;
            using (var dbContext = new MasterMindEntities(true))
            {
                data = BuildRoomCreationData(dbContext, room);
            }

            if (data == null)
            {
                return CreateEmptyRoomResult(RoomResult.Error);
            }
            return PersistRoom(data);
        }

        public string JoinRoom(Room room, string username)
        {
            if (room == null || string.IsNullOrWhiteSpace(room.Name))
            {
                return RoomResult.Error.ToString();
            }

            using (var dbContext = new MasterMindEntities(true))
            using (var transaction = dbContext.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var roomEntity = GetRoomEntityByName(dbContext, room.Name, true);
                var player = playerRepository.GetPlayerByUsername(username);
                if (roomEntity == null || player == null || !IsPublic(roomEntity))
                {
                    return RoomResult.Error.ToString();
                }

                var blockingResult = GetJoinBlockingResult(roomEntity, player.player_id);
                if (blockingResult != null)
                {
                    return blockingResult;
                }

                SetPlayerTwo(roomEntity, player.player_id);
                dbContext.SaveChanges();
                transaction.Commit();
                return RoomResult.Success.ToString();
            }
        }

        public string JoinRoomWithCode(Room room, string username, string code)
        {
            if (room == null || string.IsNullOrWhiteSpace(room.Name))
            {
                return RoomResult.Error.ToString();
            }
            using (var dbContext = new MasterMindEntities(true))
            using (var transaction = dbContext.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var roomEntity = GetRoomEntityByName(dbContext, room.Name, true);
                var player = playerRepository.GetPlayerByUsername(username);
                if (roomEntity == null || player == null || IsPublic(roomEntity))
                {
                    return RoomResult.Error.ToString();
                }
                var blockingResult = GetJoinBlockingResult(roomEntity, player.player_id);
                if (blockingResult != null)
                {
                    return blockingResult;
                }
                if (!IsRoomCodeValid(roomEntity, code))
                {
                    return RoomResult.WrongCode.ToString();
                }
                SetPlayerTwo(roomEntity, player.player_id);
                dbContext.SaveChanges();
                transaction.Commit();
                return RoomResult.Success.ToString();
            }
        }

        public Room GetRoomByName(string roomName)
        {
            using (var dbContext = new MasterMindEntities(true))
            {
                var roomEntity = GetRoomEntityByName(dbContext, roomName, false);
                return roomEntity == null
                    ? new Room { RoomId = 0 }
                    : RoomEntityToContract(roomEntity);
            }
        }

        public Room GetRoomById(int roomId)
        {
            using (var dbContext = new MasterMindEntities(true))
            {
                var roomEntity = GetRoomEntityById(dbContext, roomId, false);
                return roomEntity == null
                    ? new Room { RoomId = 0 }
                    : RoomEntityToContract(roomEntity);
            }
        }

        public Room GetRoomByCode(string code)
        {
            using (var dbContext = new MasterMindEntities(true))
            {
                var roomEntity = GetRoomEntityByCode(dbContext, code);
                return roomEntity == null
                    ? new Room { RoomId = 0 }
                    : RoomEntityToContract(roomEntity);
            }
        }

        public PlayerStatusChangeResult ChangePlayerStatus(PlayerStatus status)
        {
            var result = new PlayerStatusChangeResult { Status = RoomResult.Error.ToString() };
            PlayerPossibleStatus newStatus;
            if (status == null || !TryParsePlayerStatus(status.Status, out newStatus))
            {
                return result;
            }

            using (var dbContext = new MasterMindEntities(true))
            using (var transaction = dbContext.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var room = GetRoomEntityById(dbContext, status.RoomId, true);
                var member = GetRoomMember(room, status.Username);
                if (member == null)
                {
                    return result;
                }

                result.Status = RoomResult.Success.ToString();
                result.ChangedPlayer = BuildPlayerStatus(room, member, newStatus);
                var changeContext = new StatusChangeContext
                {
                    Db = dbContext,
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

        private RoomCreationResult PersistRoom(RoomCreationData data)
        {
            using (var dbContext = new MasterMindEntities(true))
            using (var transaction = dbContext.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                if (dbContext.MatchRoom.Any(room => room.match_room_name == data.RoomName))
                {
                    return CreateEmptyRoomResult(RoomResult.RoomNameTaken);
                }

                var roomEntity = BuildRoomEntity(dbContext, data);
                if (data.Privacy == RoomPrivacy.Private && roomEntity.room_code == null)
                {
                    return CreateEmptyRoomResult(RoomResult.Error);
                }

                dbContext.MatchRoom.Add(roomEntity);
                dbContext.SaveChanges();
                transaction.Commit();

                return new RoomCreationResult
                {
                    Status = RoomResult.Success.ToString(),
                    Room = GetRoomByName(data.RoomName)
                };
            }
        }

        private void ApplyStatusChange(StatusChangeContext context)
        {
            if (context.NewStatus == PlayerPossibleStatus.Escaped)
            {
                ApplyEscape(context);
            }
            else
            {
                SavePlayerStatus(context);
            }
        }

        private void ApplyEscape(StatusChangeContext changeContext)
        {
            changeContext.Result.EscapedPlayer = BuildPlayerStatus(
                changeContext.Room,
                changeContext.Member,
                PlayerPossibleStatus.Escaped);

            if (changeContext.Member.IsPlayerOne && !changeContext.Room.player_two_id.HasValue)
            {
                DeleteRoom(changeContext.Db, changeContext.Room);
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

            changeContext.Db.SaveChanges();
        }

        private void PromotePlayerTwo(MatchRoom room, PlayerStatusChangeResult result)
        {
            var promotedUsername = room.Player1.username;
            var promotedStatus = (byte)Enums.PlayerPossibleStatus.NotReady;

            room.player_one_id = room.player_two_id.Value;
            room.player_one_status = promotedStatus;
            room.player_two_id = null;
            room.player_two_status = (byte)Enums.PlayerPossibleStatus.NotReady;

            result.PromotedPlayer = new Contracts.PlayerStatus
            {
                RoomId = room.match_room_id,
                Username = promotedUsername,
                Status = ((Enums.PlayerPossibleStatus)promotedStatus).ToString(),
                IsPlayerOne = true
            };
        }

        private static void ClearPlayerTwo(MatchRoom room)
        {
            room.player_two_id = null;
            room.player_two_status = (byte)PlayerPossibleStatus.NotReady;
        }

        private static void SavePlayerStatus(StatusChangeContext changeContext)
        {
            var status = (byte)changeContext.NewStatus;
            if (changeContext.Member.IsPlayerOne)
            {
                changeContext.Room.player_one_status = status;
            }
            else
            {
                changeContext.Room.player_two_status = status;
            }

            changeContext.Db.SaveChanges();
        }

        private void DeleteRoom(MasterMindEntities dbContext, MatchRoom room)
        {
            var playerInMatchIds = GetPlayerByUsernameInMatchIds(dbContext, room.match_room_id);
            dbContext.MatchRoom.Remove(room);
            dbContext.SaveChanges();
            DeleteOrphanedPlayerInMatch(dbContext, playerInMatchIds);
        }

        private List<int> GetPlayerByUsernameInMatchIds(MasterMindEntities dbContext, int roomId)
        {
            var results = dbContext.MatchResults
                .Where(result => result.match_room_id == roomId)
                .Select(result => new { result.player_one_results_id, result.player_two_results_id })
                .ToList();

            return results
                .SelectMany(result => new[] { result.player_one_results_id, result.player_two_results_id })
                .Distinct()
                .ToList();
        }

        private void DeleteOrphanedPlayerInMatch(MasterMindEntities dbContext, List<int> playerInMatchIds)
        {
            if (playerInMatchIds.Count == 0)
            {
                return;
            }

            var referencedIds = GetReferencedPlayerInMatchIds(dbContext, playerInMatchIds);
            var orphanedIds = playerInMatchIds.Except(referencedIds).ToList();
            if (orphanedIds.Count == 0)
            {
                return;
            }

            var orphanedPlayers = dbContext.PlayerInMatch
                .Where(player => orphanedIds.Contains(player.player_in_match_id));
            dbContext.PlayerInMatch.RemoveRange(orphanedPlayers);
            dbContext.SaveChanges();
        }

        private List<int> GetReferencedPlayerInMatchIds(MasterMindEntities dbContext, List<int> playerInMatchIds)
        {
            var results = dbContext.MatchResults
                .Where(result => playerInMatchIds.Contains(result.player_one_results_id)
                    || playerInMatchIds.Contains(result.player_two_results_id))
                .Select(result => new { result.player_one_results_id, result.player_two_results_id })
                .ToList();

            return results
                .SelectMany(result => new[] { result.player_one_results_id, result.player_two_results_id })
                .Distinct()
                .ToList();
        }

        private RoomCreationData BuildRoomCreationData(MasterMindEntities dbContext, Room room)
        {
            if (room == null || string.IsNullOrWhiteSpace(room.Name))
            {
                return null;
            }

            var creationData = new RoomCreationData();

            var roomName = room.Name.Trim();
            var privacy = ParsePrivacy(room.RoomPrivacy);

            if (privacy == null)
            {
                return creationData;
            }

            int? privacyId = FindPrivacyId(dbContext, privacy.Value);

            var gamemodeId = FindGameModeId(dbContext, room.Gamemode);
            var difficultyId = FindDifficultyId(dbContext, room.Difficulty);
            var player = playerRepository.GetPlayerByUsername(room.PlayerOneUsername);

            creationData = new RoomCreationData
            {
                RoomName = roomName,
                Privacy = privacy.Value,
                PrivacyId = privacyId.Value,
                GamemodeId = gamemodeId.Value,
                DifficultyId = difficultyId.Value,
                PlayerOneId = player != null ? player.player_id : 0
            };

            if (ValidateRoomData(creationData))
            {
                return creationData;
            }

            return null;
        }

        private static bool ValidateRoomData(RoomCreationData data)
        {
            bool isValid = true;

            if (data.RoomName.Length > MaxRoomNameLength ||
                data.PrivacyId == 0 ||
                data.GamemodeId == 0 ||
                data.DifficultyId == 0 ||
                data.PlayerOneId == 0)
            {
                isValid = false;
            }

            return isValid;
        }

        private MatchRoom BuildRoomEntity(MasterMindEntities dbContext, RoomCreationData data)
        {
            return new MatchRoom
            {
                match_room_name = data.RoomName,
                gamemode_id = data.GamemodeId,
                difficulty_id = data.DifficultyId,
                player_one_id = data.PlayerOneId,
                player_one_status = (byte)Enums.PlayerPossibleStatus.NotReady,
                player_two_status = (byte)Enums.PlayerPossibleStatus.NotReady,
                room_privacy_id = data.PrivacyId,
                room_code = GenerateRoomCode(dbContext)
            };
        }

        private static string GetJoinBlockingResult(MatchRoom room, int playerId)
        {
            if (IsMember(room, playerId))
            {
                return RoomResult.Success.ToString();
            }

            if (room.player_two_id.HasValue)
            {
                return RoomResult.RoomIsFull.ToString();
            }

            return null;
        }

        private static void SetPlayerTwo(MatchRoom room, int playerId)
        {
            room.player_two_id = playerId;
            room.player_two_status = (byte)Enums.PlayerPossibleStatus.NotReady;
        }

        private static bool IsRoomCodeValid(MatchRoom room, string code)
        {
            var enteredCode = code?.Trim();
            return !string.IsNullOrEmpty(enteredCode)
                && string.Equals(enteredCode, room.room_code, StringComparison.Ordinal);
        }

        private static bool IsPublic(MatchRoom room)
        {
            return room.PrivacyCatalog != null
                && room.PrivacyCatalog.privacy == RoomPrivacy.Public.ToString();
        }

        private static bool IsMember(MatchRoom room, int playerId)
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

        private PlayerStatus BuildPlayerStatus(MatchRoom room, RoomMember member, Enums.PlayerPossibleStatus status)
        {
            return new PlayerStatus
            {
                RoomId = room.match_room_id,
                Username = member.Username,
                Status = status.ToString(),
                IsPlayerOne = member.IsPlayerOne
            };
        }

        private static bool TryParsePlayerStatus(string status, out PlayerPossibleStatus result)
        {
            result = PlayerPossibleStatus.NotReady;
            return !string.IsNullOrWhiteSpace(status)
                && Enum.TryParse(status.Trim(), true, out result)
                && Enum.IsDefined(typeof(PlayerPossibleStatus), result);
        }

        private static RoomPrivacy? ParsePrivacy(string privacy)
        {
            RoomPrivacy result;
            if (!string.IsNullOrWhiteSpace(privacy)
                && Enum.TryParse(privacy.Trim(), true, out result)
                && Enum.IsDefined(typeof(RoomPrivacy), result))
            {
                return result;
            }

            return null;
        }

        private int? FindPrivacyId(MasterMindEntities dbContext, RoomPrivacy privacy)
        {
            var catalog = dbContext.PrivacyCatalog
                .FirstOrDefault(entity => entity.privacy == privacy.ToString());

            int id = 0;

            if (catalog != null)
            {
                id = catalog.privacy_id;
            }
            return id;
        }

        private int? FindGameModeId(MasterMindEntities dbContext, string gamemode)
        {
            if (string.IsNullOrWhiteSpace(gamemode))
            {
                return null;
            }

            var catalog = dbContext.GameModesCatalog
                .FirstOrDefault(entity => entity.gamemode == gamemode.Trim());

            int id;

            if (catalog == null)
            {
                id = 0;
            }
            else
            {
                id = catalog.gamemode_id;
            }

            return id;
        }

        private int? FindDifficultyId(MasterMindEntities dbContext, string difficulty)
        {
            if (string.IsNullOrWhiteSpace(difficulty))
            {
                return null;
            }

            var catalog = dbContext.DifficultiesCatalog
                .FirstOrDefault(entity => entity.difficulty == difficulty.Trim());
            return catalog?.difficulty_id;
        }

        private static IQueryable<MatchRoom> RoomsWithDetails(MasterMindEntities dbContext)
        {
            return dbContext.MatchRoom
                .Include(room => room.Player)
                .Include(room => room.Player1)
                .Include(room => room.GameModesCatalog)
                .Include(room => room.DifficultiesCatalog)
                .Include(room => room.PrivacyCatalog);
        }

        private static IQueryable<MatchRoom> RoomsForQuery(MasterMindEntities dbContext, bool forUpdate)
        {
            var rooms = RoomsWithDetails(dbContext);
            return forUpdate ? rooms : rooms.AsNoTracking();
        }

        private static MatchRoom GetRoomEntityByName(MasterMindEntities dbContext, string roomName, bool forUpdate)
        {
            if (string.IsNullOrWhiteSpace(roomName))
            {
                return null;
            }

            var name = roomName.Trim();
            return RoomsForQuery(dbContext, forUpdate).FirstOrDefault(room => room.match_room_name == name);
        }

        private static MatchRoom GetRoomEntityById(MasterMindEntities dbContext, int roomId, bool forUpdate)
        {
            return RoomsForQuery(dbContext, forUpdate).FirstOrDefault(room => room.match_room_id == roomId);
        }

        private static MatchRoom GetRoomEntityByCode(MasterMindEntities dbContext, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var roomCode = code.Trim();
            var roomEntity = RoomsForQuery(dbContext, false).FirstOrDefault(room => room.room_code == roomCode);

            return roomEntity != null && !IsPublic(roomEntity) ? roomEntity : null;
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

        private RoomCreationResult CreateEmptyRoomResult(RoomResult status)
        {
            return new RoomCreationResult
            {
                Status = status.ToString(),
                Room = new Room { RoomId = 0 }
            };
        }

        private static string GenerateRoomCode(MasterMindEntities dbContext)
        {
            for (var attempt = 0; attempt < RoomCodeAttempts; attempt++)
            {
                var code = BuildRandomRoomCode();
                if (!dbContext.MatchRoom.Any(room => room.room_code == code))
                {
                    return code;
                }
            }

            return null;
        }

        private static string BuildRandomRoomCode()
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
    }
}
