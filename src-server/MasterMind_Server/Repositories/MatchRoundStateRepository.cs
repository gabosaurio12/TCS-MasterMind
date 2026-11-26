using MasterMind_Server.Data;
using MasterMind_Server.Repositories.ClassHelpers;
using MasterMind_Server.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MasterMind_Server.Repositories
{
    internal class MatchRoundStateRepository : IMatchRoundStateRepository
    {
        public void Save(MatchRoundRecord record)
        {
            using (var context = new MasterMindEntities(true))
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var row = context.MatchRoundState.FirstOrDefault(state => state.match_room_id == record.RoomId);
                if (row == null)
                {
                    row = new MatchRoundState { match_room_id = record.RoomId };
                    context.MatchRoundState.Add(row);
                }

                CopyValues(record, row);
                context.SaveChanges();
                transaction.Commit();
            }
        }

        public void Delete(int roomId)
        {
            using (var context = new MasterMindEntities(true))
            {
                var row = context.MatchRoundState.FirstOrDefault(state => state.match_room_id == roomId);
                if (row != null)
                {
                    context.MatchRoundState.Remove(row);
                    context.SaveChanges();
                }
            }
        }

        public IReadOnlyList<MatchRoundRecord> GetAll()
        {
            using (var context = new MasterMindEntities(true))
            {
                return context.MatchRoundState.AsNoTracking().ToList().Select(ToRecord).ToList();
            }
        }

        private static void CopyValues(MatchRoundRecord record, MatchRoundState row)
        {
            row.round_number = record.RoundNumber;
            row.player_one_points = record.PlayerOnePoints;
            row.player_two_points = record.PlayerTwoPoints;
            row.started_at_utc = record.StartedAtUtc;
            row.deadline_utc = record.DeadlineUtc;
            row.finished_username = record.FinishedUsername;
            row.finished_solved = record.FinishedUsername == null ? (bool?)null : record.FinishedSolved;
            row.finished_tries = record.FinishedUsername == null ? (int?)null : record.FinishedTries;
        }

        private static MatchRoundRecord ToRecord(MatchRoundState row)
        {
            return new MatchRoundRecord
            {
                RoomId = row.match_room_id,
                RoundNumber = row.round_number,
                PlayerOnePoints = row.player_one_points,
                PlayerTwoPoints = row.player_two_points,
                StartedAtUtc = row.started_at_utc,
                DeadlineUtc = row.deadline_utc,
                FinishedUsername = row.finished_username,
                FinishedSolved = row.finished_solved ?? false,
                FinishedTries = row.finished_tries ?? 0
            };
        }
    }
}
