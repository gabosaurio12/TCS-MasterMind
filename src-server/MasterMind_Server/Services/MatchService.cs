using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Services.ErrorHelpers;
using MasterMind_Server.Repositories.ClassHelpers;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;
using MasterMind_Server.Services.MatchHelpers;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class MatchService : IMatchService
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MatchService));
        private readonly IMatchRepository matchRepository = new MatchRepository();
        private readonly MatchRoundCoordinator coordinator = new MatchRoundCoordinator();
        private readonly MatchRoundPersistence persistence = new MatchRoundPersistence();

        public string UpdateCode(string username)
        {
            try
            {
                return ReplaceSecretCode(username);
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex);
                throw BuildFault(ParseReason(ex.Message));
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
        }

        public int GetMaxTries(string username)
        {
            try
            {
                return matchRepository.GetParticipation(username).MaxTries;
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex);
                throw BuildFault(ParseReason(ex.Message));
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
        }

        public void Subscribe(string username)
        {
            var callback = OperationContext.Current?.GetCallbackChannel<IMatchCallbackService>();
            if (callback == null || string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            MatchCallbackRegistry.Subscribe(username, callback);
            var channel = OperationContext.Current.Channel;
            channel.Closed += (sender, e) => MatchCallbackRegistry.Unsubscribe(username, callback);
            channel.Faulted += (sender, e) => MatchCallbackRegistry.Unsubscribe(username, callback);
        }

        public void Unsubscribe(string username)
        {
            var callback = OperationContext.Current?.GetCallbackChannel<IMatchCallbackService>();
            if (callback == null || string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            MatchCallbackRegistry.Unsubscribe(username, callback);
            try
            {
                NotifyAbandonment(username);
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
        }

        public void ReadyForRound(string username)
        {
            try
            {
                var participation = matchRepository.GetParticipation(username);
                if (participation.IsTimeTrial)
                {
                    coordinator.MarkReady(matchRepository.GetRoomInfo(participation.RoomId), username);
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex);
                throw BuildFault(ParseReason(ex.Message));
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
        }

        public MatchSnapshot GetMatchSnapshot(string username)
        {
            try
            {
                var participation = matchRepository.GetParticipation(username);
                return BuildSnapshot(matchRepository.GetRoomInfo(participation.RoomId));
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex);
                throw BuildFault(ParseReason(ex.Message));
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
        }

        public Clues ValidateTry(string username, CodeTry codeTry)
        {
            try
            {
                return EvaluateTry(username, codeTry);
            }
            catch (ArgumentException ex)
            {
                logger.Warn(ex);
                throw BuildFault(ErrorMessages.InvalidCode);
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex);
                throw BuildFault(ParseReason(ex.Message));
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
        }

        private string ReplaceSecretCode(string username)
        {
            var participation = matchRepository.GetParticipation(username);
            if (!participation.IsPlayerOne)
            {
                throw new InvalidOperationException(ErrorMessages.NotPlayerOne.ToString());
            }

            if (!participation.AreBothReady)
            {
                throw new InvalidOperationException(ErrorMessages.PlayersNotReady.ToString());
            }

            matchRepository.SaveSecretCode(participation.RoomId, SecretCodeGenerator.Generate());
            coordinator.BeginMatch(participation.RoomId);
            return RepositoriesMessages.Success.ToString();
        }

        private Clues EvaluateTry(string username, CodeTry codeTry)
        {
            var guess = ValidateGuess(codeTry);
            var participation = matchRepository.GetParticipation(username);
            var secretCode = matchRepository.GetSecretCode(participation.RoomId);
            if (participation.IsTimeTrial && !MatchCallbackRegistry.IsClockRunning(participation.RoomId))
            {
                throw new InvalidOperationException(ErrorMessages.RoundNotStarted.ToString());
            }

            if (!matchRepository.RegisterTry(participation))
            {
                throw new InvalidOperationException(ErrorMessages.TriesExhausted.ToString());
            }

            var clues = ClueCalculator.Calculate(secretCode, guess);
            bool isCorrect = ClueCalculator.IsExactMatch(clues);
            ReportProgress(participation, username, isCorrect);
            return new Clues { CluesResult = clues, IsCorrect = isCorrect };
        }

        private void ReportProgress(MatchParticipation participation, string username, bool isCorrect)
        {
            int triesUsed = MatchCallbackRegistry.CountTry(participation.RoomId, username);
            bool outOfTries = triesUsed >= participation.MaxTries;
            if (triesUsed == 0 || !(isCorrect || outOfTries))
            {
                return;
            }

            var progress = new OpponentProgress { Username = username, Solved = isCorrect, TriesUsed = triesUsed };
            if (participation.IsTimeTrial)
            {
                ResolveTimeTrial(participation, progress);
                return;
            }

            var result = MatchCallbackRegistry.RecordFinish(participation.RoomId, progress);
            if (RoundState.IsPending(result))
            {
                MatchCallbackRegistry.Notify(
                    participation.OpponentUsername, callback => callback.OnOpponentFinishedCallback(progress));
                return;
            }

            coordinator.Resolve(matchRepository.GetRoomInfo(participation.RoomId), result);
        }

        private void ResolveTimeTrial(MatchParticipation participation, OpponentProgress progress)
        {
            var result = MatchCallbackRegistry.SolveFirst(
                participation.RoomId, progress, participation.OpponentUsername);
            if (!RoundState.IsPending(result))
            {
                coordinator.Resolve(matchRepository.GetRoomInfo(participation.RoomId), result);
            }
        }

        private static MatchSnapshot BuildSnapshot(MatchRoomInfo info)
        {
            var round = MatchCallbackRegistry.GetRound(info.RoomId);
            var match = MatchCallbackRegistry.GetMatch(info.RoomId);
            if (round == null || match == null)
            {
                return new MatchSnapshot { Scores = new PlayerScore[0] };
            }

            return new MatchSnapshot
            {
                IsActive = true,
                IsTimeTrial = info.IsTimeTrial,
                IsClockRunning = round.IsClockRunning,
                RemainingMs = round.GetRemainingMs(),
                RoundNumber = match.RoundNumber,
                Scores = new[] { info.PlayerOneUsername, info.PlayerTwoUsername }
                    .Select(name => new PlayerScore { Username = name, Points = match.GetPoints(name) })
                    .ToArray()
            };
        }

        private void NotifyAbandonment(string username)
        {
            var participation = matchRepository.GetParticipation(username);
            if (MatchCallbackRegistry.TryAbandonRound(participation.RoomId))
            {
                persistence.Delete(participation.RoomId);
                MatchCallbackRegistry.Notify(
                    participation.OpponentUsername, callback => callback.OnOpponentAbandonedCallback(username));
            }
        }

        private static string[] ValidateGuess(CodeTry codeTry)
        {
            int codeLength = 4;
            var code = codeTry?.Code;
            if (code == null || code.Length != codeLength || !code.All(IsValidColour))
            {
                throw new ArgumentException(ErrorMessages.InvalidCode.ToString());
            }

            return code;
        }

        private static bool IsValidColour(string colour)
        {
            return colour != null && Enum.IsDefined(typeof(Colours), colour);
        }

        private static ErrorMessages ParseReason(string message)
        {
            return Enum.TryParse<ErrorMessages>(message, out var reason) ? reason : ErrorMessages.DataBaseError;
        }

        private static FaultException<ErrorMessage> BuildFault(ErrorMessages reason)
        {
            return new FaultException<ErrorMessage>(new ErrorMessage { Message = reason.ToString() });
        }
    }
}
