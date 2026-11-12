using log4net;
using MasterMind_Server.Data;
using MasterMind_Server.Repositories.Dtos;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;
using System.Data.Entity.Core;
using System.Linq;

namespace MasterMind_Server.Repositories
{
    internal class PlayerRepository : IPlayerRepository
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(PlayerRepository));
        private readonly MasterMindEntities Context = new MasterMindEntities(true);

        public string CreatePlayer(Player player)
        {
            try
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
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
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
            try
            {
                var player = Context.Player.FirstOrDefault(p => p.username == username);

                return player;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public Player GetPlayerById(int playerId)
        {
            try
            {
                var player = Context.Player.FirstOrDefault(p => p.player_id == playerId);

                return player;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public PlayerRecordsDto GetPlayerRecords(string username)
        {
            try
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
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public string UpdatePlayer(Player player)
        {
            var formerPlayer = GetPlayerById(player.player_id);
            string validationResult = ValidateUpdateData(player, formerPlayer.player_id);

            if (formerPlayer != null)
            {
                if (validationResult.Equals(RepositoriesMessages.ValidInfo.ToString()))
                {
                    formerPlayer.username = player.username;
                    formerPlayer.email = player.email;

                    try
                    {
                        Context.SaveChanges();
                        return RepositoriesMessages.Success.ToString();
                    }
                    catch (EntityException ex)
                    {
                        logger.Error(ex);
                        throw;
                    }
                }
                else
                {
                    validationResult = RepositoriesMessages.PlayerNotFound.ToString();
                }
            }

            return validationResult;
        }

        public string UpdatePassword(string username, string password)
        {
            try
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
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public string UpdateAvatarPath(int playerId, string fileName)
        {
            try
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
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public string ReportPlayer(ReportDto report)
        {
            try
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
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        private PlayerReport MakePlayerReport(ReportDto report, int[] playerIds)
        {
            ReportReasonCatalog inappropriateLanguageEntity;
            int inappropriateLanguageId;
            ReportReasonCatalog cheatingEntity;
            int cheatingId;
            try
            {
                inappropriateLanguageEntity = Context.ReportReasonCatalog.FirstOrDefault(
                    r => r.reason == ReportReason.InappropriateLanguage
                    .ToString());

                cheatingEntity = Context.ReportReasonCatalog.FirstOrDefault(
                    r => r.reason == ReportReason.Cheating
                    .ToString());
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw;
            }

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
