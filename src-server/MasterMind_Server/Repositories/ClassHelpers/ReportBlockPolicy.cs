using MasterMind_Server.Data;
using System;
using System.Linq;

namespace MasterMind_Server.Repositories.ClassHelpers
{
    internal static class ReportBlockPolicy
    {
        internal const int MaxReportsBeforeBlock = 5;
        private const int BlockDurationHours = 1;

        internal static bool IsBlocked(MasterMindEntities dbContext, int playerId)
        {
            var player = dbContext.Player.FirstOrDefault(p => p.player_id == playerId);
            if (player == null)
            {
                return false;
            }

            if (player.blocked_until.HasValue && player.blocked_until.Value <= DateTime.UtcNow)
            {
                LiftBlock(dbContext, player);
                return false;
            }

            StartBlockIfLimitReached(dbContext, player);
            return player.blocked_until.HasValue;
        }

        internal static void StartBlockIfLimitReached(MasterMindEntities dbContext, Player player)
        {
            if (player.blocked_until.HasValue)
            {
                return;
            }

            var reportCount = dbContext.PlayerReport.Count(r => r.reported_player_id == player.player_id);
            if (reportCount >= MaxReportsBeforeBlock)
            {
                player.blocked_until = DateTime.UtcNow.AddHours(BlockDurationHours);
                dbContext.SaveChanges();
            }
        }

        private static void LiftBlock(MasterMindEntities dbContext, Player player)
        {
            var reports = dbContext.PlayerReport.Where(r => r.reported_player_id == player.player_id);
            dbContext.PlayerReport.RemoveRange(reports);
            player.blocked_until = null;
            dbContext.SaveChanges();
        }
    }
}
