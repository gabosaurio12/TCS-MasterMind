using DotNetEnv;
using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using System;
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
            catch (EntityException)
            {
                var clientMessage = new ErrorMessage
                {
                    Message = ErrorMessagesEnum.DataBaseError.ToString()
                };
                throw new FaultException<ErrorMessage>(clientMessage);
            }
            
        }

        public void SendVerificationCode(int playerId)
        {
            var result = VerificationCodeRepository.CreateVerificationCode(playerId);

            if (result.Item1)
            {
                string email = new PlayerRepository().GetPlayerById(playerId).email;

                if (!String.IsNullOrWhiteSpace(email))
                {
                    SendEmail(email, result.Item2);
                }
            }
        }

        private bool SendEmail(string email, string code)
        {
            Env.Load();
            MailAddress to = new MailAddress(email);
            MailAddress from = new MailAddress(Environment.GetEnvironmentVariable("SMTP_SENDER"));

            MailMessage emailMessage = new MailMessage(from, to)
            {
                Subject = "Verification Code!",
                Body = code
            };

            SmtpClient smtp = new SmtpClient()
            {
                Host = Environment.GetEnvironmentVariable("SMTP_SERVER"),
                Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    Environment.GetEnvironmentVariable("SMTP_LOGIN"),
                    Environment.GetEnvironmentVariable("SMTP_KEY")),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true
            };

            try
            {
                smtp.Send(emailMessage);
                return true;
            }
            catch (SmtpException ex)
            {
                logger.Error(ex);
            }

            return false;
        }
    }
}
