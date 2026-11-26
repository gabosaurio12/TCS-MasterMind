using log4net;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.ClassHelpers;
using MasterMind_Server.Repositories.Interfaces;
using System;
using System.Collections.Generic;

namespace MasterMind_Server.Services.MatchHelpers
{
    internal class MatchRestorer
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MatchRestorer));
        private readonly IMatchRepository matchRepository = new MatchRepository();
        private readonly IMatchRoundStateRepository stateRepository = new MatchRoundStateRepository();
        private readonly MatchRoundCoordinator coordinator = new MatchRoundCoordinator();

        public void RestoreAll()
        {
            foreach (var record in stateRepository.GetAll())
            {
                Restore(record);
            }
        }

        private void Restore(MatchRoundRecord record)
        {
            MatchRoomInfo info;
            try
            {
                info = matchRepository.GetRoomInfo(record.RoomId);
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn($"Room {record.RoomId} no longer exists, its match state is discarded.", ex);
                stateRepository.Delete(record.RoomId);
                return;
            }

            if (info.PlayerOneUsername == null || info.PlayerTwoUsername == null)
            {
                stateRepository.Delete(record.RoomId);
                return;
            }

            Register(record, info);
        }

        private void Register(MatchRoundRecord record, MatchRoomInfo info)
        {
            var points = new Dictionary<string, int>
            {
                [info.PlayerOneUsername] = record.PlayerOnePoints,
                [info.PlayerTwoUsername] = record.PlayerTwoPoints
            };
            MatchCallbackRegistry.RestoreMatch(info.RoomId, new MatchState(record.RoundNumber, points));
            MatchCallbackRegistry.RestoreRound(info.RoomId, RoundState.Restore(record, info));
            if (info.IsTimeTrial && record.DeadlineUtc.HasValue)
            {
                coordinator.ResumeClock(info.RoomId, record.DeadlineUtc.Value);
            }

            logger.InfoFormat("Match of room {0} restored at round {1}.", info.RoomId, record.RoundNumber);
        }
    }
}
