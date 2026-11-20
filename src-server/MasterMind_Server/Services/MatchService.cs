using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.ClassHelpers;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;
using MasterMind_Server.Services.MatchHelpers;
using System;
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
                throw BuildFault(ErrorMessages.DataBaseError);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw BuildFault(ErrorMessages.DataBaseError);
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
                throw BuildFault(ErrorMessages.DataBaseError);
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
                throw BuildFault(ErrorMessages.DataBaseError);
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
                throw BuildFault(ErrorMessages.DataBaseError);
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
                throw BuildFault(ErrorMessages.DataBaseError);
            }
            catch (DbUpdateException ex)
            {
                logger.Error(ex);
                throw BuildFault(ErrorMessages.DataBaseError);
            }
            catch (SqlException ex)
            {
                logger.Error(ex);
                throw BuildFault(ErrorMessages.DataBaseError);
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
            MatchCallbackRegistry.StartMatch(participation.RoomId);
            MatchCallbackRegistry.OpenRound(participation.RoomId);
            return RepositoriesMessages.Success.ToString();
        }

        private Clues EvaluateTry(string username, CodeTry codeTry)
        {
            var guess = ValidateGuess(codeTry);
            var participation = matchRepository.GetParticipation(username);
            var secretCode = matchRepository.GetSecretCode(participation.RoomId);
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
            bool outOfTries = participation.MaxTries.HasValue && triesUsed >= participation.MaxTries.Value;
            if (triesUsed == 0 || !(isCorrect || outOfTries))
            {
                return;
            }

            var progress = new OpponentProgress { Username = username, Solved = isCorrect, TriesUsed = triesUsed };
            var result = MatchCallbackRegistry.RecordFinish(participation.RoomId, progress);
            if (result == null)
            {
                MatchCallbackRegistry.Notify(
                    participation.OpponentUsername, callback => callback.OnOpponentFinishedCallback(progress));
                return;
            }

            ResolveRound(participation, username, result);
        }

        private void ResolveRound(MatchParticipation participation, string username, RoundResult result)
        {
            MatchCallbackRegistry.ApplyRoundOutcome(participation.RoomId, result);
            if (!result.IsMatchOver)
            {
                matchRepository.SaveSecretCode(participation.RoomId, SecretCodeGenerator.Generate());
                MatchCallbackRegistry.OpenRound(participation.RoomId);
            }

            MatchCallbackRegistry.Notify(username, callback => callback.OnRoundResolvedCallback(result));
            MatchCallbackRegistry.Notify(
                participation.OpponentUsername, callback => callback.OnRoundResolvedCallback(result));
        }

        private void NotifyAbandonment(string username)
        {
            var participation = matchRepository.GetParticipation(username);
            if (MatchCallbackRegistry.TryAbandonRound(participation.RoomId))
            {
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
