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

                if (validationResult.Equals(RepositoriesEnum.ValidInfo.ToString()))
                {
                    player.password = BCrypt.Net.BCrypt.HashPassword(player.password);

                    Context.Player.Add(player);
                    Context.SaveChanges();

                    validationResult = RepositoriesEnum.Success.ToString();
                }

                return validationResult;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw ex;
            }
        }

        private string ValidateData(Player player)
        {
            if (Context.Player.Any(p => p.username == player.username))
            {
                return RepositoriesEnum.UsernameTaken.ToString();
            }
            if (Context.Player.Any(p => p.email == player.email))
            {
                return RepositoriesEnum.EmailTaken.ToString();
            }

            return RepositoriesEnum.ValidInfo.ToString();
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
                throw ex;
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
                throw ex;
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
                throw ex;
            }
        }

        public string UpdatePlayer(Player player)
        {
            string validationResult = ValidateData(player);
            if (validationResult.Equals(RepositoriesEnum.ValidInfo))
            {
                var formerPlayer = GetPlayerById(player.player_id);
                if (formerPlayer != null)
                {
                    formerPlayer.username = player.username;
                    formerPlayer.email = player.email;

                    try
                    {
                        Context.SaveChanges();
                        return RepositoriesEnum.Success.ToString();
                    }
                    catch (EntityException ex)
                    {
                        logger.Error(ex);
                        throw ex;
                    }
                }
                else
                {
                    validationResult = RepositoriesEnum.PlayerNotFound.ToString();
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

                    return RepositoriesEnum.Success.ToString();
                }

                return RepositoriesEnum.PlayerNotFound.ToString();
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw ex;
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

                        result = RepositoriesEnum.Success.ToString();
                    }

                    result = RepositoriesEnum.PlayerNotFound.ToString();
                }

                result = RepositoriesEnum.PlayerNotFound.ToString();

                return result;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw ex;
            }
        }

        private PlayerReport MakePlayerReport(ReportDto report, int[] playerIds)
        {
            int inappropriateLanguageId;
            int cheatingId;
            try
            {
                inappropriateLanguageId = Context.ReportReasonCatalog.FirstOrDefault(
                    r => r.reason == ReportReasonEnum.InappropriateLanguage
                    .ToString()).report_reason_id;

                cheatingId = Context.ReportReasonCatalog.FirstOrDefault(
                    r => r.reason == ReportReasonEnum.Cheating
                    .ToString()).report_reason_id;
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw ex;
            }

            int reasonId;

            if (report.Reason == ReportReasonEnum.InappropriateLanguage.ToString())
            {
                reasonId = inappropriateLanguageId;
            }
            else
            {
                reasonId = cheatingId;
            }

            return new PlayerReport
            {
                reported_player_id = playerIds[0],
                reported_by_player_id = playerIds[1],
                reason_id = reasonId
            };
        }
    }
}
