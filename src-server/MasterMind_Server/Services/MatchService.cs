using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
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
            return new Clues { CluesResult = clues, IsCorrect = ClueCalculator.IsExactMatch(clues) };
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
