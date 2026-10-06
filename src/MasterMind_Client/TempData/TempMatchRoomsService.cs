using log4net;
using MasterMind_Client.Data;
using MasterMind_Client.TempData.DTO;
using MasterMind_Client.TempData.Enum;
using System.Data.Entity.Core;
using System.Linq;
using System.Collections.Generic;

namespace MasterMind_Client.TempData
{
    public class TempMatchRoomsService
    {
        private readonly static MasterMindEntities Context = new MasterMindEntities(true);
        private readonly static ILog logger = LogManager.GetLogger(typeof(TempMatchRoomsService));

        private static Dictionary<string, int> gamemodeCatalog;
        private static Dictionary<string, int> difficultyCatalog;
        private static Dictionary<string, int> privacyCatalog;
        private static Dictionary<int, string> gamemodeCatalogReverse;
        private static Dictionary<int, string> difficultyCatalogReverse;
        private static Dictionary<int, string> privacyCatalogReverse;
        private static readonly object catalogLock = new object();

        private static void EnsureCatalogsLoaded()
        {
            if (gamemodeCatalog != null)
            {
                return;
            }

            lock (catalogLock)
            {
                if (gamemodeCatalog != null)
                {
                    return;
                }

                using (var context = new MasterMindEntities(true))
                {
                    gamemodeCatalog = context.GameModesCatalog
                        .ToDictionary(gm => gm.gamemode, gm => gm.gamemode_id);
                    gamemodeCatalogReverse = gamemodeCatalog
                        .ToDictionary(kv => kv.Value, kv => kv.Key);

                    difficultyCatalog = context.DifficultiesCatalog
                        .ToDictionary(d => d.difficulty, d => d.difficulty_id);
                    difficultyCatalogReverse = difficultyCatalog
                        .ToDictionary(kv => kv.Value, kv => kv.Key);

                    privacyCatalog = context.PrivacyCatalog
                        .ToDictionary(p => p.privacy, p => p.privacy_id);
                    privacyCatalogReverse = privacyCatalog
                        .ToDictionary(kv => kv.Value, kv => kv.Key);
                }
            }
        }

        public static MatchRoomCreationResultEnum CreateMatchRoom(MatchRoomDto matchRoomDto)
        {
            try
            {
                EnsureCatalogsLoaded();

                var searchRoom = Context.MatchRoom.FirstOrDefault(
                    room => room.match_room_name == matchRoomDto.RoomName);

                if (searchRoom == null)
                {
                    int gamemodeId = gamemodeCatalog[matchRoomDto.Gamemode];
                    int privacyId = privacyCatalog[matchRoomDto.Privacy];
                    var room = new MatchRoom
                    {
                        player_one_id = CurrentPlayer.Instance.Id,
                        gamemode_id = gamemodeId,
                        match_room_name = matchRoomDto.RoomName,
                        difficulty_id = difficultyCatalog[matchRoomDto.Difficulty],
                        room_privacy_id = privacyId
                    };

                    Context.MatchRoom.Add(room);
                    Context.SaveChanges();

                    return MatchRoomCreationResultEnum.Success;
                }
                else
                {
                    return MatchRoomCreationResultEnum.NameInUse;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return MatchRoomCreationResultEnum.Error;
        }

        public static List<MatchRoomDto> GetMatchRooms()
        {
            try
            {
                EnsureCatalogsLoaded();

                var rooms = Context.MatchRoom.Select(room => room).ToList();
                if (rooms.Count > 0)
                {
                    var availableRooms = new List<MatchRoomDto>();
                    foreach (var room in rooms)
                    {
                        availableRooms.Add(new MatchRoomDto
                        {
                            MatchRoomId = room.match_room_id,
                            RoomName = room.match_room_name,
                            Gamemode = gamemodeCatalogReverse[room.gamemode_id],
                            Difficulty = difficultyCatalogReverse[room.difficulty_id],
                            Privacy = privacyCatalogReverse[room.room_privacy_id]
                        });
                    }

                    return availableRooms;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return new List<MatchRoomDto>();
        }

        public static MatchRoomDto GetMatchRoomByName(string roomName)
        {
            try
            {
                EnsureCatalogsLoaded();
                var room = Context.MatchRoom.FirstOrDefault(matchRoom => matchRoom.match_room_name == roomName);
                if (room != null)
                {
                    return new MatchRoomDto
                    {
                        MatchRoomId = room.match_room_id,
                        RoomName = room.match_room_name,
                        Gamemode = gamemodeCatalogReverse[room.gamemode_id],
                        Difficulty = difficultyCatalogReverse[room.difficulty_id],
                        Privacy = privacyCatalogReverse[room.room_privacy_id]
                    };
                }

            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return null;
        }

        public static MatchRoomDto GetMatchRoomById(int matchRoomId)
        {
            try
            {
                EnsureCatalogsLoaded();
                var room = Context.MatchRoom.FirstOrDefault(matchRoom => matchRoom.match_room_id == matchRoomId);
                if (room != null)
                {
                    return new MatchRoomDto
                    {
                        MatchRoomId = room.match_room_id,
                        RoomName = room.match_room_name,
                        Gamemode = gamemodeCatalogReverse[room.gamemode_id],
                        Difficulty = difficultyCatalogReverse[room.difficulty_id],
                        Privacy = privacyCatalogReverse[room.room_privacy_id],
                        PlayerOneId = room.player_one_id,
                        PlayerTwoId = room.player_two_id ?? 0
                    };
                }

            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return null;
        }

        public static MatchRoomJoinResultEnum JoinMatchRoom(int matchRoomId, int playerId)
        {
            try
            {
                var room = Context.MatchRoom.FirstOrDefault(matchRoom => matchRoom.match_room_id == matchRoomId);

                if (room == null)
                {
                    return MatchRoomJoinResultEnum.Error;
                }

                if (room.player_one_id == playerId)
                {
                    return MatchRoomJoinResultEnum.OwnRoom;
                }

                if (room.player_two_id == playerId)
                {
                    return MatchRoomJoinResultEnum.AlreadyJoined;
                }

                if (room.player_two_id != null)
                {
                    return MatchRoomJoinResultEnum.RoomFull;
                }

                room.player_two_id = playerId;
                Context.SaveChanges();

                return MatchRoomJoinResultEnum.Success;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return MatchRoomJoinResultEnum.Error;
        }
    }
}
