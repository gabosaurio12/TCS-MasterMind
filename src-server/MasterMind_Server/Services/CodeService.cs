using DotNetEnv;
using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Services.ErrorHelpers;
using System;
using System.IO;
using System.Data.Entity.Core;
using System.Net;
using System.Net.Mail;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class CodeService : ICodeService
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(CodeService));

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
                    SendEmail(email, code);
                }
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(DbFaultClassifier.Classify(ex));
            }
        }

        private static void SendEmail(string email, string code)
        {
            using (MailMessage emailMessage = BuildMessage(email, code))
            using (SmtpClient smtp = BuildSmtpClient())
            {
                try
                {
                    smtp.Send(emailMessage);
                }
                catch (SmtpFailedRecipientException ex)
                {
                    logger.Warn(ex);
                    throw BuildFault(ErrorMessages.SendEmailError);
                }
                catch (SmtpException ex)
                {
                    logger.Error(ex);
                    throw BuildFault(ErrorMessages.SmtpServerError);
                }
            }
        }

        private static MailMessage BuildMessage(string email, string code)
        {
            MailAddress to = ParseRecipient(email);
            MailAddress from = ReadSmtpSetting(
                () => new MailAddress(Environment.GetEnvironmentVariable("SMTP_SENDER")));

            return new MailMessage(from, to)
            {
                Subject = "Verification Code!",
                Body = code
            };
        }

        private static MailAddress ParseRecipient(string email)
        {
            try
            {
                return new MailAddress(email);
            }
            catch (FormatException ex)
            {
                logger.Warn(ex);
                throw BuildFault(ErrorMessages.SendEmailError);
            }
        }

        private static SmtpClient BuildSmtpClient()
        {
            return ReadSmtpSetting(() => new SmtpClient()
            {
                Host = Environment.GetEnvironmentVariable("SMTP_SERVER"),
                Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    Environment.GetEnvironmentVariable("SMTP_LOGIN"),
                    Environment.GetEnvironmentVariable("SMTP_KEY")),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true
            });
        }

        private static T ReadSmtpSetting<T>(Func<T> build)
        {
            try
            {
                Env.Load();
                return build();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException || ex is IOException)
            {
                logger.Error("The SMTP configuration is missing or invalid.", ex);
                throw BuildFault(ErrorMessages.SmtpServerError);
            }
        }

        private static FaultException<ErrorMessage> BuildFault(ErrorMessages reason)
        {
            return new FaultException<ErrorMessage>(new ErrorMessage { Message = reason.ToString() });
        }
    }
}
