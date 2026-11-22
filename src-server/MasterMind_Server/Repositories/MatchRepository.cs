using MasterMind_Server.Data;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories.ClassHelpers;
using MasterMind_Server.Repositories.Interfaces;
using MasterMind_Server.Services.MatchHelpers;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace MasterMind_Server.Repositories
{
    internal class MatchRepository : IMatchRepository
    {
        private readonly MasterMindEntities context = new MasterMindEntities(true);

        public MatchParticipation GetParticipation(string username)
        {
            var player = context.Player.AsNoTracking().FirstOrDefault(p => p.username == username);
            if (player == null)
            {
                throw new InvalidOperationException(ErrorMessages.PlayerNotFound.ToString());
            }

            var room = context.MatchRoom.AsNoTracking().Include(r => r.DifficultiesCatalog).FirstOrDefault(
                r => r.player_one_id == player.player_id || r.player_two_id == player.player_id);
            if (room == null)
            {
                throw new InvalidOperationException(ErrorMessages.PlayerNotInRoom.ToString());
            }

            return new MatchParticipation
            {
                RoomId = room.match_room_id,
                IsPlayerOne = room.player_one_id == player.player_id,
                OpponentUsername = GetOpponentUsername(room, player.player_id),
                PlayerOneStatus = room.player_one_status,
                PlayerTwoStatus = room.player_two_status,
                MaxTries = TriesRules.GetMaxTries(room.DifficultiesCatalog?.difficulty)
            };
        }

        public void SaveSecretCode(int roomId, string[] code)
        {
            var room = context.MatchRoom.FirstOrDefault(r => r.match_room_id == roomId);
            if (room == null)
            {
                throw new InvalidOperationException(ErrorMessages.RoomNotFound.ToString());
            }

            var secretCode = context.SecretCode.FirstOrDefault(s => s.match_room_id == roomId);
            if (secretCode == null)
            {
                secretCode = new SecretCode { match_room_id = roomId };
                context.SecretCode.Add(secretCode);
            }

            context.Entry(room).Reload();
            secretCode.first_color = code[0];
            secretCode.second_color = code[1];
            secretCode.third_color = code[2];
            secretCode.fourth_color = code[3];
            room.player_one_tries = 0;
            room.player_two_tries = 0;
            context.SaveChanges();
        }

        public string[] GetSecretCode(int roomId)
        {
            var row = context.SecretCode.AsNoTracking()
                .Where(s => s.match_room_id == roomId)
                .Select(s => new SecretCodeRow
                {
                    FirstColor = s.first_color,
                    SecondColor = s.second_color,
                    ThirdColor = s.third_color,
                    FourthColor = s.fourth_color
                })
                .FirstOrDefault();
            if (row == null)
            {
                throw new InvalidOperationException(ErrorMessages.SecretCodeNotFound.ToString());
            }

            return row.ToArray();
        }

        public bool RegisterTry(MatchParticipation participation)
        {
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                bool result = false;
                var room = context.MatchRoom.First(r => r.match_room_id == participation.RoomId);
                context.Entry(room).Reload();
                var tries = participation.IsPlayerOne ? room.player_one_tries : room.player_two_tries;
                if (tries < participation.MaxTries)
                {
                    IncrementTries(room, participation.IsPlayerOne);
                    context.SaveChanges();
                    transaction.Commit();
                    result = true;
                }

                return result;
            }
        }

        private string GetOpponentUsername(MatchRoom room, int playerId)
        {
            int? opponentId = room.player_one_id == playerId ? room.player_two_id : room.player_one_id;
            return context.Player.AsNoTracking()
                .Where(p => p.player_id == opponentId)
                .Select(p => p.username)
                .FirstOrDefault();
        }

        private static void IncrementTries(MatchRoom room, bool isPlayerOne)
        {
            if (isPlayerOne)
            {
                room.player_one_tries++;
            }
            else
            {
                room.player_two_tries++;
            }
        }
    }
}
