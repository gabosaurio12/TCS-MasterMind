using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.ClassHelpers;
using MasterMind_Server.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace MasterMind_Server.Services.MatchHelpers
{
    internal class MatchRoundCoordinator
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MatchRoundCoordinator));
        private readonly IMatchRepository matchRepository = new MatchRepository();
        private readonly IRecordsRepository recordsRepository = new RecordsRepository();
        private readonly MatchRoundPersistence persistence = new MatchRoundPersistence();

        public void BeginMatch(int roomId)
        {
            MatchRoundScheduler.Cancel(roomId);
            MatchCallbackRegistry.StartMatch(roomId);
            MatchCallbackRegistry.OpenRound(roomId);
            persistence.Save(matchRepository.GetRoomInfo(roomId));
        }

        public void Resolve(MatchRoomInfo info, RoundResult result)
        {
            MatchRoundScheduler.Cancel(info.RoomId);
            RecordWinnerTime(info, result);
            var bestTries = MatchCallbackRegistry.ApplyRoundOutcome(info.RoomId, result);
            if (result.IsMatchOver)
            {
                FinishMatch(info, bestTries);
            }
            else
            {
                matchRepository.SaveSecretCode(info.RoomId, SecretCodeGenerator.Generate());
                MatchCallbackRegistry.OpenRound(info.RoomId);
                persistence.Save(info);
            }

            NotifyBoth(info, callback => callback.OnRoundResolvedCallback(result));
        }

        public void MarkReady(MatchRoomInfo info, string username)
        {
            var duration = TimeSpan.FromSeconds(info.RoundSeconds);
            if (!MatchCallbackRegistry.TryStartClock(info.RoomId, username, duration))
            {
                return;
            }

            MatchRoundScheduler.Start(info.RoomId, duration, ExpireRoom);
            persistence.Save(info);
            NotifyBoth(info, callback =>
                callback.OnTimeTrialStartedCallback(MatchCallbackRegistry.GetRemainingMs(info.RoomId)));
        }

        public void ResumeClock(int roomId, DateTime deadlineUtc)
        {
            var remaining = deadlineUtc - DateTime.UtcNow;
            MatchRoundScheduler.Start(roomId, remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining, ExpireRoom);
        }

        public void OnTimeExpired(int roomId)
        {
            try
            {
                var info = matchRepository.GetRoomInfo(roomId);
                var result = MatchCallbackRegistry.ExpireRound(roomId, info.PlayerOneUsername, info.PlayerTwoUsername);
                if (!RoundState.IsPending(result))
                {
                    Resolve(info, result);
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
            }
        }

        private static void ExpireRoom(int roomId)
        {
            new MatchRoundCoordinator().OnTimeExpired(roomId);
        }

        private static void NotifyBoth(MatchRoomInfo info, Action<IMatchCallbackService> notify)
        {
            MatchCallbackRegistry.Notify(info.PlayerOneUsername, notify);
            MatchCallbackRegistry.Notify(info.PlayerTwoUsername, notify);
        }

        private static void RecordWinnerTime(MatchRoomInfo info, RoundResult result)
        {
            var round = MatchCallbackRegistry.GetRound(info.RoomId);
            if (info.IsTimeTrial && round != null && !string.IsNullOrEmpty(result.WinnerUsername))
            {
                MatchCallbackRegistry.GetMatch(info.RoomId)
                    ?.RecordWinnerSeconds(result.WinnerUsername, round.GetElapsedSeconds());
            }
        }

        private void FinishMatch(MatchRoomInfo info, IReadOnlyDictionary<string, int> bestTries)
        {
            persistence.Delete(info.RoomId);
            if (info.IsTimeTrial)
            {
                SaveTimeRecords(MatchCallbackRegistry.GetMatch(info.RoomId)?.GetBestSeconds());
                return;
            }

            SaveRecords(bestTries);
        }

        private void SaveTimeRecords(IReadOnlyDictionary<string, int> bestSeconds)
        {
            try
            {
                recordsRepository.SaveTimeRecords(bestSeconds);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
            }
        }

        private void SaveRecords(IReadOnlyDictionary<string, int> bestTries)
        {
            try
            {
                recordsRepository.SaveTriesRecords(bestTries);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
            }
        }
    }
}
