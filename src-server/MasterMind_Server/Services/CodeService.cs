using DotNetEnv;
using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using System;
using System.Net;
using System.Net.Mail;
using System.ServiceModel;

namespace MasterMind_Server.Services
{
    internal class CodeService : ICodeService
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(CodeService));

        public (bool, Player) AuthVerificationCode(string username, string code)
        {
            var authResult = VerificationCodeRepository.AuthVerificationCode(username, code);

            if (!authResult.Item1)
            {
                var clientMessage = new ErrorMessage
                {
                    Message = authResult.Item2
                };
                throw new FaultException<ErrorMessage>(clientMessage);
            }

            var playerEntity = PlayerRepository.GetPlayerByUsername(username);
            var player = new Player
            {
                PlayerId = playerEntity.player_id,
                Username = playerEntity.username,
                Email = playerEntity.email,
                Password = playerEntity.password
            };

            return (authResult.Item1, player);
        }

        public void SendVerificationCode(int playerId)
        {
            var result = VerificationCodeRepository.CreateVerificationCode(playerId);

            if (result.Item1)
            {
                string email = PlayerRepository.GetPlayerById(playerId).email;

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
            MailAddress from = new MailAddress(Environment.GetEnvironmentVariable("EMAIL_USER"));

            MailMessage emailMessage = new MailMessage(from, to)
            {
                Subject = "Verification Code!",
                Body = code
            };

            SmtpClient smtp = new SmtpClient()
            {
                Host = Environment.GetEnvironmentVariable("SMTP_SERVER"),
                Port = 587,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    Environment.GetEnvironmentVariable("EMAIL_USER"),
                    Environment.GetEnvironmentVariable("EMAIL_PASSWORD")),
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
