using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MasterMind_Server.Repositories
{
    internal class RecordsRepository : IRecordsRepository
    {
        private const int MaxRecordsPerPlayer = 3;

        public void SaveTriesRecords(IReadOnlyDictionary<string, int> bestTriesByUsername)
        {
            if (bestTriesByUsername == null || bestTriesByUsername.Count == 0)
            {
                return;
            }

            using (var context = new MasterMindEntities(true))
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                foreach (var entry in bestTriesByUsername)
                {
                    SavePlayerTries(context, entry.Key, entry.Value);
                }

                transaction.Commit();
            }
        }

        public void SaveTimeRecords(IReadOnlyDictionary<string, int> bestSecondsByUsername)
        {
            if (bestSecondsByUsername == null || bestSecondsByUsername.Count == 0)
            {
                return;
            }

            using (var context = new MasterMindEntities(true))
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                foreach (var entry in bestSecondsByUsername)
                {
                    SavePlayerTime(context, entry.Key, entry.Value);
                }

                transaction.Commit();
            }
        }

        private static void SavePlayerTime(MasterMindEntities context, string username, int seconds)
        {
            var player = context.Player.FirstOrDefault(candidate => candidate.username == username);
            if (player == null || player.is_guest)
            {
                return;
            }

            var existing = context.PlayerTimeTrialRecords
                .Where(record => record.player_id == player.player_id)
                .OrderBy(record => record.rank)
                .ToList();
            var values = existing
                .Where(record => record.time_record.HasValue)
                .Select(record => record.time_record.Value)
                .Concat(new[] { seconds })
                .OrderBy(value => value)
                .Take(MaxRecordsPerPlayer)
                .ToList();
            context.PlayerTimeTrialRecords.RemoveRange(existing);
            context.SaveChanges();
            for (int index = 0; index < values.Count; index++)
            {
                context.PlayerTimeTrialRecords.Add(new PlayerTimeTrialRecords
                {
                    player_id = player.player_id,
                    time_record = values[index],
                    rank = index + 1
                });
            }

            context.SaveChanges();
        }

        private static void SavePlayerTries(MasterMindEntities context, string username, int tries)
        {
            var player = context.Player.FirstOrDefault(candidate => candidate.username == username);
            if (player == null || player.is_guest)
            {
                return;
            }

            var existing = context.PlayerTriesRecords
                .Where(record => record.player_id == player.player_id)
                .OrderBy(record => record.rank)
                .ToList();
            var values = BuildTopValues(existing, tries);

            ApplyValues(context, player.player_id, existing, values);
        }

        private static List<int> BuildTopValues(List<PlayerTriesRecords> existing, int tries)
        {
            var values = existing
                .Where(record => record.tries_record.HasValue)
                .Select(record => record.tries_record.Value)
                .ToList();
            values.Add(tries);

            return values.OrderBy(value => value).Take(MaxRecordsPerPlayer).ToList();
        }

        private static void ApplyValues(
            MasterMindEntities context, int playerId, List<PlayerTriesRecords> existing, List<int> values)
        {
            if (existing.Count > 0)
            {
                context.PlayerTriesRecords.RemoveRange(existing);
                context.SaveChanges();
            }

            for (int index = 0; index < values.Count; index++)
            {
                context.PlayerTriesRecords.Add(new PlayerTriesRecords
                {
                    player_id = playerId,
                    tries_record = values[index],
                    rank = index + 1
                });
            }

            context.SaveChanges();
        }
    }
}
