using MasterMind_Server.Data;
using MasterMind_Server.Repositories.ClassHelpers;
using MasterMind_Server.Repositories.Dtos;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MasterMind_Server.Repositories
{
    internal class PlayerRepository : IPlayerRepository
    {
        internal const byte OnlineValue = 1;
        private const byte OfflineValue = 0;
        private const int NoReasonId = 0;

        private readonly MasterMindEntities Context = new MasterMindEntities(true);

        public string CreatePlayer(Player player)
        {
            var validationResult = ValidateData(player);

            if (validationResult.Equals(RepositoriesMessages.ValidInfo.ToString()))
            {
                player.password = BCrypt.Net.BCrypt.HashPassword(player.password);

                Context.Player.Add(player);
                Context.SaveChanges();

                validationResult = RepositoriesMessages.Success.ToString();
            }

            return validationResult;
        }

        private string ValidateData(Player player)
        {
            if (Context.Player.Any(p => p.username == player.username))
            {
                return RepositoriesMessages.UsernameTaken.ToString();
            }
            if (Context.Player.Any(p => p.email == player.email))
            {
                return RepositoriesMessages.EmailTaken.ToString();
            }

            return RepositoriesMessages.ValidInfo.ToString();
        }

        private string ValidateUpdateData(Player player, int playerId)
        {
            if (Context.Player.Any(p => p.username == player.username && p.player_id != playerId))
            {
                return RepositoriesMessages.UsernameTaken.ToString();
            }
            if (Context.Player.Any(p => p.email == player.email && p.player_id != playerId))
            {
                return RepositoriesMessages.EmailTaken.ToString();
            }

            return RepositoriesMessages.ValidInfo.ToString();
        }

        public Player GetPlayerByUsername(string username)
        {
            var player = Context.Player.FirstOrDefault(p => p.username == username);

            return player;
        }

        public Player GetPlayerById(int playerId)
        {
            var player = Context.Player.FirstOrDefault(p => p.player_id == playerId);

            return player;
        }

        public PlayerRecordsDto GetPlayerRecords(string username)
        {
            var player = GetPlayerByUsername(username);
            if (player != null)
            {
                var triesRecords = Context.PlayerTriesRecords
                    .Where(tr => tr.player_id == player.player_id && tr.tries_record.HasValue)
                    .OrderBy(gr => gr.rank)
                    .Select(gr => gr.tries_record.Value)
                    .ToArray();
                var timeTrialRecords = Context.PlayerTimeTrialRecords
                    .Where(gr => gr.player_id == player.player_id && gr.time_record.HasValue)
                    .OrderBy(gr => gr.rank)
                    .Select(gr => gr.time_record.Value)
                    .ToArray();

                return new PlayerRecordsDto
                {
                    PlayerUsername = player.username,
                    PlayerTriesRecords = triesRecords,
                    PlayerTimeTrialRecords = timeTrialRecords
                };
            }

            return null;
        }

        public string UpdatePlayer(Player player)
        {
            var formerPlayer = GetPlayerById(player.player_id);
            string validationResult;

            if (formerPlayer != null)
            {
                validationResult = ValidateUpdateData(player, formerPlayer.player_id);
                if (validationResult.Equals(RepositoriesMessages.ValidInfo.ToString()))
                {
                    formerPlayer.username = player.username;
                    formerPlayer.email = player.email;

                    Context.SaveChanges();
                    return RepositoriesMessages.Success.ToString();
                }
            }
            else
            {
                validationResult = RepositoriesMessages.PlayerNotFound.ToString();
            }

            return validationResult;
        }

        public string UpdatePassword(string username, string password)
        {
            var player = GetPlayerByUsername(username);
            if (player != null)
            {
                player.password = BCrypt.Net.BCrypt.HashPassword(password);

                Context.SaveChanges();

                return RepositoriesMessages.Success.ToString();
            }

            return RepositoriesMessages.PlayerNotFound.ToString();
        }

        public string UpdateAvatarPath(int playerId, string fileName)
        {
            var player = GetPlayerById(playerId);

            if (player == null)
            {
                return RepositoriesMessages.PlayerNotFound.ToString();
            }

            player.avatar_path = fileName;

            Context.SaveChanges();

            return RepositoriesMessages.Success.ToString();
        }

        public string UpdateOnlineStatus(int playerId, bool isOnline)
        {
            using (var context = new MasterMindEntities(true))
            {
                var player = context.Player.FirstOrDefault(p => p.player_id == playerId);

                if (player == null)
                {
                    return RepositoriesMessages.PlayerNotFound.ToString();
                }

                player.is_online = isOnline ? OnlineValue : OfflineValue;
                context.SaveChanges();

                return RepositoriesMessages.Success.ToString();
            }
        }

        public void ResetAllOnlineStatus()
        {
            using (var context = new MasterMindEntities(true))
            {
                var onlinePlayers = context.Player.Where(p => p.is_online == OnlineValue).ToList();

                foreach (var player in onlinePlayers)
                {
                    player.is_online = OfflineValue;
                }

                context.SaveChanges();
            }
        }

        public string CreateGuest(string username)
        {
            if (!GuestNamePolicy.IsValid(username))
            {
                return RepositoriesMessages.InvalidGuestName.ToString();
            }

            using (var context = new MasterMindEntities(true))
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                if (context.Player.Any(p => p.username == username))
                {
                    return RepositoriesMessages.UsernameTaken.ToString();
                }

                context.Player.Add(GuestNamePolicy.BuildGuest(username, OnlineValue));
                context.SaveChanges();
                transaction.Commit();

                return RepositoriesMessages.Success.ToString();
            }
        }

        public string DeleteGuest(int playerId)
        {
            using (var context = new MasterMindEntities(true))
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var guest = context.Player.FirstOrDefault(p => p.player_id == playerId && p.is_guest);
                if (guest == null)
                {
                    return RepositoriesMessages.PlayerNotFound.ToString();
                }

                var reports = context.PlayerReport.Where(
                    r => r.reported_player_id == playerId || r.reported_by_player_id == playerId);
                context.PlayerReport.RemoveRange(reports);
                context.Player.Remove(guest);
                context.SaveChanges();
                transaction.Commit();

                return RepositoriesMessages.Success.ToString();
            }
        }

        public bool IsGuest(string username)
        {
            using (var context = new MasterMindEntities(true))
            {
                return context.Player.Any(p => p.username == username && p.is_guest);
            }
        }

        public string GetGuestUsername(int playerId)
        {
            using (var context = new MasterMindEntities(true))
            {
                return context.Player
                    .Where(p => p.player_id == playerId && p.is_guest)
                    .Select(p => p.username)
                    .FirstOrDefault();
            }
        }

        public List<int> GetOfflineGuestIds()
        {
            using (var context = new MasterMindEntities(true))
            {
                return context.Player
                    .Where(p => p.is_guest && p.is_online != OnlineValue)
                    .Select(p => p.player_id)
                    .ToList();
            }
        }

        public string ReportPlayer(ReportDto report)
        {
            var reportedPlayer = GetPlayerByUsername(report.ReportedPlayerUsername);
            var reportedByPlayer = GetPlayerByUsername(report.ReportedByPlayerUsername);

            if (reportedPlayer == null || reportedByPlayer == null)
            {
                return RepositoriesMessages.PlayerNotFound.ToString();
            }

            if (reportedPlayer.player_id == reportedByPlayer.player_id)
            {
                return RepositoriesMessages.SelfReport.ToString();
            }

            var reasonId = GetReportReasonId(report.Reason);

            if (reasonId == NoReasonId)
            {
                return RepositoriesMessages.InvalidReportReason.ToString();
            }

            return SaveReport(reportedPlayer.player_id, reportedByPlayer.player_id, reasonId);
        }

        private int GetReportReasonId(string reason)
        {
            if (!Enum.GetNames(typeof(ReportReason)).Contains(reason))
            {
                return NoReasonId;
            }

            var reasonEntity = Context.ReportReasonCatalog.FirstOrDefault(r => r.reason == reason);

            return reasonEntity?.report_reason_id ?? NoReasonId;
        }

        private void BlockIfLimitReached(int reportedPlayerId)
        {
            var reportedPlayer = Context.Player.First(p => p.player_id == reportedPlayerId);
            Context.Entry(reportedPlayer).Reload();
            ReportBlockPolicy.StartBlockIfLimitReached(Context, reportedPlayer);
        }

        private string SaveReport(int reportedPlayerId, int reportedByPlayerId, int reasonId)
        {
            using (var transaction = Context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var alreadyReported = Context.PlayerReport.Any(
                    r => r.reported_player_id == reportedPlayerId && r.reported_by_player_id == reportedByPlayerId
                    && r.reason_id == reasonId);

                if (alreadyReported)
                {
                    return RepositoriesMessages.AlreadyReported.ToString();
                }

                Context.PlayerReport.Add(new PlayerReport
                {
                    reported_player_id = reportedPlayerId,
                    reported_by_player_id = reportedByPlayerId,
                    reason_id = reasonId
                });
                Context.SaveChanges();
                BlockIfLimitReached(reportedPlayerId);
                transaction.Commit();

                return RepositoriesMessages.Success.ToString();
            }
        }
    }
}
