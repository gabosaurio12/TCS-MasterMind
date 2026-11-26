using log4net;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.ClassHelpers;
using MasterMind_Server.Repositories.Interfaces;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace MasterMind_Server.Services.MatchHelpers
{
    internal class MatchRoundPersistence
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MatchRoundPersistence));
        private readonly IMatchRoundStateRepository stateRepository = new MatchRoundStateRepository();

        public void Save(MatchRoomInfo info)
        {
            var match = MatchCallbackRegistry.GetMatch(info.RoomId);
            var round = MatchCallbackRegistry.GetRound(info.RoomId);
            if (match == null || round == null)
            {
                return;
            }

            var finished = round.GetFirstFinished();
            Execute(() => stateRepository.Save(new MatchRoundRecord
            {
                RoomId = info.RoomId,
                RoundNumber = match.RoundNumber,
                PlayerOnePoints = match.GetPoints(info.PlayerOneUsername),
                PlayerTwoPoints = match.GetPoints(info.PlayerTwoUsername),
                StartedAtUtc = round.StartedAtUtc,
                DeadlineUtc = round.DeadlineUtc,
                FinishedUsername = finished?.Username,
                FinishedSolved = finished?.Solved ?? false,
                FinishedTries = finished?.TriesUsed ?? 0
            }));
        }

        public void Delete(int roomId)
        {
            Execute(() => stateRepository.Delete(roomId));
        }

        private static void Execute(System.Action operation)
        {
            try
            {
                operation();
            }
            catch (EntityException ex)
            {
                logger.Error("The match state could not be persisted.", ex);
            }
            catch (DbUpdateException ex)
            {
                logger.Error("The match state could not be persisted.", ex);
            }
            catch (SqlException ex)
            {
                logger.Error("The match state could not be persisted.", ex);
            }
        }
    }
}
