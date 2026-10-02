using log4net;
using MasterMind_Client.Data;
using MasterMind_Client.TempData.DTO;
using MasterMind_Client.TempData.Enum;
using System.Data.Entity.Core;
using System.Linq;

namespace MasterMind_Client.TempData
{
    public static class TempPlayerService
    {
        private readonly static MasterMindEntities Context = new MasterMindEntities(true);

        private readonly static ILog logger = LogManager.GetLogger(typeof(TempPlayerService));
        private static int inappropriateLanguageId;
        private static int cheatingId;
        private static readonly object catalogLock = new object();

        private static void EnsureCatalogsLoaded()
        {
            if (inappropriateLanguageId != 0) return;

            lock (catalogLock)
            {
                if (inappropriateLanguageId != 0) return;

                using (var context = new MasterMindEntities(true))
                {
                    inappropriateLanguageId = context.ReportReasonCatalog
                        .FirstOrDefault(r => r.reason == ReportReasonEnum.InappropriateLanguage.ToString()).report_reason_id;

                    cheatingId = context.ReportReasonCatalog
                        .FirstOrDefault(r => r.reason == ReportReasonEnum.Cheating.ToString()).report_reason_id;
                }
            }
        }

        public static void UpdatePlayer(Player updatedPlayer)
        {
            try
            {
                var player = Context.Player.FirstOrDefault(p => p.player_id == updatedPlayer.player_id);

                if (player != null)
                {
                    player.username = updatedPlayer.username;
                    player.email = updatedPlayer.email;

                    Context.SaveChanges();

                    CurrentPlayer.Instance.SetCurrentPlayer(player);
                }

            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }
            
        }

        public static Player GetPlayerByUsername(string username)
        {
            try
            {
                var player = Context.Player.FirstOrDefault(p => p.username == username);
                if (player != null)
                {
                    return player;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return null;
        }

        public static Player GetPlayerById(int playerId)
        {
            try
            {
                var player = Context.Player.FirstOrDefault(p => p.player_id == playerId);
                if (player != null)
                {
                    return player;
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }

            return null;
        }

        public static void ReportPlayer(PlayerReportDto report)
        {
            try
            {
                EnsureCatalogsLoaded();

                var reportedPlayer = GetPlayerByUsername(report.ReportedPlayerUsername);
                if (reportedPlayer != null)
                {
                    var reportedByPlayer = GetPlayerByUsername(report.ReportedByPlayerUsername);
                    if (reportedByPlayer != null)
                    {
                        int reasonId;
                        if (report.Reason == ReportReasonEnum.InappropriateLanguage)
                        {
                            reasonId = inappropriateLanguageId;
                        }
                        else
                        {
                            reasonId = cheatingId;
                        }
                        Context.PlayerReport.Add(new PlayerReport
                        {
                            reported_player_id = reportedPlayer.player_id,
                            reported_by_player_id = reportedByPlayer.player_id,
                            reason_id = reasonId
                        });

                        Context.SaveChanges();
                    }
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }
        }
    }
}
