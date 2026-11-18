using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Dtos;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;
using System.Linq;

namespace MasterMind_Server.Repositories
{
    internal class PlayerRepository : IPlayerRepository
    {
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
                    .Where(tr => tr.player_id == player.player_id)
                    .OrderBy(gr => gr.rank)
                    .Select(gr => gr.rank)
                    .ToArray();
                var timeTrialRecords = Context.PlayerTimeTrialRecords
                    .Where(gr => gr.player_id == player.player_id)
                    .OrderBy(gr => gr.rank)
                    .Select(gr => gr.rank)
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
            string validationResult = RepositoriesMessages.Error.ToString();

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

        public string ReportPlayer(ReportDto report)
        {
            string result = "";

            var reportedPlayer = GetPlayerByUsername(report.ReportedPlayerUsername);
            if (reportedPlayer != null)
            {
                var reportedByPlayer = GetPlayerByUsername(report.ReportedByPlayerUsername);
                if (reportedByPlayer != null)
                {
                    int[] playerIds = { reportedPlayer.player_id, reportedByPlayer.player_id };
                    var reportEntity = MakePlayerReport(report, playerIds);

                    Context.PlayerReport.Add(reportEntity);

                    Context.SaveChanges();

                    result = RepositoriesMessages.Success.ToString();
                }
                else
                {
                    result = RepositoriesMessages.PlayerNotFound.ToString();
                }
            }
            else
            {
                result = RepositoriesMessages.PlayerNotFound.ToString();
            }

            return result;
        }

        private PlayerReport MakePlayerReport(ReportDto report, int[] playerIds)
        {
            ReportReasonCatalog inappropriateLanguageEntity;
            int inappropriateLanguageId;
            ReportReasonCatalog cheatingEntity;
            int cheatingId;

            inappropriateLanguageEntity = Context.ReportReasonCatalog.FirstOrDefault(
                    r => r.reason == ReportReason.InappropriateLanguage
                    .ToString());

            cheatingEntity = Context.ReportReasonCatalog.FirstOrDefault(
                r => r.reason == ReportReason.Cheating
                .ToString());

            PlayerReport newReport = new PlayerReport();

            if (inappropriateLanguageEntity != null && cheatingEntity != null)
            {
                inappropriateLanguageId = inappropriateLanguageEntity.report_reason_id;
                cheatingId = cheatingEntity.report_reason_id;
                if (inappropriateLanguageId > 0 && cheatingId > 0)
                {
                    int reasonId;

                    if (report.Reason == ReportReason.InappropriateLanguage.ToString())
                    {
                        reasonId = inappropriateLanguageId;
                    }
                    else
                    {
                        reasonId = cheatingId;
                    }

                    newReport = new PlayerReport
                    {
                        reported_player_id = playerIds[0],
                        reported_by_player_id = playerIds[1],
                        reason_id = reasonId
                    };
                }
            }

            return newReport;
        }
    }
}
