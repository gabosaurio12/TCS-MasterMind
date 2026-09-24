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
        private readonly static MasterMindEntities Context = new MasterMindEntities();

        private readonly static ILog logger = LogManager.GetLogger(typeof(TempPlayerService));
        private readonly static int InappropriateLanguageId = Context.ReportReasonCatalog.FirstOrDefault(
            r => r.reason == ReportReasonEnum.InappropriateLanguage.ToString()).report_reason_id;
        private readonly static int CheatingId = Context.ReportReasonCatalog.FirstOrDefault(
            r => r.reason == ReportReasonEnum.Cheating.ToString()).report_reason_id;

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

                    CurrentPlayer.Instance.SetCurrentPlayer(Context.Player.FirstOrDefault(
                        p => p.player_id == updatedPlayer.player_id));
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

        public static void ReportPlayer(PlayerReportDto report)
        {
            try
            {
                var reportedPlayer = GetPlayerByUsername(report.ReportedPlayerUsername);
                if (reportedPlayer != null)
                {
                    var reportedByPlayer = GetPlayerByUsername(report.ReportedByPlayerUsername);
                    if (reportedByPlayer != null)
                    {
                        int reasonId;
                        if (report.Reason == ReportReasonEnum.InappropriateLanguage)
                        {
                            reasonId = InappropriateLanguageId;
                        }
                        else
                        {
                            reasonId = CheatingId;
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
