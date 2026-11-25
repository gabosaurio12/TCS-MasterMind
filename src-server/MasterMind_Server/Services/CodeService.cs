using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Services.EmailHelpers;
using MasterMind_Server.Services.ErrorHelpers;
using System;
using System.Data.Entity.Core;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class CodeService : ICodeService
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(CodeService));
        private const string VerificationSubject = "Verification Code!";

        public bool AuthVerificationCode(string username, string code)
        {
            try
            {
                var authResult = VerificationCodeRepository.AuthVerificationCode(username, code);
                return authResult;
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex);
                throw new FaultException<ErrorMessage>(new ErrorMessage { Message = ex.Message });
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
            
        }

        public void SendVerificationCode(int playerId)
        {
            try
            {
                string code = VerificationCodeRepository.CreateVerificationCode(playerId);
                string email = new PlayerRepository().GetPlayerById(playerId).email;

                if (!String.IsNullOrWhiteSpace(email))
                {
                    EmailSender.Send(email, VerificationSubject, code);
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
        }

        private static FaultException<ErrorMessage> BuildFault(ErrorMessages reason)
        {
            return new FaultException<ErrorMessage>(new ErrorMessage { Message = reason.ToString() });
        }
    }
}
